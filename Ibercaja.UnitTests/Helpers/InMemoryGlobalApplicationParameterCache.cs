using Meniga.Runtime.Cache;
using Meniga.Runtime.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ibercaja.UnitTests.Helpers
{
    public class InMemoryGlobalApplicationParameterCache : IGlobalApplicationParameterCache
    {
        private static readonly IList<ApplicationGlobalParameter> _applicationGlobalParameters = new List<ApplicationGlobalParameter>();

        public IList<ApplicationGlobalParameter> GetApplicationGlobalParameters()
        {
            return _applicationGlobalParameters;
        }

        public DateTime? GetDateTimeValue(string name)
        {
            throw new NotImplementedException();
        }

        public decimal? GetDecimalValue(string name)
        {
            throw new NotImplementedException();
        }

        public int? GetIntValue(string name)
        {
            throw new NotImplementedException();
        }

        public long? GetLongValue(string name)
        {
            throw new NotImplementedException();
        }

        public ApplicationGlobalParameter GetParameter(string name)
        {
            return _applicationGlobalParameters.FirstOrDefault(p => p.Name == name);
        }

        public string GetStringValue(string name)
        {
            throw new NotImplementedException();
        }

        public int Save(ApplicationGlobalParameter parameter)
        {
            _applicationGlobalParameters.Add(parameter);
            return 1;
        }
    }
}
