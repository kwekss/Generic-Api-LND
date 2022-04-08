using System;
using System.Collections.Generic;
using System.Text;

namespace VendorService.Models
{
    public class VendorModel
    {
        public string VendorId { get; set; }
        public string VendorName { get; set; }
        public string Location { get; set; }
        public string VendorLogo { get; set; }
        public List<VendorWorkingHours> WorkingHours { get; set; }
        public List<VendorContact> Contacts { get; set; }
        public bool Active { get; set; }
    }
}
