using System;
using System.Threading.Tasks;

namespace helpers.Interfaces
{
    public interface IIntegratorHelper
    { 
        Task<string> ValidateIntegrator(string authorizationString, DateTime requestTime);
    }
}