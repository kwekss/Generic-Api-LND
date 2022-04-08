using helpers.Database;
using OrderService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Providers
{
    public class DatabaseProvider: IDatabaseProvider
    {
        private readonly IDBHelper _dbHelper;
        public DatabaseProvider(IDBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<List<OrderModel>> GetAllOrders()
        {

            return await _dbHelper.Fetch<OrderModel>("GetAllOrders", null);

        }
    }
}
