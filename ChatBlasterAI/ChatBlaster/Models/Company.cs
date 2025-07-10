namespace ChatBlaster.Models
{
    public class Company
    {
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string OwnerName { get; set; }
        public string OwnerPhone { get; set; }
        public string OfficePhone { get; set; }
        public virtual ICollection<ServiceArea> ServiceAreas { get; set; }
        public Company()
        {
            ServiceAreas = new List<ServiceArea>();
        }
        
    }
}
