import os
import threading
from openai import OpenAI
import json
import re
import time
from waitress import serve
from flask import Flask, request
import logging
# Set up logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
ModelName = "meta-llama/Llama-3.3-70B-Instruct-fast"
os.environ["OPENAI_API_KEY"] = ""
# ──────────────────────────────────────────────────────────────────────────────
# ──────────────────────────Static Helper───────────────────────────────────────
# ──────────────────────────────────────────────────────────────────────────────
def extract_keywords(response):
    keywords = []
    pattern = r"^\d+\.\s*(.+)"
    lines = response[0].split('\n')
    for line in lines:
        match = re.match(pattern, line)
        if match:
            keyword = match.group(1).strip()
            if keyword:
                keywords.append(keyword)
    return keywords
# ──────────────────────────────────────────────────────────────────────────────
def clean_text(text):
    cleaned = re.sub(r'[*",\n]', '', text)
    return re.sub(r'\s+', ' ', cleaned).strip()
# ──────────────────────────────────────────────────────────────────────────────
def _parse_assistant_text(resp):
    data = resp.to_dict()
    return data["choices"][0]["message"]["content"].strip()
# ──────────────────────────────────────────────────────────────────────────────
# ───────────────────────────Chat instance──────────────────────────────────────
# ──────────────────────────────────────────────────────────────────────────────
class ChatInstanse:
    def __init__(self, client, company_name, industry, name_of_avatar, name_of_customer, company_area, company_phone, time_of_conversation, model_max_token):
        logging.info("Creating new ChatInstanse")
        self._lock = threading.Lock()
        self.client = client  
        self.conversation_order = []
        self.prompt_tokens = 0
        self.prev_chat_answer_token = 0
        self.prev_user_prompt_token = 0
        self.total_tokens = 0
        self.model_max_token = int(model_max_token)
        self.time_of_conversation = time_of_conversation
        self.time_start = time.time()
        self.time_end = time.ctime(self.time_start + int(time_of_conversation) * 60)
        self.name_of_customer = name_of_customer
        self.name_of_avatar = name_of_avatar
        COMPANY_NAME = company_name
        INDUSTRY = industry
        COMPANY_AREA = company_area
        COMPANY_PHONE = company_phone
        self.history = [
            {
                "role": "system",
                "content": f""
            }
        ]

    def __getstate__(self):
        state = self.__dict__.copy()
        del state['client']
        del state['_lock']
        return state

    def __setstate__(self, state):
        self.__dict__.update(state)
        self.client = OpenAI(base_url="", api_key=os.environ.get("OPENAI_API_KEY"))
        self._lock = threading.Lock()

    def ConversationMenager(self):
        logging.info("Checking token limit")
        if (600 + self.total_tokens) >= self.model_max_token:
            try:
                new_max = self.model_max_token * 2
                if self.update_max_token(new_max):
                    logging.info(f"Max tokens updated to {new_max}")
            except ValueError:
                logging.error("Invalid format for max tokens update")
            return "continue"
        logging.info("Waiting for customer input")
        customer_input_prompt = input(f"customer:")
        logging.info(f"Received customer input: {customer_input_prompt}")
        if customer_input_prompt.lower() == 'quit':
            return "break"
        try:
            logging.info("Sending request to AI model")
            response = self.SendRequestForAnswer(customer_input_prompt)
            logging.info("Received response from AI model")
            start_over = self.ExtractFromJson(response)
            if start_over:
                return "False"
        except Exception as e:
            logging.error(f"An error occurred: {e}")
        return "True"

    def update_max_token(self, new_max_token):
        allowed = {512, 1024, 2048, 4096, 8192}
        if new_max_token in allowed:
            with self._lock:
                self.model_max_token = new_max_token
            return True
        return False

    def ExtractFromJson(self, json_response):
        logging.info("Extracting data from JSON response")
        try:
            data_dict = json_response.to_dict()
            data_json_str = json.dumps(data_dict, indent=4)
            data = json.loads(data_json_str)
        except Exception as e:
            logging.error(f"Error parsing JSON: {str(e)}")
            data = data_dict
        prompt_tokens = data.get('usage', {}).get('prompt_tokens', None)
        total_tokens = data.get('usage', {}).get('total_tokens', None)
        self.prev_chat_answer_token = self.total_tokens - self.prompt_tokens
        for choice in data.get('choices', []):
            message = choice.get('message', {})
            content = message.get('content', "")
        content = clean_text(content)
        if ("bye" in content.lower() or "call ended" in content.lower()):
            return False
        with self._lock:
            self.history.append({"role": "assistant", "content": content})
        self.conversation_order.append(content)
        print(f"bot: {content}")
        self.prompt_tokens = int(prompt_tokens)
        self.total_tokens = int(total_tokens)
        return True

    def SendRequestForAnswer(self, conversation_text):
        logging.info("Preparing to send request to AI model")
        with self._lock:
            self.history.append({"role": "user", "content": conversation_text})
        try:
            response = self.client.chat.completions.create(
                model = ModelName,
                max_tokens=self.model_max_token,
                temperature=0.6,
                top_p=0.85,
                presence_penalty=0.18,
                extra_body={"top_k": 50},
                messages=self.history
            )
            logging.info("Successfully received response from AI model")
            return response
        except Exception as e:
            logging.error(f"Error sending request to AI model: {str(e)}")
            raise
# ──────────────────────────────────────────────────────────────────────────────
# ───────────────────────────Post instance──────────────────────────────────────
# ──────────────────────────────────────────────────────────────────────────────
class PostGenerator:
    def __init__(self, client, model_max_token=8192):
        logging.info("Initializing PostGenerator")
        self._lock = threading.Lock()
        self.client = client
        self.model_max_token = int(model_max_token)
        self.history = [
            {
                "role": "system",
                "content": """You are a professional content creator specializing in various industries. 
                Your task is to generate fresh, engaging marketplace posts for products or services based on the provided industry,
                company name, keyword, and location. Each post must include the following fields: 
                _title, _category, _condition, _brand, _description, _productTag.

                Guidelines:
                - _title: A catchy, concise title (10-15 words) that includes the keyword and highlights the product.
                - _category: Relevant to the industry (e.g., 'Roofing Materials', 'Plumbing Services').
                - _condition: One of 'New', 'Like New', 'Good', 'Fair' (choose appropriately).
                - _brand: Use the company name for services or a realistic brand for products.
                - _description: A detailed, engaging description (50-100 words) that emphasizes quality, benefits, and the keyword. Include the location if provided.
                - _productTag: A single tag that summarizes the post.
                
                **make sure that you NEVER offer service only products for sale**
                If asked to generate keywords, produce a list of 5-10 relevant keywords for the specified industry. 
                Ensure each post is unique and professional, avoiding repetition.
                Do not include pricing or contact information unless specified. 
                Format the response as a JSON object with the fields above."""
            }
        ]

    def generate_keywords(self, industry):
        logging.info(f"Generating keywords for industry: {industry}")
        prompt = f"Generate a list of 5-10 relevant keywords for the {industry} industry. only keywords that talk about products for sale,**Never give Keywords about Services**"
        with self._lock:
            self.history.append({"role": "user", "content": prompt})
        try:
            response = self.client.chat.completions.create(
                model = ModelName,
                max_tokens=self.model_max_token,
                temperature=0.7,
                top_p=0.9,
                presence_penalty=0.2,
                extra_body={"top_k": 50},
                messages=self.history
            )
            logging.info("Received keywords from AI model")
            data = response.to_dict()
            content = data["choices"][0]["message"]["content"].strip()
            try:
                keywords = json.loads(content) if content.startswith("[") else content.split(",")
                keywords = [kw.strip() for kw in keywords if kw.strip()]
            except:
                logging.warning("Failed to parse keywords, using defaults")
                keywords = ["default_keyword1", "default_keyword2"]
        except Exception as e:
            logging.error(f"Error generating keywords: {str(e)}")
            keywords = ["default_keyword1", "default_keyword2"]
        finally:
            with self._lock:
                self.history = self.history[:1]
        return keywords

    def generate_post(self, company_name, facebook_category, industry, location, keyword):
        logging.info(f"Generating post for company: {company_name}, industry: {industry}, keyword: {keyword}, location: {location}")
        location_text = f" in {location}" if location else ""
        prompt = f"""Generate a fresh marketplace post for {company_name} in the {industry} industry, on facebook where the post will be its under category {facebook_category} focusing on the keyword '{keyword}'{location_text}. Provide the following fields in JSON format: _title, _category, _condition, _brand, _description, _productTag. Follow the guidelines in the system prompt."""
        with self._lock:
            self.history.append({"role": "user", "content": prompt})
        try:
            response = self.client.chat.completions.create(
                model = ModelName,
                max_tokens=self.model_max_token,
                temperature=0.6,
                top_p=0.85,
                presence_penalty=0.18,
                extra_body={"top_k": 50},
                messages=self.history
            )
            logging.info("Received post from AI model")
            data = response.to_dict()
            content = data["choices"][0]["message"]["content"].strip()
            try:
                m = re.search(r'```json\s*(\{.*\})\s*```', content, re.S)
                clean = m.group(1) if m else content
                post = json.loads(clean)
            except:
                logging.warning("Failed to parse post, using default")
                post = {
                    "_title": f"Premium {keyword} for Your Home{location_text}",
                    "_category": f"{industry} Materials",
                    "_condition": "New",
                    "_brand": company_name,
                    "_description": f"Discover top-quality {keyword} from {company_name}{location_text}. Our products are designed for durability and performance. Contact us for more details!",
                    "_productTag": keyword
                }
        except Exception as e:
            logging.error(f"Error generating post: {str(e)}")
            post = {
                "_title": f"Premium {keyword} for Your Home{location_text}",
                "_category": f"{industry} Materials",
                "_condition": "New",
                "_brand": company_name,
                "_description": f"Discover top-quality {keyword} from {company_name}{location_text}. Our products are designed for durability and performance. Contact us for more details!",
                "_productTag": keyword
            }
        finally:
            with self._lock:
                self.history = self.history[:1]
        return post

class GetCorrectChoise:
    def __init__(self, client, model_max_token=8192):
        logging.info("Initializing GetCorrectChoise")
        self._lock = threading.Lock()
        self.client = client
        self.model_max_token = int(model_max_token)
        self.history = [{
            "role": "system",
            "content": "You are a professional content-classification assistant.\n\n**Task:** I will provide you with:\n1. **list** – an array of category names.\n2. **industry** – a single industry description.\n\nChoose the single entry from **list** that best matches the **industry** and reply **only** with that choice, wrapped in braces.\n\n**Example**\n```\nlist = {\"Tools\", \"Furniture\", \"Household\", \"Garden\", \"Appliances\", \"Video Games\", \"Books, films & music\", \"Bags & luggage\", \"Women's clothing & shoes\", \"Men's clothing & shoes\", \"Jewellery and accessories\", \"Health & beauty\", \"Pet supplies\", \"Baby & children\", \"Toys and games\", \"Electronics & computers\", \"Mobile phones\", \"Bicycles\", \"Arts & crafts\", \"Sport and outdoors\", \"Car parts\", \"Musical Instruments\", \"Antiques and collectibles\", \"Garage sale\", \"Miscellaneous\", \"Vehicles\"}\nindustry = \"roofing & solar\"\n```\n**Response**\n```\n{\"Tools\"}\n```\n\n**Output format:**\nReturn exactly one JSON object containing the selected list item, e.g. [ \"<ChosenItem>\" ].\n"
        }]
    def GenerateResponseChoise(self, list_of_items, industry):
        prompt = f"""list={list_of_items}\n\nIndustry={industry}"""
        with self._lock:
            self.history.append({"role": "user", "content": prompt})
        try:
            response = self.client.chat.completions.create(
                model = ModelName,
                max_tokens=self.model_max_token,
                temperature=0.6,
                top_p=0.85,
                presence_penalty=0.18,
                extra_body={"top_k": 50},
                messages=self.history
            )
            logging.info("Received post from AI model")
            data = response.to_dict()
            content = data["choices"][0]["message"]["content"].strip()
            stripped_content = content.strip('{}" \n\t')
          
        except Exception as e:
            logging.error(f"Error generating post: {str(e)}")
        finally:
            with self._lock:
                self.history = self.history[:1]
        return stripped_content
# ──────────────────────────────────────────────────────────────────────────────
# ──────────────────────────────────Open Ai─────────────────────────────────────
# ──────────────────────────────────────────────────────────────────────────────
client = OpenAI(
    base_url="",
    api_key=os.environ["OPENAI_API_KEY"]
)
# ──────────────────────────────────────────────────────────────────────────────
app = Flask(__name__)
# ──────────────────────────────────────────────────────────────────────────────
# ───────────────────────────────Users Functions────────────────────────────────
# ──────────────────────────────────────────────────────────────────────────────
@app.route("/chat/aviram_roofing", methods=["POST"])
def chat():
    logging.info("Received chat request")
    payload = request.get_json(force=True)
    messages = payload.get("messages", [])
    company_name = payload.get("company_name", "")
    industry = payload.get("industry", "")
    location = payload.get("location", "")
    phone_number = payload.get("phone_number", "")  
    name_of_avatar = payload.get("avatar_name", "")  
    name_of_customer = payload.get("customer_name", "")  
    chat = ChatInstanse(client, company_name, industry, name_of_avatar, name_of_customer, location, phone_number, time_of_conversation="10", model_max_token=8192)
    for m in messages[:-1]:
        chat.history.append({"role": "user", "content": m})
    last_msg = messages[-1] if messages else ""
    try:
        resp = chat.SendRequestForAnswer(last_msg)
        answer = _parse_assistant_text(resp)
        logging.info(f"Generated chat response: {answer}")
        return answer, 200, {"Content-Type": "text/plain"}
    except Exception as e:
        logging.error(f"Error in chat endpoint: {str(e)}")
        return "Sorry, an error occurred. Please try again.", 500, {"Content-Type": "text/plain"}
# ──────────────────────────────────────────────────────────────────────────────
@app.route("/post/aviram_roofing", methods=["POST"])
def generate_post():
    logging.info("Received post generation request")
    payload = request.get_json(force=True)
    company_name = payload.get("company_name", "")
    industry = payload.get("industry", "")
    facebook_category = payload.get("facebook_category", "")
    location = payload.get("location", "")
    keywords = payload.get("keywords", [])
    phone_number = payload.get("phone_number", "")  # Optional, not used in post unless specified
    post_generator = PostGenerator(client)
    
    if not keywords:
        logging.info("No keywords provided, generating keywords")
        keywords_string_response = post_generator.generate_keywords(industry)
        keywords = extract_keywords(keywords_string_response)
    keyword = keywords[0]
    keywords = keywords[1:] + [keyword]
    logging.info(f"Using keyword: {keyword}")
    post = post_generator.generate_post(company_name, facebook_category, industry, location, keyword)
    required_fields = ["_title", "_category", "_condition", "_brand", "_description", "_productTag"]
    for field in required_fields:
        if field not in post:
            post[field] = f"Default {field} for {keyword}"
    logging.info(f"Generated post: {post}")
    response = {
        "post": post,
        "keywords": keywords
    }
    return json.dumps(response), 200, {"Content-Type": "application/json"}
# ──────────────────────────────────────────────────────────────────────────────
@app.route("/categories/aviram_roofing", methods=["POST"])
def GetCategory():
    payload = request.get_json(force=True)
    res_list = payload.get("list", [])
    industry = payload.get("industry", "")
    industry_gen = GetCorrectChoise(client)
    result_gen_category = industry_gen.GenerateResponseChoise(res_list, industry)
    response = {"category": result_gen_category}
    return json.dumps(response), 200, {"Content-Type": "application/json"}
# ──────────────────────────────────────────────────────────────────────────────
# ────────────────────────────────────Main──────────────────────────────────────
# ──────────────────────────────────────────────────────────────────────────────
if __name__ == "__main__":
    serve(app, host="0.0.0.0", port=5000)