using helpers.Interfaces;
using models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace helpers.Engine
{
    public class IntegratorStorage : IIntegratorStorage
    {
        protected static List<Integrator> _inMemoryIntegratorList;
        public IntegratorStorage()
        {
            _inMemoryIntegratorList = GetJsonIntegrators();
        }


        public async Task<List<Integrator>> GetIntegrators() => _inMemoryIntegratorList;

        private List<Integrator> GetJsonIntegrators()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Integrators.json");
            if (!File.Exists(path)) return new List<Integrator> { } ;

            using (var reader = new StreamReader(path))
            {
                var result = reader.ReadToEnd();
                var integrators = result.ParseObject<List<Integrator>>();
                return integrators;
            }
        }
    }
}
