using helpers;
using helpers.Atttibutes;
using MenuService.Models;
using MenuService.Providers;
using models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenuService.Features
{
    [Feature(Name = "GetMealsByVendorId")]
    public class GetMealsByVendorIdFeature : BaseServiceFeature
    {
        private readonly IDatabaseProvider _databaseProvider;

        public GetMealsByVendorIdFeature(IDatabaseProvider databaseProvider) : base()
        {
            _databaseProvider = databaseProvider;
        }

        [Entry(Method = "GET", Route = "{id}")]
        public async Task<ApiResponse> GetMealByVendorId(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return new ApiResponse { Success = false, ResponseMessage = "Vendor Id is required" };
            if (!Guid.TryParse(id, out Guid uuid)) return new ApiResponse { Success = false, ResponseMessage = "Vendor Id must be a valid Guid" };

            var meals = await _databaseProvider.GetMealsByVendorId(id);
            if (!meals.Any()) meals = new List<MealModel>();

            return new ApiResponse { Success = true, Data = meals };
        }

    }
}
