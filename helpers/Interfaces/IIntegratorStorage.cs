using models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace helpers.Interfaces
{
    public interface IIntegratorStorage
    {
        Task<List<Integrator>> GetIntegrators();
    }
}