using helpers.Database.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace helpers.Database.Executors
{
    public interface IOracleExecutor
    {
        Task ExecuteNonQueryStoredProcedure(Connection _connection, string procedure_name, List<OracleStoreProcedureParameter> parameters, Action<OracleParameterCollection> callback = null);
        Task ExecuteQuery(Connection _connection, string query, Action<IDataReader> callback = null);
        Task ExecuteStoredProcedure(Connection _connection, string procedure_name, List<OracleStoreProcedureParameter> parameters, Action<IDataReader> callback = null);
        Task<OracleConnection> PrepareConnection(string connectionString);
    }
}