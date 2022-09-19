using System;
using System.Threading.Tasks;

namespace helpers.Interfaces
{
    public interface IIntegratorHelper
    {
        string Sha256encrypt(DateTime requestTime, string integratorSecretKey);
        Task<string> ValidateIntegrator(string authorizationString, DateTime requestTime);
    }
}