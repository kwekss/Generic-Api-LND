using helpers.Database;
using helpers.Database.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace helpers.Engine
{
    public class HealthCheck
    {
        private readonly IDBHelper _dbHelper;

        public HealthCheck(IDBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }
        public async Task<string> CheckPgDatabase(Connection conn)
        {
            try
            {
                var version = await _dbHelper.ExecuteRaw<string>(conn, "version", new List<StoreProcedureParameter> { });
                return version;
            }
            catch (Exception e) { }
           
            return null;
        }
        public async Task<string> CheckSystem()
        {
            return "System OK";
        }
    }
}
