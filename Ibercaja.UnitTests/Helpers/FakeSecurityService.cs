using Meniga.Core.BusinessModels;
using System.Collections.Generic;
using Ibercaja.Aggregation.Security;

namespace Ibercaja.UnitTests.Helpers
{
    public class NoEncryptionSecurityService : ISecurityService
    {
        public string DecryptValue(string value)
        {
            return value;
        }

        public IEnumerable<Parameter> EncryptCredentials(IEnumerable<Parameter> parameters)
        {
            return parameters;
        }

        public string EncryptValue(string value)
        {
            return value;
        }

        public Parameter EncryptParameter(Parameter parameter)
        {
            return parameter;
        }
    }
}
