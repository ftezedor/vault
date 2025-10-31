using System.IO;
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace BradAzFunc
{
    public class InvokePipelineMock
    {
        private readonly ILogger<InvokePipelineMock> _logger;

        public InvokePipelineMock(ILogger<InvokePipelineMock> logger)
        {
            _logger = logger;
        }

        public record PipelineRequest(string Container, string Name, long Size);

        [Function("InvokePipelineMock")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            // Read the raw request body
            using var reader = new StreamReader(req.Body);
            var raw = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(raw))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Body missing.");
                return bad;
            }

            _logger.LogInformation("Mock received body: {Raw}", raw);

            PipelineRequest? request;
            try
            {
                request = JsonSerializer.Deserialize<PipelineRequest>(
                    raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync($"Invalid JSON: {ex.Message}");
                return bad;
            }

            _logger.LogInformation(
                "MOCK: would start ADF for {Container}/{Name} (size {Size} bytes).",
                request!.Container, request.Name, request.Size);

            var res = req.CreateResponse(HttpStatusCode.Accepted);
            await res.WriteStringAsync("Accepted (mock). ADF call would happen here.");
            return res;
        }
    }
}
