namespace helpers.Database.Models
{
    public class StoreProcedureParameter
    {
        public StoreProcedureParameter()
        {
            Type = NpgsqlTypes.NpgsqlDbType.Varchar;
        }
        public string Name { get; set; }

        public NpgsqlTypes.NpgsqlDbType Type { get; set; }

        public object Value { get; set; }
    }
}
