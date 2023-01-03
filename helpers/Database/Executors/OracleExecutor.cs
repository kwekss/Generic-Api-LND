using helpers.Database.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace helpers.Database.Executors
{
    public class OracleExecutor : IOracleExecutor
    {
        public readonly int _connectionAttemptCount = 20;

        public OracleExecutor() { }
        public async Task<OracleConnection> PrepareConnection(string connectionString)
        {

            for (int i = 0; i < _connectionAttemptCount; i++)
            {
                try
                {
                    OracleConnection con = new OracleConnection(connectionString);
                    await con.OpenAsync();

                    return con;
                }
                catch (Exception x)
                {
                    if (i == _connectionAttemptCount - 1)
                    {
                        throw x;
                    }
                }
            }
            throw new ApplicationException("Unable to connect to ORACLE DB");
        }

        public async Task ExecuteQuery(Connection _connection, string query, Action<IDataReader> callback = null)
        {
            using (OracleConnection connection = await PrepareConnection(_connection.ConnectionString))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand(query, connection);
                    cmd.BindByName = true;
                    cmd.CommandTimeout = 600;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (callback != null) callback.Invoke(reader);
                    }

                    CloseConnection(connection, cmd);
                }
                catch (Exception)
                {
                    CloseConnectionOnly(connection);
                    throw;
                }

            }
        }



        public async Task ExecuteNonQueryStoredProcedure(Connection _connection, string procedure_name, List<OracleStoreProcedureParameter> parameters, Action<OracleParameterCollection> callback = null)
        {
            using (OracleConnection connection = await PrepareConnection(_connection.ConnectionString))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand($"{_connection.Schema}.{procedure_name}", connection);
                    cmd.BindByName = true;
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (parameters != null && parameters.Any())
                    {
                        foreach (OracleStoreProcedureParameter parameter in parameters)
                        {
                            cmd.Parameters.Add(parameter.Name, parameter.Type, parameter.Size).Value = parameter.Value;
                            cmd.Parameters[parameter.Name].Direction = parameter.Direction;
                        }
                    }

                    var reader = await cmd.ExecuteNonQueryAsync();
                    if (callback != null) callback.Invoke(cmd.Parameters);


                    CloseConnection(connection, cmd);
                }
                catch (Exception)
                {
                    CloseConnectionOnly(connection);
                    throw;
                }

            }
        }

        public async Task ExecuteStoredProcedure(Connection _connection, string procedure_name, List<OracleStoreProcedureParameter> parameters, Action<IDataReader> callback = null)
        {
            using (OracleConnection connection = await PrepareConnection(_connection.ConnectionString))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand(procedure_name, connection);
                    cmd.BindByName = true;
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (parameters != null && parameters.Any())
                    {
                        foreach (OracleStoreProcedureParameter parameter in parameters)
                        {
                            cmd.Parameters.Add(parameter.Name, parameter.Type, parameter.Size).Value = parameter.Value;
                            cmd.Parameters[parameter.Name].Direction = parameter.Direction;
                        }
                    }

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (callback != null) callback.Invoke(reader);
                    }

                    CloseConnection(connection, cmd);
                }
                catch (Exception)
                {
                    CloseConnectionOnly(connection);
                    throw;
                }

            }
        }





        private void CloseConnection(OracleConnection connection, OracleCommand command)
        {
            command.Dispose();
            connection.Dispose();
            connection.Close();
        }

        private void CloseConnectionOnly(OracleConnection connection)
        {
            connection.Dispose();
            connection.Close();
        }
    }
}
