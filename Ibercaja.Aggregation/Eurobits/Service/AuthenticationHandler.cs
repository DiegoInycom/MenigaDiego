using Ibercaja.Aggregation.Eurobits;
using Ibercaja.Aggregation.Eurobits.Service;
using Meniga.Runtime.IOC;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Ibercaja.Aggregation.Eurobits.Service
{
    public class AuthenticationHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = request.Headers.Authorization?.Parameter;
            // If token is no present or expired and we are not requesting a new token
            if (!TokenIsValid(token) && !request.RequestUri.AbsolutePath.Contains("api/login"))
            {
                // Login into service
                var api = IoC.Resolve<IEurobitsApiService>("Default");
                var authResponse = await api.Login().ConfigureAwait(false);
                if (string.IsNullOrEmpty(authResponse.Token))
                {
                    var error = new ErrorResponse
                    {
                        Message = authResponse.Message
                    };

                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Unauthorized,
                        Content = new ObjectContent<ErrorResponse>(error, new JsonMediaTypeFormatter())
                    };
                }
                // Set new token
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        private bool TokenIsValid(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }
            JToken exp;
            if (!JObject.Parse(Jose.JWT.Payload(token)).TryGetValue("exp", out exp))
            {
                return false;
            }

            var utc0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return utc0.AddSeconds(exp.Value<long>()) >= DateTime.UtcNow;
        }
    }
}
