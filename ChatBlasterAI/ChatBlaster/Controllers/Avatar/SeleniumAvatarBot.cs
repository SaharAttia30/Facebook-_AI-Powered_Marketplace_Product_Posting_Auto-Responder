using ChatBlaster.DB;
using ChatBlaster.Models;
using ChatBlaster.Utilities;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Data;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WebSocketSharp;
using System.Threading;
using NUnit.Framework;
using System.Threading.Tasks;
namespace ChatBlaster.Controllers.Avatar
{
    // ──────────────────────────────────────────────────────────────────────────────
    public class CategoryReturnValue
    {
        public string category { get; set; }
        // ──────────────────────────────────────────────────────────────────────────────
        public CategoryReturnValue(string category)
        {
            this.category = category;
        }
    }
    // ──────────────────────────────────────────────────────────────────────────────
    public class ThreadSafeRandommy
    {
        private static readonly ThreadSafeRandommy _instance = new ThreadSafeRandommy();
        private readonly Random _random;
        public static ThreadSafeRandommy Instance => _instance;
        // ──────────────────────────────────────────────────────────────────────────────
        public ThreadSafeRandommy()
        {
            _random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public int Next()
        {
            lock (_random)
            {
                return _random.Next();
            }
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public int Next(int maxValue)
        {
            lock (_random)
            {
                return _random.Next(maxValue);
            }
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public int Next(int minValue, int maxValue) { lock (_random) { return _random.Next(minValue, maxValue); } }
        // ──────────────────────────────────────────────────────────────────────────────
        public double NextDouble() { lock (_random) { return _random.NextDouble(); } }
    }
    // ──────────────────────────────────────────────────────────────────────────────
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class SeleniumAvatarBot
    {
        readonly ChatService _chat;
        readonly string _id;
        readonly string _email, _password, _userAgent;
        readonly Uri _proxyUri;
        private string _proxyUser, _proxyPass;
        readonly string _profileDir;
        readonly string _port;
        //[ThreadStatic]
        //public static IWebDriver _driver;
        //private Thread _thread;
        //private CancellationTokenSource _cancelSource;
        public event EventHandler<string> StatusChanged;
        public event EventHandler<string> LogMessage;
        public string URLAUTOANSWER = "http://sailfish-definite-early.ngrok-free.app/chat/aviram_roofing";
        public string URLPOSTS = "http://sailfish-definite-early.ngrok-free.app/post/aviram_roofing";
        public string URLGETCATEGORY = "http://sailfish-definite-early.ngrok-free.app/categories/aviram_roofing";
        private static readonly HttpClient _httpClient = new HttpClient();
        //private DateTime _sessionStart;
        //private TimeSpan _sessionDuration;
        //private static readonly Random _sessionRnd = new();
        private static readonly Regex _friendsRegex = new(@"^\s*\d[\d,]*\s+friends?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        //private static ThreadLocal<IWebDriver> _webDriverThreadLocal = new();
        private static readonly object _myLock = new object();
        // ──────────────────────────────────────────────────────────────────────────────
        public SeleniumAvatarBot(string id, ChatService chat, string port, string profileDir, string city,
                                string email = "", string pass = "", string proxy = "",
                                string pUser = "", string pPass = "", string ua = "")
        {
            _chat = chat ?? throw new ArgumentNullException(nameof(chat));
            _id = id;
            string city_no_paces = city.Replace(" ", "");
            _email = email;
            _port = port;
            _password = pass;
            if (!string.IsNullOrWhiteSpace(proxy))
                _proxyUri = new Uri(proxy);
            _profileDir = profileDir;
            _userAgent = ua;
            _proxyUser = pUser;
            _proxyPass = pPass;
            AppDomain.CurrentDomain.ProcessExit += (_, __) => Dispose(null);
        }
        // ──────────────────────────────────────────────────────────────────────────────
        // ─────────────────────────────API Functions────────────────────────────────────
        // ──────────────────────────────────────────────────────────────────────────────
        public IWebDriver GetDriver()
        {
            IWebDriver driver = LaunchChrome();
            return driver;
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public IWebDriver GetHeadlessDriver()
        {
            IWebDriver driver = LaunchChromeHeadless();
            return driver;
        }
        // ──────────────────────────────────────────────────────────────────────────────
        [SetUp]
        public IWebDriver Start()
        {
            lock (_myLock)
            {
                try
                {
                    var driver = GetDriver();
                    return driver;
                }
                catch
                {
                    throw;
                }
            }
        }
        // ──────────────────────────────────────────────────────────────────────────────
        [SetUp]
        public IWebDriver StartHeadless()
        {
            lock (_myLock)
            {
                try
                {
                    var driver = GetHeadlessDriver();
                    return driver;
                }
                catch
                {
                    throw;
                }
            }
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public void Stop(IWebDriver driver) => Dispose(driver);
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task<string> FirstLogin(Models.Avatar avatar, IWebDriver local_driver, CancellationToken token = default)
        {
            string return_from_login = "false";
            try
            {
                //local_driver = localDriver;// ?? LaunchChrome();
                //await MakeAFreind(avatar, token);
                //await ApproveNumberOfFriends(avatar, 2, token);
                //await AnswerUnansweredMessagesAsync(avatar, token);
                //await CreateAndUploadPostToMarketPlace(avatar, token);
                //PrivatePost my_post = new();
                //my_post._postText = "hew guys how are you?";
                //my_post.photoPaths = GetRandomPhotoPaths(avatar, 3);
                //await PostAPrivatePosts(avatar, my_post, token);

                return_from_login = await FacebookLogin(avatar, local_driver, token);
                if (return_from_login == "true")
                {
                    MessageBox.Show($"Login was Seccesfull", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (return_from_login == "blocked")
                {
                    MessageBox.Show($"User is Blocked", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    await avatar.WaitAsync("waiting for login", TimeSpan.FromMinutes(2), token);
                    return_from_login = await FacebookLogin(avatar, local_driver, token);
                    if (return_from_login == "flase")
                    {
                        MessageBox.Show($"Login was Failed", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                await avatar.WaitAsync($"there was an Error Logging in {ex.Message}", TimeSpan.FromSeconds(3));
                try
                {
                    return_from_login = await FacebookLogin(avatar, local_driver, token);
                    if (return_from_login == "true")
                    {
                        MessageBox.Show($"Login was Seccesfull", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (return_from_login == "flase")
                    {
                        MessageBox.Show($"Login was Failed", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch
                {
                }
            }
            return return_from_login;
        }
        //──────────────────────────────────────────────────────────────────────────────//
        public FeedPost ParsePostUsingHtml(Models.Avatar avatar, HtmlNode element)
        {
            FeedPost iter_post = new FeedPost();
            try
            {
                var hidden_data_virtualized_area = element.SelectSingleNode(".//div[data-virtualized='true']");
                var hidden_area = element.SelectSingleNode(".//div[hidden]");
                if (hidden_area != null)
                {
                    return iter_post;
                }
                var profile_name_element = element?.SelectSingleNode(".//div[@data-ad-rendering-role='profile_name']");
                var a_element_to_profile = profile_name_element?.SelectSingleNode(".//a");
                if (profile_name_element == null)
                {
                    HtmlNodeCollection? elemnt_list_of_a_element = null;
                    iter_post._title = (element?.SelectSingleNode(".//h3"))?.InnerText.Trim() ?? string.Empty;
                    if (iter_post._title.Contains("Reels"))
                    {
                        elemnt_list_of_a_element = element?.SelectNodes(".//a[@aria-label='reel']") ?? null;
                        if (elemnt_list_of_a_element != null)
                        {
                            foreach (var element_iter in elemnt_list_of_a_element)
                            {
                                string reels_link = element_iter != null && element_iter.Attributes["href"] != null ? element_iter.Attributes["href"].Value : string.Empty;
                                iter_post._reelsLinkList.Add(reels_link);
                            }
                        }

                    }
                    else if (iter_post._title.Contains("People"))
                    {
                        elemnt_list_of_a_element = element?.SelectNodes(".//div[@data-type='hscroll-child']//a");
                    }
                    var a_element_attr = elemnt_list_of_a_element != null && elemnt_list_of_a_element.Count > 0 ? elemnt_list_of_a_element[0].Attributes : null;
                    iter_post._urlToPostAdvertiser = a_element_attr != null && a_element_attr["href"] != null ? a_element_attr["href"].Value : string.Empty;
                    iter_post._text = string.Empty;
                    iter_post._sharesCount = string.Empty;
                    iter_post._commentsCount = string.Empty;
                    iter_post._reactionCount = string.Empty;
                }
                else
                {
                    iter_post._title = (a_element_to_profile?.SelectSingleNode(".//span"))?.InnerText.Trim() ?? string.Empty;
                    var attr = a_element_to_profile?.Attributes;
                    iter_post._urlToPostAdvertiser = attr?["href"].Value ?? string.Empty;
                    iter_post._text = element?.SelectSingleNode(".//div[@data-ad-comet-preview='message']")?.InnerText?.Trim() ?? string.Empty;
                    var reaction_part_element = element?.SelectSingleNode(".//div[@data-visualcompletion='ignore-dynamic']//div[@class]");
                    var post_interaction_list_elements = reaction_part_element?.SelectNodes(".//div[@role='button']");
                    for (int i = 0; i < post_interaction_list_elements?.Count; ++i)
                    {
                        var button = post_interaction_list_elements[i];
                        if (button.InnerText.Contains("reactions"))
                        {
                            var res = button.InnerText.Split(' ')[1];
                            iter_post._reactionCount = res.Split(':')[1];
                            if (post_interaction_list_elements.Count > i + 1 && post_interaction_list_elements[i + 1].InnerText.Contains("shares"))
                            {
                                iter_post._sharesCount = post_interaction_list_elements[i + 1].InnerText.Split(' ')[0];
                                continue;
                            }
                            iter_post._commentsCount = post_interaction_list_elements.Count > i + 1 && !(post_interaction_list_elements[i + 1].InnerText.Contains("shares")) ? post_interaction_list_elements[i + 1].InnerText.Split(' ')[0] : string.Empty;
                            iter_post._sharesCount = post_interaction_list_elements.Count > i + 2 ? post_interaction_list_elements[i + 2].InnerText.Split(' ')[0] : string.Empty;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return iter_post;
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task<bool> ReactRandomeReaction(Models.Avatar avatar, IWebElement post_to_react_on, IWebDriver localDriver, CancellationToken token = default)
        {
            var js = (IJavaScriptExecutor)localDriver;
            var reaction_list = new List<string>() { "Like", "Love", "Care", "Haha", "Wow", "Sad", "Angry" };
            Random rnd = new Random();
            int position_in_array = rnd.Next(0, reaction_list.Count);
            await avatar.WaitAsync("human pause ", TimeSpan.FromMilliseconds(rnd.Next(100, 700)), token);
            string randome_reaction_choosen = reaction_list[position_in_array];
            IWebElement? button_to_open_reactions_options = null;
            try
            {
                button_to_open_reactions_options = post_to_react_on.FindElement(By.XPath(".//div[@aria-label='React']"));
                SafeClick(localDriver, button_to_open_reactions_options);

            }
            catch
            {
                return false;
            }
            await avatar.WaitAsync("human pause ", TimeSpan.FromMilliseconds(rnd.Next(500, 1500)), token);
            var reaction_elements = localDriver.FindElements(By.XPath("//div[@aria-label='Reactions']//div[@role='button']"));
            js.ExecuteScript("arguments[0].scrollIntoView({behavior:'smooth',block:'center'});", reaction_elements);
            SafeClick(localDriver, reaction_elements[position_in_array]);
            ++avatar.ReactionsMadeToday;
            _chat.UpdateAvatar(avatar);
            return true;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------
        public async Task ApproveNumberOfFriends(Models.Avatar avatar, IWebDriver localDriver, int number_of_confirm, CancellationToken token = default)
        {
            try
            {
                Navigate(localDriver, "https://www.facebook.com/friends/requests");
                await avatar.WaitAsync("waiting for navigations at ApproveNumberOfFriends", TimeSpan.FromSeconds(2), token);
                IWebElement navigation_links = null;
                try
                {
                    navigation_links = Wait(localDriver, By.CssSelector("div[aria-label*='friend request' i][role='navigation' i]"), timeout: 5);
                }
                catch
                {
                    navigation_links = Wait(localDriver, By.CssSelector("div[aria-label='Friend Requests'][role='navigation']"), timeout: 5);

                }
                var list_of_links = navigation_links.FindElements(By.CssSelector("div[data-visualcompletion] a[role='link']"));
                var html_string = (string)((IJavaScriptExecutor)localDriver).ExecuteScript("return arguments[0].outerHTML;", navigation_links);
                var document = new HtmlAgilityPack.HtmlDocument();
                document.LoadHtml(html_string);
                var elements = document.DocumentNode.SelectNodes("//div[@data-visualcompletion]//a[@role='link']");
                List<IWebElement> confirm_buttons = new List<IWebElement>();
                if (elements == null)
                {
                    return;
                }
                for (int i = 0; i < elements.Count && 0 < number_of_confirm; i++)
                {
                    var item_node = elements[i].SelectSingleNode(".//div[@aria-label='Confirm']");
                    try
                    {
                        if (item_node != null)
                        {
                            var confirm_button = list_of_links[i].FindElement(By.XPath(".//div[@aria-label='Confirm']"));
                            SafeClick(localDriver, confirm_button);
                            --number_of_confirm;
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                await avatar.WaitAsync($"Error ApproveNumberOfFriends: {ex.Message}", TimeSpan.FromSeconds(2), token);
            }
            finally
            {
                //_slot.Release();
            }

        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task DefaultScroll(Models.Avatar avatar, IWebDriver driver, DateTime time_to_end, CancellationToken token = default)
        {
            var rnd = ThreadSafeRandommy.Instance;
            try
            {
                if (driver.Url != "https://www.facebook.com" && driver.Url != "https://www.facebook.com/")
                {
                    Navigate(driver, "https://www.facebook.com/");
                }
                await avatar.WaitAsync("home", TimeSpan.FromMilliseconds(rnd.Next(600, 1800)), token);
                if (DateTime.Now >= time_to_end) return;
                var js = (IJavaScriptExecutor)driver;
                int targetIndex = -1;
                const double actionProb = 0.12;
                var html_string = driver.PageSource;
                var document = new HtmlAgilityPack.HtmlDocument();
                document.LoadHtml(html_string);
                while (DateTime.Now < time_to_end && !token.IsCancellationRequested)
                {
                    try
                    {
                        var posts = driver.FindElements(By.CssSelector("div[aria-posinset]"));
                        html_string = driver.PageSource;
                        document.LoadHtml(html_string);
                        var posts_node_html = document.DocumentNode.SelectNodes("//div[@aria-posinset]");
                        if (posts.Count == 0)
                        {
                            js.ExecuteScript("window.scrollBy(0,400);");
                            continue;
                        }
                        if (targetIndex == -1)
                        {
                            int visible = posts.TakeWhile(p => !string.IsNullOrWhiteSpace(p.Text)).Count();
                            if (visible > 2) targetIndex = rnd.Next(0, visible);
                        }
                        int delta = rnd.Next(250, 1050);
                        js.ExecuteScript($"window.scrollBy(0,{delta});");
                        await avatar.WaitAsync("scroll", TimeSpan.FromMilliseconds(rnd.Next(1200, 3000)), token);
                        bool canReact = avatar.ReactionsMadeToday < AvatarConfig.MaxReactions;
                        if (canReact && targetIndex >= 0 && targetIndex < posts.Count && rnd.NextDouble() < actionProb)
                        {
                            try
                            {
                                bool res_from_reaction = false;
                                while (!res_from_reaction)
                                {
                                    var target = posts[targetIndex];
                                    js.ExecuteScript("arguments[0].scrollIntoView({behavior:'smooth',block:'center'});", target);

                                    FeedPost post_info = ParsePostUsingHtml(avatar, posts_node_html[targetIndex]);
                                    if (post_info._title.Contains("Reels"))
                                    {

                                    }
                                    else if (post_info._title.Contains("People"))
                                    {
                                    }
                                    else if (post_info._title.IsNullOrEmpty())
                                    {

                                    }
                                    var reaction_list = new List<string>() { "Like", "Love", "Care", "Haha", "Wow", "Sad", "Angry" };
                                    int position_in_array = rnd.Next(0, reaction_list.Count);
                                    await avatar.WaitAsync("human pause ", TimeSpan.FromMilliseconds(rnd.Next(1000, 3700)), token);
                                    string randome_reaction_choosen = reaction_list[position_in_array];
                                    IWebElement? button_to_open_reactions_options = null;
                                    try
                                    {
                                        button_to_open_reactions_options = target.FindElement(By.XPath(".//div[@aria-label='React']"));
                                        js.ExecuteScript("arguments[0].click();", button_to_open_reactions_options);
                                        await avatar.WaitAsync("human pause ", TimeSpan.FromMilliseconds(rnd.Next(900, 3500)), token);
                                        var reaction_elements = driver.FindElements(By.XPath("//div[@aria-label='Reactions']//div[@role='button']"));
                                        await avatar.WaitAsync("human pause ", TimeSpan.FromMilliseconds(rnd.Next(900, 2900)), token);
                                        SafeClick(driver, reaction_elements[position_in_array]);
                                        res_from_reaction = true;
                                    }
                                    catch
                                    {
                                        res_from_reaction = false;
                                        targetIndex += 1;
                                        continue;
                                    }
                                    ++avatar.ReactionsMadeToday;
                                    _chat.UpdateAvatar(avatar);
                                    await avatar.WaitAsync("after react", TimeSpan.FromMilliseconds(rnd.Next(1000, 2500)), token);
                                }
                            }
                            catch (StaleElementReferenceException) { targetIndex = -1; }
                        }
                    }
                    catch
                    {
                        await avatar.WaitAsync("scroll-err", TimeSpan.FromMilliseconds(rnd.Next(600, 1800)), token);
                    }
                }
            }
            catch (Exception ex)
            {
                await avatar.WaitAsync($"err DefaultScroll: {ex.Message}", TimeSpan.FromMilliseconds(rnd.Next(600, 1800)), token);
            }
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task AnswerUnansweredMessagesAsync(Models.Avatar avatar, IWebDriver driver, CancellationToken token = default, int max_loops = 15)
        {
            string text_input_query = "div[aria-label='Message'][role='textbox']";
            IWebDriver localDriver = driver;
            OnStatusChanged("Scanning chats…");
            try
            {
                Navigate(localDriver, "https://www.facebook.com/messages/t/");
                await avatar.WaitAsync("page render", TimeSpan.FromSeconds(3), token);
                IWebElement? chatGrid = null;
                try
                {
                    chatGrid = Wait(localDriver, By.CssSelector("div[aria-label='Chats'][role='grid']"), timeout: 10);
                }
                catch
                {
                    try
                    {
                        await avatar.WaitAsync("Refresh Page 1st time", TimeSpan.FromSeconds(1), token);
                        Refresh(localDriver);
                        chatGrid = Wait(localDriver, By.CssSelector("div[aria-label='Chats'][role='grid']"), timeout: 10);
                    }
                    catch
                    {
                        if (IsBlockedPage(localDriver))
                        {
                            avatar._blocked = true;
                            await avatar.WaitAsync("This user is blocked", TimeSpan.FromSeconds(3), token);
                            return;
                        }
                        Navigate(localDriver, "https://www.facebook.com/messages/t/");
                        await avatar.WaitAsync("try to Rerender page", TimeSpan.FromSeconds(1), token);
                        Refresh(localDriver);
                        await avatar.WaitAsync("Page has been Refreshed()", TimeSpan.FromSeconds(3), token);
                        try
                        {
                            chatGrid = Wait(localDriver, By.CssSelector("div[aria-label='Chats'][role='grid']"), timeout: 10);
                        }
                        catch
                        {
                            await avatar.WaitAsync("Can't Find Chats for user", TimeSpan.FromSeconds(3), token);
                            return;
                        }
                    }
                }
                var cells = chatGrid.FindElements(By.CssSelector("div[data-virtualized]")).ToList();
                int mpIndex = cells.FindIndex(c => c.Text.Contains("Marketplace"));
                if (mpIndex >= 0) SafeClick(localDriver, cells[mpIndex]);
                else if (mpIndex < 0)
                {
                    await avatar.WaitAsync("No Chats ", TimeSpan.FromSeconds(3), token);
                    return;
                }
                int i = 0;
                for (i = 0; i < max_loops; i++)
                {
                    int answered_messages = 0;
                    try
                    {
                        await avatar.WaitAsync("Waiting For Messages to Load", TimeSpan.FromMilliseconds(5), token);
                        string chat_grid_selector_for_market = "div[aria-label=\"Marketplace\"][role=\"grid\"]";
                        var chat_grid = Wait(localDriver, By.CssSelector(chat_grid_selector_for_market), 20);
                        string outer = (string)((IJavaScriptExecutor)localDriver).ExecuteScript("return arguments[0].outerHTML;", chat_grid);
                        var html_string = outer;
                        var chats_elements = WaitAll(localDriver, By.CssSelector("div[aria-label=\"Marketplace\"][role=\"grid\"] div[data-virtualized]")).ToList();
                        var document = new HtmlAgilityPack.HtmlDocument();
                        document.LoadHtml(html_string);
                        var elements = document.DocumentNode.SelectNodes("//div[@data-virtualized]");
                        var conversation_queue = GetNChatsMarketPlaceFastHtmlOnly(avatar, max_loops, elements, chats_elements);
                        if (conversation_queue == null)
                        {
                            await avatar.WaitAsync("page render", TimeSpan.FromSeconds(8), token);
                            conversation_queue = GetNChatsMarketPlaceFastHtmlOnly(avatar, max_loops, elements, chats_elements);
                        }
                        if (conversation_queue == null || conversation_queue.Count < 1)
                        {
                            await avatar.WaitAsync($"Done Searching: Going To Sleep", TimeSpan.FromSeconds(3), token);
                            break;
                        }
                        var conversation = conversation_queue.Dequeue();
                        if (i % 4 == 0)
                        {
                            await HumanScrollTo(localDriver, conversation._WebDriveElement, token);
                        }
                        if (conversation._hasUnreadMessages)
                        {
                            try
                            {
                                var conversationDb = _chat.GetConversation(conversation.ConversationId) ?? _chat.StartConversation(conversation);
                                _chat.AddMessage(conversationDb.ConversationId, conversation._userName, conversation._lastMessage);
                                conversation.Messages = conversationDb.Messages;
                                string text_for_conc = await RandText(avatar, conversation._userName, conversation);
                                SafeClick(localDriver, conversation._WebDriveElement);
                                var text_input = Wait(localDriver, By.CssSelector(text_input_query));
                                await avatar.WaitAsync($"Waiting Before input values", TimeSpan.FromSeconds(3), token);
                                text_input.SendKeys(text_for_conc);
                                PressEnter(localDriver, By.CssSelector(text_input_query));
                                avatar.MessagesAnsweredToday += 1;
                                _chat.UpdateAvatar(avatar);
                                i = 0;
                            }
                            catch
                            {
                                await avatar.WaitAsync($"Error with: Randomizig and sendiong text", TimeSpan.FromSeconds(3), token);
                                continue;
                            }
                            OnStatusChanged($"Answered message {++answered_messages}");
                            Navigate(localDriver, "https://www.facebook.com/messages/t/");
                            chatGrid = Wait(localDriver, By.CssSelector("div[aria-label='Chats'][role='grid']"));
                            cells = chatGrid.FindElements(By.CssSelector("div[data-virtualized]")).ToList();
                            mpIndex = cells.FindIndex(c => c.Text.Contains("Marketplace"));
                            if (mpIndex == 0)
                            {
                                SafeClick(localDriver, cells[mpIndex + 1]);
                                SafeClick(localDriver, cells[mpIndex]);
                            }
                            else
                            {
                                SafeClick(localDriver, cells[mpIndex - 1]);
                                SafeClick(localDriver, cells[mpIndex]);
                            }
                            int pause = new Random().Next(1_000, 3_000);
                            await avatar.WaitAsync("human pause", TimeSpan.FromMilliseconds(pause), token);
                        }
                    }
                    catch
                    {
                        await avatar.WaitAsync($"Done Searching: Going To Sleep", TimeSpan.FromSeconds(8), token);
                        continue;
                    }
                }
                OnStatusChanged("Answer messages is Completed");
            }
            catch (Exception ex)
            {
                await avatar.WaitAsync($"Error AnswerUnansweredMessagesAsync:{ex}", TimeSpan.FromSeconds(2), token);
            }
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task MakeAFreind(Models.Avatar avatar, IWebDriver driver, CancellationToken token = default)
        {
            IWebDriver? localDriver = driver;
            try
            {
                Service service = _chat.GetServiceById(avatar.ServiceId);
                ServiceArea service_area = _chat.GetServiceAreaById(service.ServiceAreaId);
                List<Models.Avatar> avatars_by_state = _chat.GetAvatarsByState(service_area.State);
                foreach (Models.Avatar avatar_iterator in avatars_by_state)
                {
                    if (avatar_iterator.Id == avatar.Id)
                    {
                        continue;
                    }
                    var avatar_to_add_as_friend_url = avatar_iterator._linkForAvatarProfile;
                    Navigate(localDriver, avatar_to_add_as_friend_url);
                    try
                    {
                        await avatar.WaitAsync($"Making Freinds", TimeSpan.FromSeconds(1.5), token);
                        IWebElement add_friend_button = Wait(localDriver, By.CssSelector("div[aria-label*='add friend' i]"), timeout: 5);
                        if (add_friend_button == null)
                        {
                            await avatar.WaitAsync($"cant Add {avatar._userName}", TimeSpan.FromSeconds(1.5), token);
                            continue;
                        }
                        SafeClick(localDriver, add_friend_button);
                        await avatar.WaitAsync($"Add {avatar_iterator._userName} As a friend", TimeSpan.FromSeconds(2), token);
                        ++avatar.FriendRequestSentToday;
                        _chat.UpdateAvatar(avatar);
                        break;
                    }
                    catch (Exception ex)
                    {
                        await avatar.WaitAsync($"Error Making Freinds: {ex.Message}", TimeSpan.FromSeconds(2), token);
                    }
                }
            }
            catch (Exception ex)
            {
                await avatar.WaitAsync($"Error Making Freinds: {ex.Message}", TimeSpan.FromSeconds(2), token);
            }
            finally
            {
                await avatar.WaitAsync($"Waiting {2}ms before Finisht", TimeSpan.FromSeconds(2), token);

            }
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task CreateAndUploadPostToMarketPlace(Models.Avatar avatar, IWebDriver driver, CancellationToken token = default)
        {
            IWebDriver localDriver = driver;
            try
            {
                List<string> keywords = LoadKeywords(avatar);
                await avatar.WaitAsync($"Processing post", TimeSpan.FromSeconds(2), token);
                Service service = _chat.GetServiceById(avatar.ServiceId);
                ServiceArea service_area = _chat.GetServiceAreaById(service.ServiceAreaId);
                Company company = _chat.GetCompanyById(service_area.CompanyId);
                var payload = new
                {
                    company_name = company.Name,
                    facebook_category = service.FacebookCategory,
                    industry = service.Industry,
                    location = $"{service_area.City.Trim()} {service_area.State.Trim().ToUpper()}",
                    keywords = keywords,
                    phone_number = service.PhoneNumber
                };
                var response = await SendPostRequest(payload);
                var postData = response["post"];
                var updatedKeywords = response["keywords"]?.ToObject<List<string>>();
                string _createIteamUrl = "https://www.facebook.com/marketplace/create/item/";
                string title = postData?["_title"]?.ToString() ?? string.Empty;
                string price = "1";
                string facebook_category = payload.facebook_category;
                string industry = payload.industry;
                string condition = postData?["_condition"]?.ToString() ?? string.Empty;
                string brand = postData?["_brand"]?.ToString() ?? string.Empty;
                string description = postData?["_description"]?.ToString() ?? string.Empty;
                string product_tag = postData?["_productTag"]?.ToString() ?? string.Empty;
                string location = payload.location;
                bool public_meetup = false;
                bool door_pick_up = false;
                bool door_drop_off = true;
                List<string> photoPaths = GetRandomPhotoPaths(avatar, 3);
                MarketPlacePost post = new(title, price, facebook_category, industry, condition, brand,
                    description, product_tag, location, public_meetup, door_pick_up, door_drop_off)
                {
                    photoPaths = photoPaths
                };
                await UploadMarketPlacePost(avatar, localDriver, post, token);
                keywords = updatedKeywords ?? new List<string>();
                SaveKeywords(avatar, keywords);
                await avatar.WaitAsync($"Completed post", TimeSpan.FromSeconds(2), token);
            }
            catch (Exception ex)
            {
                await avatar.WaitAsync($"Error processing post: {ex.Message}", TimeSpan.FromSeconds(2), token);
            }
            finally
            {
                await avatar.WaitAsync($"Waiting 2 ms before Finisht", TimeSpan.FromSeconds(2), token);
            }
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task PostToGroups(Models.Avatar avatar, IWebDriver driver, CancellationToken token = default)
        {
            Random random = new Random();
            IWebDriver localDriver = driver;
            try
            {
                var avatar_groups_from_db = _chat.GetGroupsByAvatarId(avatar.Id);
                if (IsMoreThanAWeekSinceLastUpdate(avatar.LastGroupsUpdateTime))
                {
                    await UpdateAvatarGroupsList(avatar, localDriver, token);
                }
                else
                {
                    avatar.Groups = _chat.GetGroupsByAvatarId(avatar.Id);
                }
                //
            }
            catch (Exception ex)
            {
                await avatar.WaitAsync($"Error with: PostToGroups {ex} ", TimeSpan.FromSeconds(2), token);
            }
            //finally
            //{
            //    await avatar.WaitAsync($"PostToGroups is done", TimeSpan.FromSeconds(1), token);
            //    try
            //    {
            //        localDriver?.Quit();
            //    }
            //    catch { }
            //    _driver = null;
            //    _slot.Release();
            //}
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task CommentOnPrivatePosts(Models.Avatar avatar, IWebDriver driver, MarketPlacePost ready_post_to_send, CancellationToken token = default)
        {
            Random random = new Random();
            IWebDriver localDriver = driver;
            try
            {

            }
            catch
            {
                await avatar.WaitAsync("Error with: Run outside loop ", TimeSpan.FromSeconds(2), token);
            }
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task PostAPrivatePosts(Models.Avatar avatar, IWebDriver driver, PrivatePost ready_post_to_send, CancellationToken token = default)
        {
            Random random = new Random();
            IWebDriver localDriver = driver;
            try
            {

                Navigate(localDriver, "https://www.facebook.com/");
                var create_post_button = Wait(localDriver, By.CssSelector("div[aria-label*='create' i] div[role='button']"));
                SafeClick(localDriver, create_post_button);
                await avatar.WaitAsync("Human wait PostAPrivatePosts", TimeSpan.FromSeconds(2), token);
                try
                {
                    var file_input = Wait(localDriver, By.CssSelector("input[accept*='image' i]"));
                    var filePaths = ready_post_to_send.photoPaths.Select(path => Path.GetFullPath(path)).ToArray();
                    file_input.SendKeys(string.Join("\n", filePaths));
                    await avatar.WaitAsync("Human wait PostAPrivatePosts", TimeSpan.FromMilliseconds(1347), token);
                    var input_elements = WaitAll(localDriver, By.CssSelector("div[role='textbox']"));
                    var list_tmp = input_elements.ToList();

                    if (input_elements.Count > 1)
                    {
                        list_tmp[1].SendKeys(ready_post_to_send._postText);
                    }
                    else
                    {
                        SetKeys(localDriver, By.CssSelector("div[role='textbox']"), ready_post_to_send._postText);
                    }
                }
                catch (Exception ex)
                {
                    await avatar.WaitAsync("Error with:PostAPrivatePosts ", TimeSpan.FromSeconds(2), token);
                }
            }
            catch
            {
                await avatar.WaitAsync("Error with: PostAPrivatePosts ", TimeSpan.FromSeconds(2), token);
            }
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task<bool> HasUnreadAsync(Models.Avatar avatar, IWebDriver local_driver, CancellationToken token = default)
        {
            IWebDriver? driver = local_driver;
            try
            {
                Navigate(driver, "https://www.facebook.com/messages/t/");
                await avatar.WaitAsync("page render", TimeSpan.FromSeconds(3), token);
                IWebElement? chatGrid = null;
                try
                {
                    chatGrid = Wait(driver, By.CssSelector("div[aria-label='Chats'][role='grid']"), timeout: 10);
                }
                catch
                {
                    try
                    {
                        await avatar.WaitAsync("Refresh Page 1st time", TimeSpan.FromSeconds(1), token);
                        Refresh(driver);
                        chatGrid = Wait(driver, By.CssSelector("div[aria-label='Chats'][role='grid']"), timeout: 10);
                    }
                    catch
                    {
                        Navigate(driver, "https://www.facebook.com/messages/t/");
                        await avatar.WaitAsync("try to Rerender page", TimeSpan.FromSeconds(1), token);
                        Refresh(driver);
                        await avatar.WaitAsync("Page has been Refreshed()", TimeSpan.FromSeconds(3), token);
                        try
                        {
                            chatGrid = Wait(driver, By.CssSelector("div[aria-label='Chats'][role='grid']"), timeout: 10);
                        }
                        catch
                        {
                            await avatar.WaitAsync("Can't Find Chats for user", TimeSpan.FromSeconds(3), token);
                            return false;
                        }
                    }
                }
                var cells = chatGrid.FindElements(By.CssSelector("div[data-virtualized]")).ToList();
                int mpIndex = cells.FindIndex(c => c.Text.Contains("Marketplace") && c.Text.Contains("new message"));
                if (mpIndex >= 0) return true;
            }
            catch (Exception ex)
            {
                await avatar.WaitAsync($"error HasUnreadAsync {ex.Message}", TimeSpan.FromSeconds(3), token);
            }
            finally
            {
                //Slots.Release();
            }
            return false;
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public IWebDriver LaunchChromeHeadless()
        {
            var options = new ChromeOptions { AcceptInsecureCertificates = true };
            Directory.CreateDirectory(_profileDir);
            string chrome_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chrome-win64", "chrome.exe");
            options.BinaryLocation = chrome_path;
            options.AddArgument($"--user-data-dir={_profileDir}");
            options.AddArgument($"--remote-debugging-port={_port}");
            options.AddArgument("--headless=new");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--blink-settings=imagesEnabled=false");
            options.AddArgument("--mute-audio");
            options.AddArgument("--disable-extensions");

            if (!string.IsNullOrWhiteSpace(_userAgent))
            {
                options.AddArgument($"--user-agent={_userAgent}");
            }
            var rnd = new Random();
            options.AddArgument($"--window-size={rnd.Next(800, 1201)},{rnd.Next(600, 901)}");
            options.AddArgument("--lang=en-US");
            IWebDriver driver;
            if (_proxyUri != null)
            {
                string proxyHost = _proxyUri.Host;
                int proxyPort = _proxyUri.Port;
                var proxy = new Proxy
                {
                    Kind = ProxyKind.Manual,
                    HttpProxy = $"{proxyHost}:{proxyPort}",
                    SslProxy = $"{proxyHost}:{proxyPort}"
                };
                options.Proxy = proxy;
                options.AddArgument("--proxy-bypass-list=<-loopback>");
                string extDir = ProxyExtension.Build(proxyHost, proxyPort, _proxyUser, _proxyPass);
                options.AddArgument($"--load-extension={extDir}");
                options.AddArgument($"--disable-extensions-except={extDir}");
                driver = new ChromeDriver(options);
                if (!VerifyProxy(driver))
                {
                    driver.Quit();
                    throw new InvalidOperationException("Proxy check failed – aborting bot start");
                }
            }
            else
            {
                driver = new ChromeDriver(options);
            }
            try
            {
                ((ChromeDriver)driver).ExecuteCdpCommand("Page.addScriptToEvaluateOnNewDocument", new Dictionary<string, object>
                {
                    ["source"] = "Object.defineProperty(navigator,'webdriver',{get:()=>undefined});"
                });
            }
            catch { /* ignore exceptions silently */ }

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);

            return driver;
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public IWebDriver LaunchChrome()
        {
            var options = new ChromeOptions { AcceptInsecureCertificates = true };
            Directory.CreateDirectory(_profileDir);
            string chrome_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chrome-win64", "chrome.exe");
            options.BinaryLocation = chrome_path;
            options.AddArgument($"--user-data-dir={_profileDir}");
            options.AddArgument($"--remote-debugging-port={_port}");
            string[] args = {
                "--disable-blink-features=AutomationControlled",
                "--disable-infobars",
                //"--disable-extensions",
                "--disable-popup-blocking",
                "--no-sandbox",
                "--disable-dev-shm-usage",
                "--disable-gpu",
                "--disable-software-rasterizer",
                "--use-gl=swiftshader",
                "--enable-unsafe-swiftshader",
                "--disable-notifications",
                "--disable-background-networking",
                "--disable-default-apps",
                "--disable-sync",
                "--disable-translate",
                "--no-first-run",
                "--no-default-browser-check",
                "--disable-save-password-bubble",
                "--disable-password-generation",
                "--disable-autofill-keyboard-accessory-view",
                "--disable-single-click-autofill",
                "--disable-prompt-on-repost",
                "--disable-client-side-phishing-detection",
                "--disable-component-update",
                "--disable-domain-reliability",
                "--disable-renderer-backgrounding",
                "--mute-audio"

            };
            foreach (var a in args)
            {
                options.AddArgument(a);
            }
            if (!string.IsNullOrWhiteSpace(_userAgent))
            {
                options.AddArgument($"--user-agent={_userAgent}");
            }
            var rnd = new Random();
            options.AddArgument($"--window-size={rnd.Next(800, 1201)},{rnd.Next(600, 901)}");
            options.AddArgument("--lang=en-US");
            IWebDriver driver;
            if (_proxyUri != null)
            {
                string proxyHost = _proxyUri.Host;
                int proxyPort = _proxyUri.Port;
                var proxy = new Proxy
                {
                    Kind = ProxyKind.Manual,
                    HttpProxy = $"{proxyHost}:{proxyPort}",
                    SslProxy = $"{proxyHost}:{proxyPort}"
                };
                options.Proxy = proxy;
                options.AddArgument("--proxy-bypass-list=<-loopback>");
                string extDir = ProxyExtension.Build(proxyHost, proxyPort, _proxyUser, _proxyPass);
                options.AddArgument($"--load-extension={extDir}");
                options.AddArgument($"--disable-extensions-except={extDir}");
                driver = new ChromeDriver(options);
                if (!VerifyProxy(driver))
                {
                    driver.Quit();
                    throw new InvalidOperationException("Proxy check failed – aborting bot start");
                }
            }
            else
            {
                driver = new ChromeDriver(options);
            }
            try
            {
                ((ChromeDriver)driver).ExecuteCdpCommand("Page.addScriptToEvaluateOnNewDocument", new Dictionary<string, object>
                {
                    ["source"] = "Object.defineProperty(navigator,'webdriver',{get:()=>undefined});"
                });
            }
            catch { /* ignore exceptions silently */ }

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);

            return driver;
        }
        // ──────────────────────────────────────────────────────────────────────────────
        private static bool VerifyProxy(IWebDriver drv)
        {
            string externalIp = null;
            using var hc = new HttpClient();
            var machineIp = hc.GetStringAsync("https://api.ipify.org").Result.Trim();
            try
            {
                drv.Navigate().GoToUrl("https://api.ipify.org?format=text");
                externalIp = drv.FindElement(By.TagName("body")).Text.Trim();
            }
            catch
            {
                // First attempt failed; wait and retry
            }
            if (externalIp != null && !System.Net.IPAddress.TryParse(externalIp, out _))
            {
                externalIp = null;
            }
            if (externalIp == null || string.Equals(machineIp, externalIp, StringComparison.Ordinal))
            {
                Thread.Sleep(2000);
                try
                {
                    drv.Navigate().GoToUrl($"https://api.ipify.org?format=text&uid={Guid.NewGuid()}");
                    externalIp = drv.FindElement(By.TagName("body")).Text.Trim();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Proxy check failed on retry: {ex.Message}");
                    return false;
                }
            }
            if (externalIp != null && !System.Net.IPAddress.TryParse(externalIp, out _))
            {
                return false;
            }
            if (string.IsNullOrEmpty(externalIp))
            {
                return false;
            }
            return !string.Equals(machineIp, externalIp, StringComparison.Ordinal);
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task<string> FacebookLogin(Models.Avatar avatar, IWebDriver local_driver, CancellationToken token = default)
        {
            var duration_minimum = AvatarConfig.MinSessionLength;
            var time_start = DateTime.Now;
            string value_of_actions = "false";
            try
            {

                Navigate(local_driver, "https://www.facebook.com/");
                try
                {
                    SetKeys(local_driver, By.CssSelector("input[type='text'][aria-label*='email' i]"), avatar._email);
                    SetKeys(local_driver, By.CssSelector("input[type='password'][aria-label*='pass' i]"), avatar._password);
                    PressEnter(local_driver, By.CssSelector("input[type='password'][aria-label*='pass' i]"));
                }
                catch
                {
                    string html = local_driver.PageSource;
                    var document = new HtmlAgilityPack.HtmlDocument();
                    document.LoadHtml(html);
                    var email_node_by_name = document.DocumentNode.SelectSingleNode("//input[@name='email']");
                    var email_node = document.DocumentNode.SelectSingleNode("//input[@id='email']");
                    if (email_node_by_name != null)
                    {
                        SetKeys(local_driver, By.CssSelector("input[name='email']"), avatar._email);
                        SetKeys(local_driver, By.CssSelector("input[name='pass']"), avatar._password);
                        PressEnter(local_driver, By.CssSelector("input[name='pass']"));
                    }
                    else if (email_node != null)
                    {
                        SetKeys(local_driver, By.Id("email"), avatar._email);
                        SetKeys(local_driver, By.Id("pass"), avatar._password);
                        PressEnter(local_driver, By.Id("pass"));
                    }
                }
                if (IsLoggedIn(local_driver, TimeSpan.FromSeconds(120)))
                {
                    value_of_actions = "true";
                }
            }
            catch
            {
                if (IsLoggedIn(local_driver, TimeSpan.FromSeconds(60)))
                {
                    value_of_actions = "true";
                }
                else
                {
                    value_of_actions = "false";
                }
            }
            try
            {
                if (value_of_actions != "true")
                {
                    Navigate(local_driver, "https://www.facebook.com/");
                    try
                    {
                        SetKeys(local_driver, By.CssSelector("input[type='text'][aria-label*='email' i]"), avatar._email);
                        SetKeys(local_driver, By.CssSelector("input[type='password'][aria-label*='pass' i]"), avatar._password);
                        PressEnter(local_driver, By.CssSelector("input[type='password'][aria-label*='pass' i]"));
                    }
                    catch
                    {
                        string html = local_driver.PageSource;
                        var document = new HtmlAgilityPack.HtmlDocument();
                        document.LoadHtml(html);
                        var email_node_by_name = document.DocumentNode.SelectSingleNode("//input[@name='email']");
                        var email_node = document.DocumentNode.SelectSingleNode("//input[@id='email']");
                        if (email_node_by_name != null)
                        {
                            SetKeys(local_driver, By.CssSelector("input[name='email']"), avatar._email);
                            SetKeys(local_driver, By.CssSelector("input[name='pass']"), avatar._password);
                            PressEnter(local_driver, By.CssSelector("input[name='pass']"));
                        }
                        else if (email_node != null)
                        {
                            SetKeys(local_driver, By.Id("email"), avatar._email);
                            SetKeys(local_driver, By.Id("pass"), avatar._password);
                            PressEnter(local_driver, By.Id("pass"));
                        }
                    }
                }
                try
                {
                    await InitAvatarInfo(local_driver, avatar, token);
                    OnStatusChanged("Ready To Run");
                    value_of_actions = "true";
                }
                catch { }
                var sessionEnd = time_start + duration_minimum;
                var timeLeft = sessionEnd - DateTime.Now;
                if (timeLeft > TimeSpan.Zero)
                {
                    await ApproveNumberOfFriends(avatar, local_driver, 2, token);
                    await DefaultScroll(avatar, local_driver, sessionEnd, token);
                }
            }
            catch
            {
                var wait = new WebDriverWait(local_driver, TimeSpan.FromSeconds(10));
                try
                {
                    if (IsBlockedPage(local_driver))
                    {
                        value_of_actions = "blocked";
                    }
                }
                catch (WebDriverTimeoutException)
                {
                }
            }

            return value_of_actions;
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public void SafeClick(IWebDriver driver, IWebElement el)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", el);
            new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d =>
            {
                try
                {
                    return el.Displayed && el.Enabled;
                }
                catch
                {
                    return false;
                }
            });
            el.Click();
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task ScrollAllTheWayDown(Models.Avatar avatar, IWebDriver localDriver, CancellationToken token = default, int loops = 20)
        {
            var js = (IJavaScriptExecutor)localDriver;
            for (int i = 0; i < loops; i++)
            {
                try
                {
                    js.ExecuteScript("window.scrollBy(0, window.innerHeight)");
                    await avatar.WaitAsync($"PostToGroups:", TimeSpan.FromSeconds(2), token);
                    js.ExecuteScript("window.scrollBy(0, window.innerHeight)");
                    await avatar.WaitAsync($"PostToGroups:", TimeSpan.FromSeconds(2), token);
                    if (await AtPageEndAsync(avatar, localDriver, token))
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    await avatar.WaitAsync($"Scroll error: {ex.Message}", TimeSpan.FromSeconds(1), token);
                }
            }
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public void GetGroupsUsingHtmlParsing(Models.Avatar avatar, HtmlNodeCollection elements)
        {
            try
            {
                Service service = _chat.GetServiceById(avatar.ServiceId);
                ServiceArea service_area = _chat.GetServiceAreaById(service.ServiceAreaId);
                foreach (var item in elements)
                {
                    try
                    {
                        var a_list_elements = item.SelectNodes(".//a");
                        string name = "";
                        string url = "";
                        foreach (var a_element in a_list_elements)
                        {
                            if (a_element != null && !string.IsNullOrWhiteSpace(a_element.InnerText.Trim()))
                            {
                                name = a_element.InnerText.Trim();
                                var attr = a_element.Attributes;
                                url = attr["href"].Value;
                                break;
                            }
                        }
                        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(name))
                        {
                            continue;
                        }

                        Models.Group group_to_add = _chat.AddGroup(name, url, service_area.State, avatar);
                        if (!avatar.Groups.Any(g => g.GroupId == group_to_add.GroupId))
                        {
                            avatar.Groups.Add(group_to_add);
                        }
                    }
                    catch { /* ignore malformed item */ }
                }
            }
            catch { /* ignore malformed item */ }

        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task UpdateAvatarGroupsList(Models.Avatar avatar, IWebDriver localDriver, CancellationToken token = default)
        {
            try
            {
                Navigate(localDriver, "https://www.facebook.com/groups/joins/");
                int loops = 20;
                avatar.Groups.Clear();
                await ScrollAllTheWayDown(avatar, localDriver, token, loops);
                await avatar.WaitAsync($"Update Groups:", TimeSpan.FromSeconds(1.5), token);
                try
                {
                    List<IWebElement> main_lists = WaitAll(localDriver, By.CssSelector("div[role='list']")).ToList();
                    if (main_lists.Count == 0)
                    {
                        return;
                    }
                    IWebElement using_main_list = main_lists.Count > 1 ? main_lists[1] : main_lists[0];
                    var html_string = (string)((IJavaScriptExecutor)localDriver).ExecuteScript("return arguments[0].outerHTML;", using_main_list);
                    var document = new HtmlAgilityPack.HtmlDocument();
                    document.LoadHtml(html_string);
                    var elements = document.DocumentNode.SelectNodes("//div[@role='listitem']");
                    GetGroupsUsingHtmlParsing(avatar, elements);
                    avatar._groupsMemberCount = avatar.Groups.Count;
                }
                catch (Exception ex)
                {
                    await avatar.WaitAsync($"Error with: Udating an group to avatar {ex} ", TimeSpan.FromSeconds(2), token);
                }
                avatar.LastGroupsUpdateTime = DateTime.Now;
                _chat.UpdateAvatar(avatar);
            }
            catch (Exception ex)
            {
                await avatar.WaitAsync($"Error with: PostToGroups {ex} ", TimeSpan.FromSeconds(2), token);
            }
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task<bool> AtPageEndAsync(Models.Avatar avatar, IWebDriver localDriver, CancellationToken token = default)
        {
            var js = (IJavaScriptExecutor)localDriver;
            var initialItems = localDriver.FindElements(By.CssSelector("div[role='listitem']")).Count;
            js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
            await avatar.WaitAsync($"scrolling down", TimeSpan.FromSeconds(1.5), token);
            var currentItems = localDriver.FindElements(By.CssSelector("div[role='listitem']")).Count;
            bool hasEndIndicator = localDriver.FindElements(By.XPath("//*[contains(text(), 'No more results') or contains(text(), 'End of results')]")).Count > 0;
            return initialItems == currentItems || hasEndIndicator;
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task UploadMarketPlacePost(Models.Avatar avatar, IWebDriver driver, MarketPlacePost ready_post_to_send, CancellationToken token = default)
        {
            Random random = new Random();
            IWebDriver localDriver = driver;
            try
            {

                StatusChanged?.Invoke(this, "Running");
                Navigate(localDriver, "https://www.facebook.com/marketplace/create/item");
                await Task.Delay(3000, token);

                IWebElement? fileInput = null;
                try
                {
                    fileInput = Wait(localDriver, By.CssSelector("input[type='file'][accept='image/*,image/heif,image/heic']"), timeout: 15);
                }
                catch
                {
                    try
                    {
                        await avatar.WaitAsync("Refresh Page  1st time", TimeSpan.FromSeconds(1), token);
                        Refresh(localDriver);
                        fileInput = Wait(localDriver, By.CssSelector("input[type='file'][accept='image/*,image/heif,image/heic']"), timeout: 15);
                    }
                    catch
                    {
                        if (IsBlockedPage(localDriver))
                        {
                            avatar._blocked = true;
                            await avatar.WaitAsync("This user is blocked", TimeSpan.FromSeconds(3), token);
                            return;
                        }
                        Navigate(localDriver, "https://www.facebook.com/marketplace/create/item");
                        await avatar.WaitAsync("try to Rerender page", TimeSpan.FromSeconds(1), token);
                        Refresh(localDriver);
                        await avatar.WaitAsync("Page has been Refreshed()", TimeSpan.FromSeconds(3), token);
                        try
                        {
                            fileInput = Wait(localDriver, By.CssSelector("input[type='file'][accept='image/*,image/heif,image/heic']"), timeout: 15);
                        }
                        catch
                        {
                            await avatar.WaitAsync("Can't Find Photos Input for user", TimeSpan.FromSeconds(3), token);
                            return;
                        }
                    }
                }
                if (fileInput != null)
                {
                    try
                    {
                        var filePaths = ready_post_to_send.photoPaths.Select(path => Path.GetFullPath(path)).ToArray();
                        fileInput.SendKeys(string.Join("\n", filePaths));
                        await Task.Delay(5000, token);
                        await avatar.WaitAsync("Photos Uploaded Successfully", TimeSpan.FromSeconds(3), token);
                    }
                    catch (Exception ex)
                    {
                        await avatar.WaitAsync($"Photo upload failed: {ex.Message}", TimeSpan.FromSeconds(3), token);
                    }
                }
                else
                {
                    await avatar.WaitAsync("File input element not found for photo upload", TimeSpan.FromSeconds(3), token);
                }
                await InputToMarketPlaceAsync(localDriver, avatar, ready_post_to_send, token);
                await avatar.WaitAsync("Post was Posted seccessfully", TimeSpan.FromSeconds(2), token);
            }
            catch
            {
                await avatar.WaitAsync("Error with: Run outside loop ", TimeSpan.FromSeconds(2), token);
            }
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task InputToMarketPlaceAsync(IWebDriver driver, Models.Avatar avatar, MarketPlacePost market_place_post, CancellationToken token = default)
        {
            List<IWebElement>? market_place_items_inputs = null;
            try
            {
                market_place_items_inputs = await Task.Run(() => driver.FindElements(By.CssSelector("input[type='text']")).ToList());
                await Task.Delay(500, token);
                var title_input = market_place_items_inputs[0];
                var price_input = market_place_items_inputs[1];
                SafeClick(driver, title_input);
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", title_input);
                await avatar.WaitAsync("Photos Uploaded Successfully", TimeSpan.FromSeconds(3), token);
                title_input.SendKeys(market_place_post._title);
                price_input.SendKeys(market_place_post._price);
                await avatar.WaitAsync("Title and price Uploaded Successfully", TimeSpan.FromSeconds(1), token);
                int category_res = await InputCategories(driver, avatar, market_place_post, token);
                await InputCondition(driver, market_place_post, token);
                await avatar.WaitAsync("Condition and Categories Uploaded Successfully", TimeSpan.FromSeconds(1), token);
            }
            catch { }
            try
            {
                var more_details_button = Wait(driver, By.CssSelector("div[data-visualcompletion] div[aria-expanded='false'][role='button']"), timeout: 10);
                await HumanScrollTo(driver, more_details_button, token);
                SafeClick(driver, more_details_button);
            }
            catch { }
            try
            {
                await avatar.WaitAsync("Human wait", TimeSpan.FromSeconds(2), token);
                market_place_items_inputs = await Task.Run(() => driver.FindElements(By.CssSelector("input[type='text']")).ToList());
                var brand_input = market_place_items_inputs[2];
                await HumanScrollTo(driver, brand_input, token);
                brand_input.SendKeys(market_place_post._brand);
            }
            catch { }
            try
            {
                List<IWebElement> textarea_input = WaitAll(driver, By.CssSelector("div[aria-label=\"Marketplace\"] textarea")).ToList();
                var description_input = textarea_input[0];
                description_input.SendKeys(market_place_post._description);
                var tag_input = textarea_input[1];
                tag_input.SendKeys(market_place_post._productTag);
            }
            catch { }
            try
            {
                await avatar.WaitAsync("Uploaded tag_input", TimeSpan.FromSeconds(2), token);
                await InputLocation(driver, avatar, market_place_post, token);
            }
            catch { }
            try
            {
                await avatar.WaitAsync("Uploaded Location", TimeSpan.FromSeconds(0.5), token);
                if (market_place_post._publicMeetup || market_place_post._doorPickUp || market_place_post._doorDropOff)
                {
                    List<IWebElement> delivery_inputs = WaitAll(driver, By.CssSelector("div[role=\"checkbox\"]")).ToList();
                    if (market_place_post._publicMeetup)
                    {
                        IWebElement public_meetup_input = delivery_inputs[0];
                        SafeClick(driver, public_meetup_input);
                    }
                    else if (market_place_post._doorPickUp)
                    {
                        IWebElement door_pick_up_input = delivery_inputs[1];
                        SafeClick(driver, door_pick_up_input);
                    }
                    else
                    {
                        IWebElement door_drop_off_input = delivery_inputs[2];
                        SafeClick(driver, door_drop_off_input);
                    }
                }
            }
            catch (Exception ex)
            {
                await avatar.WaitAsync($"Error InputToMarketPlaceAsync: {ex.Message}", TimeSpan.FromSeconds(1), token);
            }
            await avatar.WaitAsync("pickup Uploaded Successfully", TimeSpan.FromSeconds(1), token);
            try
            {
                var next_button = Wait(driver, By.CssSelector("div[aria-label='Next']"), timeout: 5);
                SafeClick(driver, next_button);
            }
            catch (Exception ex)
            {
                await avatar.WaitAsync($"Error InputToMarketPlaceAsync: {ex.Message}", TimeSpan.FromSeconds(1), token);
            }
            try
            {
                await avatar.WaitAsync("Next has been pressed Successfully", TimeSpan.FromSeconds(5), token);
                var publish = Wait(driver, By.CssSelector("div[aria-label='Publish']"), timeout: 5);
                SafeClick(driver, publish);
                await avatar.WaitAsync("Post has Been Published Successfully", TimeSpan.FromSeconds(7), token);
                ++avatar.PostsMadeToday;
                _chat.UpdateAvatar(avatar);
            }
            catch (Exception ex)
            {
                await avatar.WaitAsync($"Error couldnt update info InputToMarketPlaceAsync: {ex.Message}", TimeSpan.FromSeconds(2), token);
            }

        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task<int> InputCategories(IWebDriver driver, Models.Avatar avatar, MarketPlacePost market_place_post, CancellationToken token = default)
        {
            int res = 0;
            try
            {
                var categories_input = Wait(driver, By.CssSelector("input[aria-label='Category']"), 5);
                categories_input.Click();
                await avatar.WaitAsync("Inputing Categories", TimeSpan.FromSeconds(3), token);
                if (market_place_post._facebookCategory == "Other Bath Accessories")
                {
                    try
                    {
                        var clear_button = Wait(driver, By.CssSelector("div[aria-label='Clear']"));
                        clear_button.Click();
                        await avatar.WaitAsync($"Input Category", TimeSpan.FromSeconds(2), token);

                        var menu_list = WaitAll(driver, By.CssSelector("div[role='dialog'] div[data-visualcompletion] div[role='button']")).ToList();
                        if (menu_list != null && menu_list.Count > 0)
                        {
                            for (int i = 0; i < menu_list.Count; ++i)
                            {
                                var menu_item = menu_list[i];
                                if (menu_item.Text.Contains("Home"))
                                {
                                    menu_item.Click();
                                    await avatar.WaitAsync($"Input Category", TimeSpan.FromSeconds(2), token);
                                    i = menu_list.Count;
                                    menu_list = WaitAll(driver, By.CssSelector("div[role='dialog'] div[data-visualcompletion] div[role='button']")).ToList();
                                    for (int j = 0; j < menu_list.Count; ++j)
                                    {
                                        var inner_menu_item = menu_list[j];
                                        if (inner_menu_item.Text.Contains("Bath"))
                                        {
                                            inner_menu_item.Click();
                                            await avatar.WaitAsync($"Input Category", TimeSpan.FromSeconds(2), token);

                                            var inner_menu_list = WaitAll(driver, By.CssSelector("div[role='dialog'] div[data-visualcompletion] div[role='button']")).ToList();
                                            foreach (var inner_item in inner_menu_list)
                                            {
                                                if (inner_item.Text.Contains("Accessories"))
                                                {
                                                    var radio_menu_list = WaitAll(driver, By.CssSelector("div[role='dialog'] div[data-visualcompletion] div[role='radio']")).ToList();
                                                    foreach (var radio_item in radio_menu_list)
                                                    {
                                                        if (radio_item.Text.Contains("Other"))
                                                        {
                                                            radio_item.Click();
                                                            res = 1;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                    catch { }
                }
                else if (market_place_post._facebookCategory == "Other Home Heating & Cooling")
                {

                    try
                    {

                        try
                        {
                            var clear_button = Wait(driver, By.CssSelector("div[aria-label='Clear']"));
                            clear_button.Click();
                        }
                        catch { }

                        var menu_list = WaitAll(driver, By.CssSelector("div[role='dialog'] div[data-visualcompletion] div[role='button']")).ToList();
                        if (menu_list != null && menu_list.Count > 0)
                        {
                            for (int i = 0; i < menu_list.Count; ++i)
                            {
                                var menu_item = menu_list[i];
                                if (menu_item.Text.Contains("Tools &"))
                                {
                                    menu_item.Click();
                                    await avatar.WaitAsync($"Input Category", TimeSpan.FromSeconds(2), token);
                                    i = menu_list.Count;
                                    menu_list = WaitAll(driver, By.CssSelector("div[role='dialog'] div[data-visualcompletion] div[role='button']")).ToList();
                                    for (int j = 0; j < menu_list.Count; ++j)
                                    {
                                        var inner_menu_item = menu_list[j];
                                        if (inner_menu_item.Text.Contains("Home Heating"))
                                        {
                                            inner_menu_item.Click();
                                            await avatar.WaitAsync($"Input Category", TimeSpan.FromSeconds(2), token);

                                            var radio_menu_list = WaitAll(driver, By.CssSelector("div[role='dialog'] div[data-visualcompletion] div[role='radio']")).ToList();
                                            foreach (var inner_radio_item in radio_menu_list)
                                            {
                                                if (inner_radio_item.Text.Contains("Other"))
                                                {
                                                    inner_radio_item.Click();
                                                    await avatar.WaitAsync($"Input Category", TimeSpan.FromSeconds(2), token);

                                                    res = 1;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }
                else
                {
                    categories_input.SendKeys(market_place_post._facebookCategory);
                    await avatar.WaitAsync("Inputing Categories", TimeSpan.FromSeconds(3), token);
                    var options = driver.FindElements(By.CssSelector("li[role='option']")).ToList();
                    if (options.Count > 0)
                    {
                        SafeClick(driver, options[0]);
                        res = 1;
                    }
                }
            }
            catch
            {
                var combo = Wait(driver, By.CssSelector("label[role='combobox']"), 5);
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", combo);
                SafeClick(driver, combo);
                await avatar.WaitAsync($"InputCategories wait", TimeSpan.FromSeconds(1), token);
                var menu_items = driver.FindElements(By.CssSelector("div[aria-label='Dropdown menu'] div[data-visualcompletion] div[role='button']")).ToList();
                var menu_items_string = new List<string>();
                foreach (var item in menu_items)
                {
                    try
                    {
                        menu_items_string.Add(item.Text);
                    }
                    catch (Exception ex)
                    {
                        await avatar.WaitAsync($"there was an error InputCategories {ex.Message}", TimeSpan.FromSeconds(3), token);
                    }
                }
                string choosen_category = await GetCategoryAsync(avatar, URLGETCATEGORY, menu_items_string, market_place_post._industry);
                if (menu_items.Count > 0)
                {
                    int i = 0;
                    foreach (IWebElement? item in menu_items)
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", item);
                        await avatar.WaitAsync($"InputCategories human wait", TimeSpan.FromMilliseconds(500), token);
                        if (item.Text.ToLower().Contains(choosen_category.ToLower()))
                        {
                            SafeClick(driver, item);
                            res = 0;
                            break;
                        }
                        ++i;
                    }
                    if (i == menu_items.Count)
                    {
                        menu_items[i - 3].Click();
                    }
                }
                res = 2;
            }
            await avatar.WaitAsync($"InputCategories is done", TimeSpan.FromMilliseconds(1000), token);
            return res;
        }
        // ──────────────────────────────────────────────────────────────────────────────

        public async Task<bool> InputCondition(IWebDriver driver, MarketPlacePost market_place_post, CancellationToken token = default)
        {
            bool res = true;
            bool is_condition = false;
            try
            {
                var condition_input = Wait(driver,
                    By.CssSelector("div[aria-label='Marketplace'] label[aria-haspopup='listbox']"),
                    timeout: 5);

                ((IJavaScriptExecutor)driver)
                    .ExecuteScript("arguments[0].scrollIntoView(true);", condition_input);

                condition_input.Click();
                condition_input.Click();
                condition_input.Click();

                var drop_down = driver.FindElements(
                    By.CssSelector("div[role='listbox'] div[role='option']")).ToList();

                for (int i = 0; i < drop_down.Count; i++)
                {
                    var item = drop_down[i];
                    string item_text = item.Text;
                    if (item_text.ToLower().Contains(market_place_post._condition.ToLower())
                        || i == drop_down.Count - 1)
                    {
                        condition_input.Click();
                        condition_input.Click();
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", item);
                        SafeClick(driver, item);
                        await Task.Delay(1000, token);
                        is_condition = true;
                        break;
                    }
                }

                if (!is_condition && drop_down.Count > 0)
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", drop_down[0]);
                    SafeClick(driver, drop_down[0]);
                    await Task.Delay(1000, token);
                }
            }
            catch
            {
                res = false;
            }

            await Task.Delay(1000, token);
            return res;
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task<bool> InputLocation(IWebDriver driver, Models.Avatar avatar, MarketPlacePost market_place_post, CancellationToken token = default)
        {
            bool res = false;
            try
            {
                var locInputs = driver
                    .FindElements(By.CssSelector("input[type='text'][aria-label='Location']"))
                    .ToList();

                if (locInputs.Count > 0)
                {
                    var location_input = locInputs[0];
                    await HumanScrollTo(driver, location_input, token);
                    SafeClick(driver, location_input);
                    await avatar.WaitAsync("Inputing location", TimeSpan.FromSeconds(5), token);
                    ((IJavaScriptExecutor)driver).ExecuteScript($"arguments[0].value = '{market_place_post._location}';", location_input);
                    await avatar.WaitAsync("Inputing location", TimeSpan.FromSeconds(5), token);
                    location_input.SendKeys(" ");
                    await avatar.WaitAsync("Inputing location", TimeSpan.FromSeconds(3), token);

                    var suggestion = Wait(driver,
                        By.CssSelector("ul[role='listbox'] li"), timeout: 5);
                    SafeClick(driver, suggestion);
                    res = true;
                }
            }
            catch
            {
                res = false;
            }

            await Task.Delay(1000, token);
            return res;
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task<string> GetCategoryAsync(Models.Avatar avatar, string serverUrl, List<string> menu_items_string, string industry)
        {
            Service service = _chat.GetServiceById(avatar.ServiceId);
            ServiceArea service_area = _chat.GetServiceAreaById(service.ServiceAreaId);
            Company company = _chat.GetCompanyById(service_area.CompanyId);
            var payload = new
            {
                list = menu_items_string,
                industry = industry
            };
            using var client = new HttpClient();
            var resp = await client.PostAsJsonAsync(serverUrl, payload);
            resp.EnsureSuccessStatusCode();
            string response_dict = await resp.Content.ReadAsStringAsync();
            CategoryReturnValue res_val = JsonConvert.DeserializeObject<CategoryReturnValue>(response_dict) ?? new CategoryReturnValue(string.Empty);
            return res_val.category;
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task<string> RandText(Models.Avatar avatar, string user_name, Conversation conversation)
        {
            string response = "";
            try
            {

                response = await RandTextAsync(avatar, URLAUTOANSWER, user_name, conversation);
            }
            catch
            {
                OnStatusChanged("There was a problem with bot");
            }

            return response;
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task<string> RandTextAsync(Models.Avatar avatar, string serverUrl, string customer_name, Conversation conversation)
        {
            Service service = _chat.GetServiceById(avatar.ServiceId);
            ServiceArea service_area = _chat.GetServiceAreaById(service.ServiceAreaId);
            Company company = _chat.GetCompanyById(service_area.CompanyId);
            var payload = new
            {
                company_name = company.Name,
                facebook_category = service.FacebookCategory,
                industry = service.Industry,
                location = $"{service_area.City.Trim()} {service_area.State.Trim().ToUpper()}",
                phone_number = service.PhoneNumber,
                customer_name = customer_name,
                avatar_name = avatar._userName,
                messages = new List<string>(conversation.Messages.Select(m => m.Text))
            };
            using var client = new HttpClient();
            var resp = await client.PostAsJsonAsync(serverUrl, payload);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsStringAsync();
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task HumanScrollTo(IWebDriver driver, IWebElement target, CancellationToken tok)
        {
            var js = (IJavaScriptExecutor)driver;
            var rnd = new Random();
            for (int i = 0; i < rnd.Next(3, 6) && !tok.IsCancellationRequested; i++)
            {
                js.ExecuteScript($"window.scrollBy(0,{rnd.Next(300, 600)});");
                await Task.Delay(rnd.Next(350, 800), tok);
                if (rnd.NextDouble() < 0.2)
                    js.ExecuteScript($"window.scrollBy(0,{rnd.Next(-120, -60)});");
            }
            js.ExecuteScript("arguments[0].scrollIntoView({block:'center',behavior:'smooth'});", target);
            await Task.Delay(rnd.Next(600, 1000), tok);
            new Actions(driver).ScrollByAmount(0, rnd.Next(-60, 60)).Perform();
        }
        // ──────────────────────────────────────────────────────────────────────────────
        private List<string> LoadKeywords(Models.Avatar avatar)
        {
            var keywordsFile = Path.Combine(avatar._avatarFolderPath, $"keywords.json");
            if (File.Exists(keywordsFile))
            {
                var json = File.ReadAllText(keywordsFile);
                var deserialized_json = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
                return deserialized_json;
            }
            return new List<string>(); // API will generate initial keywords if empty
        }
        // ──────────────────────────────────────────────────────────────────────────────
        private List<string> GetRandomPhotoPaths(Models.Avatar avatar, int count)
        {
            var photoDirectory = Path.Combine(avatar._avatarFolderPath, "PhotosForPosts");
            if (!Directory.Exists(photoDirectory))
            {
                Directory.CreateDirectory(photoDirectory);
                return new List<string>();
            }
            var photos = Directory.GetFiles(photoDirectory, "*.jpeg").ToList();
            if (photos.Count == 0)
            {
                return new List<string>();
            }
            var random = new Random();
            return photos.OrderBy(x => random.Next()).Take(count).ToList();
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public static bool HasProfilePicture(IWebDriver driver)
        {
            var img = driver.FindElement(By.CssSelector("div[aria-label='Profile picture actions'] image"));
            var url = img.GetAttribute("xlink:href") ?? string.Empty;
            return !Regex.IsMatch(url, @"/t1\.") && !url.Contains("dst-png");
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public static IWebElement? GetFriendsElement(IWebDriver driver)
        {
            return driver?.FindElements(By.TagName("span")).FirstOrDefault(span => _friendsRegex.IsMatch(span.Text ?? string.Empty));
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public static int GetFriendsCount(IWebDriver driver)
        {
            var freiend_element = GetFriendsElement(driver);
            return freiend_element != null && int.TryParse(Regex.Replace(freiend_element.Text, @"[^\d]", ""), out var n) ? n : -1;
        }
        // ──────────────────────────────────────────────────────────────────────────────
        private async Task<Dictionary<string, string>> InitAvatarInfo(IWebDriver driver, Models.Avatar avatar, CancellationToken token = default)
        {
            Dictionary<string, string> userin_info = new();
            var js = (IJavaScriptExecutor)driver;
            var profile_button = Wait(driver, By.CssSelector("div[aria-label='Your profile']"));
            SafeClick(driver, profile_button);
            await avatar.WaitAsync("Wait to click profile", TimeSpan.FromSeconds(1), token);
            var profile_link_button = Wait(driver, By.CssSelector("div[aria-label='Your profile'] a[role='link']"));
            SafeClick(driver, profile_link_button);
            await avatar.WaitAsync("Wait to click profile", TimeSpan.FromSeconds(3), token);
            string user_link = driver.Url ?? string.Empty;
            var h1_list = driver.FindElements(By.CssSelector("h1"));
            string user_name = h1_list[h1_list.Count - 1].Text;
            bool has_profile_picture = HasProfilePicture(driver);
            js.ExecuteScript("window.scrollBy(0, window.innerHeight)");
            js.ExecuteScript("window.scrollBy(0, window.innerHeight)");
            await avatar.WaitAsync("Wait to click profile", TimeSpan.FromSeconds(1), token);
            js.ExecuteScript("window.scrollBy(0, window.innerHeight)");
            await avatar.WaitAsync("Get User Information", TimeSpan.FromSeconds(3), token);
            js.ExecuteScript("window.scrollBy(0, window.innerHeight)");
            int freinds_count;
            try
            {
                await avatar.WaitAsync("Get User Friends Count", TimeSpan.FromSeconds(3), token);
                freinds_count = GetFriendsCount(driver);
                freinds_count = freinds_count != -1 ? freinds_count : 0;
            }
            catch
            {
                freinds_count = 0;
            }

            await UpdateAvatarGroupsList(avatar, driver, token);
            avatar._userName = user_name;
            avatar._linkForAvatarProfile = user_link;
            avatar._freindsCount = freinds_count > 0 ? freinds_count : 0;
            avatar._hasProfilePicture = has_profile_picture;
            _chat.UpdateAvatar(avatar);
            return userin_info;
        }
        // ──────────────────────────────────────────────────────────────────────────────
        private void SaveKeywords(Models.Avatar avatar, List<string> keywords)
        {
            var keywordsFile = Path.Combine(avatar._avatarFolderPath, $"keywords.json");
            var json = JsonConvert.SerializeObject(keywords);
            File.WriteAllText(keywordsFile, json);
        }
        // ──────────────────────────────────────────────────────────────────────────────
        private async Task<JObject> SendPostRequest(object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(URLPOSTS, content);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return JObject.Parse(responseJson);
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public ConversationQueue GetNChatsMarketPlaceFastHtmlOnly(Models.Avatar avatar, int N, HtmlNodeCollection elements, List<IWebElement> chats_elements)
        {
            List<Conversation> conversations = new List<Conversation>();

            for (int i = 0; i < elements.Count && i < N; i++)
            {
                bool has_unread_messages = false;
                var a_node_arr = elements[i].SelectNodes(".//a");
                var a_node = a_node_arr.Count > 0 ? a_node_arr[0] : null;
                string attr_href = a_node?.Attributes["href"].Value;
                string conversation_link = $"https://www.facebook.com{attr_href}";
                string conversation_id = Regex.Match(attr_href, @"(?<=/t/)\d+(?=/)").Value;
                string conversation_type = conversation_link.Contains("e2ee") ? "private" : "channel";
                string has_unread_messages_selector = ".//div[@role=\"button\"]/span[@data-visualcompletion]";
                var span_has_unread_messages = a_node?.SelectSingleNode(has_unread_messages_selector);
                if (span_has_unread_messages != null)
                {
                    has_unread_messages = true;
                }
                string xpath_for_info = ".//div[contains(concat(\" \", @class, \" \"), \" html-div \")]/span";
                var responses_info_spans = a_node?.SelectNodes(xpath_for_info);
                var responses_info_span_0 = responses_info_spans != null ? responses_info_spans[0] : null;
                var responses_info_span_1 = responses_info_spans != null ? responses_info_spans[1] : null;
                var responses_info_span_3 = responses_info_spans != null ? responses_info_spans[3] : null;
                string[] user_name_arr = responses_info_span_0 != null ? responses_info_span_0.InnerText.Split("·") : new string[5];
                string user_name = user_name_arr.Length > 0 ? user_name_arr[0].Trim() : "";
                string contant = responses_info_span_1 != null ? responses_info_span_1.InnerText : "";
                string last_message_time_str = responses_info_span_3 != null ? responses_info_span_3.InnerText : "";
                DateTime last_message_time = StaticHelpers.ParseLastMessageTime(last_message_time_str);
                var conversation = new Conversation(avatar, conversation_id, chats_elements[i],
                                                   conversation_link, conversation_type, has_unread_messages,
                                                   user_name, contant, last_message_time);
                conversations.Add(conversation);
            }
            return new ConversationQueue(conversations);
        }
        // ──────────────────────────────────────────────────────────────────────────────
        [TearDown]
        public void Dispose(IWebDriver local_driver)
        {
            lock (_myLock)
            {
                IWebDriver driver = local_driver;
                try
                {

                    driver?.Quit();
                    driver?.Close();
                }
                catch { }
                //_webDriverThreadLocal.Value = null;
                //_driver = null;
            }
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public void Refresh(IWebDriver driver) => driver.Navigate().Refresh();
        // ──────────────────────────────────────────────────────────────────────────────
        public bool IsBlockedPage(IWebDriver driver)
        {
            bool is_blocked_page = false;
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                IWebElement span = wait.Until(d => d.FindElements(By.TagName("span")).FirstOrDefault(s => s.Text.Contains("confirm you're human to use your account")));
                if (span != null)
                {
                    is_blocked_page = true;
                }
            }
            catch (WebDriverTimeoutException)
            {
            }
            return is_blocked_page;

        }
        // ──────────────────────────────────────────────────────────────────────────────
        public void Navigate(IWebDriver driver, string url) => driver.Navigate().GoToUrl(url);
        // ──────────────────────────────────────────────────────────────────────────────
        public IWebElement Wait(IWebDriver driver, By by, int timeout = 10)
        {
            return new WebDriverWait(driver, TimeSpan.FromSeconds(timeout)).Until(d => d.FindElement(by));
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public ICollection<IWebElement> WaitAll(IWebDriver driver, By by, int timeout = 10)
        {
            return new WebDriverWait(driver, TimeSpan.FromSeconds(timeout)).Until(d => d.FindElements(by));
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public void Click(IWebDriver driver, By by, int timeout = 10) => Wait(driver, by, timeout).Click();
        // ──────────────────────────────────────────────────────────────────────────────
        public void SetKeys(IWebDriver driver, By by, string text, int timeout = 10)
        {
            var el = Wait(driver, by, timeout);
            el.Clear();
            el.SendKeys(text);
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public void PressEnter(IWebDriver driver, By by) => Wait(driver, by).SendKeys(OpenQA.Selenium.Keys.Enter);
        // ──────────────────────────────────────────────────────────────────────────────
        private void OnStatusChanged(string newStatus)
        {
            StatusChanged?.Invoke(this, newStatus);
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public bool IsMoreThanAWeekSinceLastUpdate(DateTime last_groups_update_time)
        {
            return (DateTime.Now - last_groups_update_time).TotalDays > 7;
        }
        //*──────────────────────────────────────────────────────────────────────────────*//
        public bool IsLoggedIn(IWebDriver driver, TimeSpan delay_span)
        {
            return new WebDriverWait(driver, delay_span).Until(d => d.FindElements(By.CssSelector("div[aria-label='Create a post']")).Count > 0);
        }
        //*──────────────────────────────────────────────────────────────────────────────*//
        public static IWebElement ToWebElement(HtmlNode node, IWebDriver driver)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (driver == null) throw new ArgumentNullException(nameof(driver));
            return driver.FindElement(By.XPath(node.XPath));
        }
        //*──────────────────────────────────────────────────────────────────────────────*//
        void OnLog(string message)
        {
            LogMessage?.Invoke(this, $"Bot {_id}: {message}");
        }
    }
}

