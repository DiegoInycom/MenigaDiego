using Meniga.Core.BusinessModels;
using System.Collections.Generic;

namespace Ibercaja.Aggregation.Security
{
    public interface ISecurityService
    {
        string EncryptValue(string value);
        string DecryptValue(string value);
        IEnumerable<Parameter> EncryptCredentials(IEnumerable<Parameter> parameters);
        Parameter EncryptParameter(Parameter parameter);
    }
}