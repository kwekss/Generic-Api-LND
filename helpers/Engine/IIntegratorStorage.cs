using models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace helpers.Engine
{
    public interface IIntegratorStorage
    {
        Task<List<Integrator>> GetIntegrators();
    }
}