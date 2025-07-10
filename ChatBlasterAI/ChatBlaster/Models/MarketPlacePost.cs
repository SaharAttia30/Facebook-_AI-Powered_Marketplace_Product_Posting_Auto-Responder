namespace ChatBlaster.Models
{
    public class MarketPlacePost
    {
        public List<string> photoPaths { get; set; }
        private static readonly object _lock = new();
        public string _createIteamUrl { get; set; }
        public string _title { get; set; }
        public string Id { get; set; }
        public string _price { get; set; }
        public string _industry { get; set; }
        public string _facebookCategory { get; set; }
        public string _condition { get; set; }
        public string _brand { get; set; }
        public string _description { get; set; }
        public string _productTag { get; set; }
        public string _location { get; set; }
        public bool _publicMeetup { get; set; }
        public bool _doorPickUp { get; set; }
        public bool _doorDropOff { get; set; }
        public MarketPlacePost()
        {
        }
        public MarketPlacePost(string title, string price, string facebook_category,string industry, string condition, string brand, string description, string product_tag, string location, bool public_meetup, bool door_pick_up, bool door_drop_off)
        {
            _title = title;
            _price = price;
            _createIteamUrl = "https://www.facebook.com/marketplace/create/item";
            _facebookCategory = facebook_category;
            _industry = industry;
            _condition = condition;
            _brand = brand;
            _description = description;
            _productTag = product_tag;
            _location = location;
            if (public_meetup)
            {
                _publicMeetup = true;
                _doorPickUp = false;
                _doorDropOff = false;
            }
            else if (door_pick_up)
            {
                _publicMeetup = false;
                _doorPickUp = true;
                _doorDropOff = false;
            }
            else
            {
                _publicMeetup = false;
                _doorPickUp = false;
                _doorDropOff = true;
            }
        }
    }
}
