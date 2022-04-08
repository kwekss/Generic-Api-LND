using helpers.Database;
using helpers.Database.Models;
using MenuService.Models;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MenuService.Providers
{
    public class DatabaseProvider: IDatabaseProvider
    {
        private readonly IDBHelper _dbHelper;
        public DatabaseProvider(IDBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<List<MealModel>> GetMealsByVendorId(string id)
        {
            var parameters = new List<StoreProcedureParameter> {
                new StoreProcedureParameter{Name = "reqVendorId", Type = NpgsqlDbType.Varchar, Value = id}
            };
            return await _dbHelper.Fetch<MealModel>("GetMealsByVendorId", parameters);
        }
        
    }
}
