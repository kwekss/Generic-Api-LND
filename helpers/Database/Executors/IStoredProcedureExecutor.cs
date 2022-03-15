using helpers.Database.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace helpers.Database.Executors
{
    public interface IStoredProcedureExecutor
    {
        Task ExecuteStoredProcedure(Connection _connection, string storeProcedureName, List<StoreProcedureParameter> parameters, Action<IDataReader> callback = null);
        Task<List<T>> ExecuteStoredProcedure<T>(Connection connection, string storeProcedureName, List<StoreProcedureParameter> parameters);
        Task Subscribe(Connection connection, string notificationTag, NotificationEventHandler handler);
    }
}
