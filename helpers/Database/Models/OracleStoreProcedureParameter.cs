using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace helpers.Database.Models
{
    public class OracleStoreProcedureParameter
    {
        public OracleStoreProcedureParameter()
        {
            Type = OracleDbType.Varchar2;
            Direction = ParameterDirection.Input;
            Size = 200;
        }
        public string Name { get; set; }

        public OracleDbType Type { get; set; }

        public object Value { get; set; }
        public int Size { get; set; }
        public ParameterDirection Direction { get; set; }
    }
}
