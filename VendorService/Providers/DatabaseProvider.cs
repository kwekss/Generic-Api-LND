using helpers.Database;
using helpers.Database.Models;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VendorService.Models;

namespace VendorService.Providers
{
    public class DatabaseProvider : IDatabaseProvider
    {
        private readonly IDBHelper _dbHelper;
        public DatabaseProvider(IDBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<List<VendorModel>> GetAllVendors()
        {

            return await _dbHelper.Fetch<VendorModel>("GetAllVendors", null);

        }

        public async Task<InputVendorModel> InsertVendor(InputVendorModel vendor)
        {
            var parameters = new List<StoreProcedureParameter> {
                new StoreProcedureParameter{Name = "reqVendorName", Type = NpgsqlDbType.Varchar, Value = vendor.VendorName},
                new StoreProcedureParameter{Name = "reqLocation", Type = NpgsqlDbType.Varchar, Value = vendor.Location},
                new StoreProcedureParameter{Name = "reqDescription", Type = NpgsqlDbType.Varchar, Value = vendor.Description},
                new StoreProcedureParameter{Name = "reqVendorLogo", Type = NpgsqlDbType.Varchar, Value = vendor.VendorLogo},
            };

            return await _dbHelper.ExecuteRaw<InputVendorModel>("InsertVendor", parameters);

        }
    }
}
