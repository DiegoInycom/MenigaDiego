using log4net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ibercaja.Aggregation.Eurobits
{
    public class LoggingHandler : DelegatingHandler
    {
        private static readonly ILog RequestResponseLogger = LogManager.GetLogger("Ibercaja.Aggregation.Eurobits.Service");
        private readonly StringBuilder _sb = new StringBuilder();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _sb.Clear();

            RequestResponseLogger.Info($"Request: {request}");
            _sb.AppendLine($"Request: {request}");
            _sb.AppendLine(request.Content != null
                           ? $"Request body: {await request.Content.ReadAsStringAsync()}"
                           : "Request body: empty");

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            RequestResponseLogger.Info($"Response: {response}");
            _sb.AppendLine($"Response: {response}");
            _sb.AppendLine(response.Content != null
                            ? $"Response body: {await response.Content.ReadAsStringAsync()}"
                            : "Response body: empty");
            RequestResponseLogger.Debug(_sb);

            return response;
        }
    }
}
