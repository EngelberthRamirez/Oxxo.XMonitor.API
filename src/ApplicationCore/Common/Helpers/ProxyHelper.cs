using System.Net.Http.Headers;
using ApplicationCore.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Common.Helpers
{
    public class ProxyHelper(HttpClient httpClient, ILogger<ProxyHelper> logger)
    {
        public async Task<bool> ExecuteRequestAsync(HttpRequestMessage requestMessage, string endpoint, TimeSpan timeout)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("xmon-auth", Serializer.GetAuthHeaderValue(httpClient.BaseAddress!.AbsoluteUri, endpoint));
                httpClient.DefaultRequestHeaders.Add("xmon-auth2", Serializer.GetAuthHeaderValueV2(httpClient.BaseAddress.AbsoluteUri, endpoint));

                using var cts = new CancellationTokenSource(timeout);
                var httpResponse = await httpClient.SendAsync(requestMessage, cts.Token);
                httpResponse.EnsureSuccessStatusCode();
                var content = await httpResponse.Content.ReadAsStringAsync();

                return !string.IsNullOrEmpty(content) && content != "null";
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, message: $"ExecuteRequest failed for {requestMessage.RequestUri}");
                throw;
            }
            catch (TaskCanceledException ex)
            {
                logger.LogError(ex, "Request timed out");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the request");
                throw;
            }
        }
    }
}