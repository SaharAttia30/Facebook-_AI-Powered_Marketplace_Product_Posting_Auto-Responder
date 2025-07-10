namespace ChatBlaster.Models
{
    public class Service
    {
        public string ServiceId { get; set; }
        public string Industry { get; set; }
        public string FacebookCategory { get; set; }
        public string PhoneNumber { get; set; }
        public string ServiceAreaId { get; set; }
        public virtual ServiceArea ServiceArea { get; set; }
        public virtual ICollection<Avatar> Avatars { get; set; } = new List<Avatar>();
        public Service()
        {
            ServiceId = Guid.NewGuid().ToString();
            Avatars = new List<Avatar>();
        }
    }
}
