using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using System.Linq;

namespace Server.Controllers
{
    [Route("ElectroDepot/[controller]")]
    [ApiController]
    public class AIAssistantController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly string[] LocalhostHosts = new[] { "localhost", "127.0.0.1", "::1" };

        public AIAssistantController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("FetchHtml")]
        public async Task<IActionResult> FetchHtml([FromBody] FetchRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Url))
            {
                return BadRequest(new { message = "Url is required." });
            }

            if (!TryValidateRemoteUri(request.Url, out var uri, out var validationError))
            {
                return BadRequest(new { message = validationError });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri);
                httpRequest.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                using var response = await client.SendAsync(httpRequest);
                response.EnsureSuccessStatusCode();

                var html = await response.Content.ReadAsStringAsync();
                return Ok(new FetchHtmlResponse(html));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { message = "Failed to download page content.", detail = ex.Message });
            }
        }

        [HttpPost("FetchImage")]
        public async Task<IActionResult> FetchImage([FromBody] FetchRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Url))
            {
                return BadRequest(new { message = "Url is required." });
            }

            if (!TryValidateRemoteUri(request.Url, out var uri, out var validationError))
            {
                return BadRequest(new { message = validationError });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri);
                httpRequest.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                using var response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var contentLength = response.Content.Headers.ContentLength;
                if (contentLength.HasValue && contentLength.Value > 7_000_000)
                {
                    return BadRequest(new { message = "Image too large to download." });
                }

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                return File(ms.ToArray(), "application/octet-stream");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { message = "Failed to download image content.", detail = ex.Message });
            }
        }

        private static bool TryValidateRemoteUri(string url, out Uri? uri, out string error)
        {
            error = string.Empty;
            uri = null;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var parsedUri) ||
                (parsedUri.Scheme != Uri.UriSchemeHttp && parsedUri.Scheme != Uri.UriSchemeHttps))
            {
                error = "Invalid URL provided.";
                return false;
            }

            if (IsLocalAddress(parsedUri))
            {
                error = "Local addresses are not allowed.";
                return false;
            }

            uri = parsedUri;
            return true;
        }

        private static bool IsLocalAddress(Uri uri)
        {
            if (uri.IsLoopback || LocalhostHosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }

            if (IPAddress.TryParse(uri.DnsSafeHost, out var ipAddress))
            {
                if (IPAddress.IsLoopback(ipAddress))
                {
                    return true;
                }

                var bytes = ipAddress.GetAddressBytes();
                return ipAddress.AddressFamily switch
                {
                    System.Net.Sockets.AddressFamily.InterNetwork => IsPrivateIPv4(bytes),
                    System.Net.Sockets.AddressFamily.InterNetworkV6 => ipAddress.IsIPv6LinkLocal || ipAddress.IsIPv6SiteLocal,
                    _ => false
                };
            }

            return false;
        }

        private static bool IsPrivateIPv4(byte[] bytes)
        {
            return bytes[0] switch
            {
                10 => true,
                127 => true,
                172 when bytes[1] >= 16 && bytes[1] <= 31 => true,
                192 when bytes[1] == 168 => true,
                _ => false
            };
        }
    }

    public record FetchRequest(string Url);
    public record FetchHtmlResponse(string Content);
}
