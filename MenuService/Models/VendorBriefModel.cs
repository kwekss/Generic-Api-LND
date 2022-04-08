using System.Collections.Generic;

namespace MenuService.Models
{
    public class VendorBriefModel
    {
        public string VendorId { get; set; }
        public string VendorName { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string VendorLogo { get; set; }
        public List<VendorWorkingHours> WorkingHours { get; set; }
        public List<VendorContact> Contacts { get; set; }
    }
}
