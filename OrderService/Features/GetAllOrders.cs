using helpers;
using helpers.Atttibutes;
using models;
using OrderService.Providers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Features
{
    [Feature(Name = "GetAllOrders")]
    public class GetAllOrdersFeature : BaseServiceFeature
    {
        private readonly IDatabaseProvider _databaseProvider;
        public GetAllOrdersFeature(IDatabaseProvider databaseProvider) : base()
        {
            _databaseProvider = databaseProvider;
        }

        [Entry(Method = "GET")]
        public async Task<ApiResponse> GetAllOrders()
        {

            var orders = await _databaseProvider.GetAllOrders();
            if (orders == null) return new ApiResponse { Success = false, ResponseMessage = $"Orders  not found" };

            return new ApiResponse { Success = true, Data = orders };
        }

    }
}
