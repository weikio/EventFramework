using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Api.SDK;
using Weikio.EventFramework.EventSource.GitHub.Events;

namespace Weikio.EventFramework.EventSource.GitHub
{
    public class GitHubApi : IApiEventSource<GitHubConfiguration>
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<GitHubApi> _logger;

        public GitHubConfiguration Configuration { get; set; }

        public GitHubApi(ILogger<GitHubApi> logger, IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
            _logger = logger;
        }

        public async Task<IActionResult> Handle(ICloudEventPublisher cloudEventPublisher)
        {
            var context = _contextAccessor.HttpContext;

            // Verify content type
            if (!VerifyContentType(context, MediaTypeNames.Application.Json))
            {
                _logger.LogError("GitHub event doesn't have correct content type");

                return new ContentResult() { StatusCode = StatusCodes.Status400BadRequest, Content = "GitHub event doesn't have correct content type" };
            }

            // Get body
            var body = await GetBodyAsync(context);

            // Verify signature
            if (!await VerifySignatureAsync(context, Configuration.Secret, body))
            {
                _logger.LogError("GitHub event failed signature validation");

                return new ContentResult() { StatusCode = StatusCodes.Status412PreconditionFailed, Content = "GitHub event failed signature validation" };
            }

            // Process body
            try
            {
                var headers = context.Request.Headers.ToDictionary(kv => kv.Key, kv => kv.Value);
                var message = GitHubEvent.Parse(headers, body);

                var evType = message.Kind.ToString();
                var source = new Uri(message.Body.Repository.Url);
                var ev = CloudEventCreator.Create(message, id: message.Delivery, eventTypeName: evType, source: source);
                
                await cloudEventPublisher.Publish(ev);

                return new OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GitHub event");

                return new ContentResult() { StatusCode = StatusCodes.Status500InternalServerError, Content = "Error processing GitHub event" };
            }
        }

        private static bool VerifyContentType(HttpContext context, string expectedContentType)
        {
            if (context.Request.ContentType is null)
            {
                return false;
            }

            var contentType = new ContentType(context.Request.ContentType);

            if (contentType.MediaType != expectedContentType)
            {
                context.Response.StatusCode = 400;

                return false;
            }

            return true;
        }

        private static async Task<string> GetBodyAsync(HttpContext context)
        {
            string body;

            using (var reader = new StreamReader(context.Request.Body))
                body = await reader.ReadToEndAsync();

            return body;
        }

        private static async Task<bool> VerifySignatureAsync(HttpContext context, string secret, string body)
        {
            context.Request.Headers.TryGetValue("X-Hub-Signature-256", out var signatureSha256);

            var isSigned = signatureSha256.Count > 0;
            var expectedSignature = !string.IsNullOrEmpty(secret);

            if (!isSigned && !expectedSignature)
            {
                // Nothing to do.
                return true;
            }
            else if (!isSigned && expectedSignature)
            {
                context.Response.StatusCode = 400;

                return false;
            }
            else if (isSigned && !expectedSignature)
            {
                context.Response.StatusCode = 400;

                await context.Response.WriteAsync(
                    "Payload includes a secret, so the web hook receiver must configure a secret");

                return false;
            }
            else // if (isSigned && expectedSignature)
            {
                var keyBytes = Encoding.UTF8.GetBytes(secret!);
                var bodyBytes = Encoding.UTF8.GetBytes(body);

                using (var hmac = new HMACSHA256(keyBytes))
                {
                    var hash = hmac.ComputeHash(bodyBytes);
                    var hashHex = BitConverter.ToString(hash).Replace("-", "");
                    var expectedHeader = $"sha256={hashHex.ToLower()}";

                    if (signatureSha256.ToString() != expectedHeader)
                    {
                        context.Response.StatusCode = 400;

                        return false;
                    }
                }

                return true;
            }
        }
    }
}
