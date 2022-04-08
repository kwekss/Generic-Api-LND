using System.Collections.Generic;
using System.Threading.Tasks;
using VendorService.Models;

namespace VendorService.Providers
{
    public interface IDatabaseProvider
    {
        Task<List<VendorModel>> GetAllVendors();
        Task<InputVendorModel> InsertVendor(InputVendorModel vendor);
    }
}