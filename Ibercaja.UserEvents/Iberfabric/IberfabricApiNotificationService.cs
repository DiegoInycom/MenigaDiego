using log4net;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace Ibercaja.UserEvents.Iberfabric
{
    public class IberfabricApiNotificationService : INotificationService
    {
        private readonly Uri _notificationsUri;
        private readonly string _identityAddress;
        private readonly string _identityClientId;
        private readonly string _identityClientSecret;
        private readonly string _identityClientScopes;

        private TokenResponse _cachedTokenResponse;
        private DateTime _timeToRefreshToken;

        private readonly object _lockObject = new object();

        private static readonly ILog Logger = LogManager.GetLogger(typeof(IberfabricApiNotificationService));

        public IberfabricApiNotificationService(string notificationEndpoint, string identityAddress, string identityClientId, string identityClientSecret, string identityClientScopes)
        {
            _notificationsUri = new Uri(notificationEndpoint);
            _identityAddress = identityAddress;
            _identityClientId = identityClientId;
            _identityClientSecret = identityClientSecret;
            _identityClientScopes = identityClientScopes;
        }

        public async Task<bool> SendNotification(Notification notification)
        {
            using (var response = await Request(HttpMethod.Post, _notificationsUri, notification).ConfigureAwait(false))
            {
                if (response.IsSuccessStatusCode)
                {
                    Logger.Debug($"Notification response OK for notification type {notification.NotificationType} and user {notification.UserNici}");
                    return true;
                }
                else
                {
                    Logger.Error($"Notification error to send Notification with notificationType {notification.NotificationType} for person (Nici) {notification.UserNici} with StatusCode {response.StatusCode.ToString()}");
                    return false;
                }
            }
        }

        private async Task<HttpResponseMessage> Request(HttpMethod method, Uri uri, Notification parameter = null)
        {
            using (var client = new HttpClient())
            {
                Logger.Debug($"Notification request: {method} {uri}");

                client.SetBearerToken(GetAccessToken());

                HttpResponseMessage response;

                try
                {
                    switch (method.Method)
                    {
                        case "POST":
                            response = await client.PostAsJsonAsync(uri, parameter).ConfigureAwait(false);
                            break;
                        default:
                            response = await client.GetAsync(uri).ConfigureAwait(false);
                            break;
                    }
                }
                catch
                {
                    // Raise Exception
                    throw new Exception("Notification API Error");
                }

                Logger.Debug($"Notification response Status Code: {response.StatusCode.ToString()}");

                return response;
            }
        }

        private string GetAccessToken()
        {
            lock (_lockObject)
            {
                if (!HasValidToken())
                {
                    Logger.Debug("Refreshing Iberfabric AccessToken as its not valid.");
                    RefreshToken();
                }

                return _cachedTokenResponse.AccessToken;
            }
        }

        private bool HasValidToken()
        {
            return _cachedTokenResponse != null && DateTime.Now.AddMinutes(-10) < _timeToRefreshToken;
        }

        private void RefreshToken()
        {
            using (var client = new HttpClient())
            {
                var discovery = client.GetDiscoveryDocumentAsync(_identityAddress).GetAwaiter().GetResult();
                if (discovery.IsError)
                {
                    Logger.Error($"Iberfabric IdentityServer error: {discovery.Error}", discovery.Exception);
                    throw discovery.Exception;
                }

                var tokenResponse = client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                {
                    Address = discovery.TokenEndpoint,
                    ClientId = _identityClientId,
                    ClientSecret = _identityClientSecret,
                    Scope = _identityClientScopes
                }).GetAwaiter().GetResult();

                if (tokenResponse.IsError)
                {
                    Logger.Error($"Iberfabric IdentityServer token error: {tokenResponse.Error}", tokenResponse.Exception);
                    throw tokenResponse.Exception;
                }

                _timeToRefreshToken = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn);
                _cachedTokenResponse = tokenResponse;
            }
        }
    }
}