namespace ChatBlaster.Models
{
    public class ServiceArea
    {
        public string ServiceAreaId { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string SurroundingCities { get; set; }
        public int Radius { get; set; }
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public virtual ICollection<Service> Services { get; set; }
        public ServiceArea()
        {
            ServiceAreaId = Guid.NewGuid().ToString();
            Services = new List<Service>();
        }
    }
}
