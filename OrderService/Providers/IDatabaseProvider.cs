using OrderService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Providers
{
    public interface IDatabaseProvider
    {
        Task<List<OrderModel>> GetAllOrders();
    }
}
