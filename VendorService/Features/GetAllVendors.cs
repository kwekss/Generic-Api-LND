using helpers;
using helpers.Atttibutes;
using helpers.Database;
using models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VendorService.Models;
using VendorService.Providers;

namespace VendorService.Features
{
    [Feature(Name = "GetAllVendors")]
    public class GetAllVendorsFeature : BaseServiceFeature
    {
        private readonly IDatabaseProvider _databaseProvider;
        public GetAllVendorsFeature(IDatabaseProvider databaseProvider) : base()
        {
            _databaseProvider = databaseProvider;
        }

        [Entry(Method = "GET")]
        public async Task<ApiResponse> GetAllVendors()
        {

            var vendors = await _databaseProvider.GetAllVendors();
            if (vendors == null) return new ApiResponse { Success = false, ResponseMessage = $"Vendors with  not found" };

            return new ApiResponse { Success = true, Data = vendors };
        }

    }
}
