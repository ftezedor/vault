
Azure Functions Core Tools
Core Tools Version:       4.3.0+df07acf9d837d635d1efc2c973225f4f1c8a4333 (64-bit)
Function Runtime Version: 4.1042.100.25374

[2025-10-31T00:36:03.700Z] Found /home/blau/workspace/C-Sharp/BradAzFunc/BradAzFunc.csproj. Using for user secrets file configuration.
[2025-10-31T00:36:06.017Z] Worker process started and initialized.

Functions:

        InvokePipelineMock: [POST] http://localhost:7071/api/InvokePipelineMock

        BlobWatcher: blobTrigger

For detailed output, run func with --verbose flag.
[2025-10-31T00:36:11.002Z] Host lock lease acquired by instance ID '0000000000000000000000002CD5ADB3'.
[2025-10-31T00:36:16.534Z] Executing 'Functions.BlobWatcher' (Reason='New blob detected(LogsAndContainerScan): samples-workitems/test.txt', Id=40c2f9b9-b1d6-41b2-b1ba-e1745914e9a1)
[2025-10-31T00:36:16.535Z] Trigger Details: MessageId: b93dfa1b-373b-4b43-865d-08f3c5cda6a4, DequeueCount: 1, InsertedOn: 2025-10-31T00:36:16.000+00:00, BlobCreated: 2025-10-31T00:36:14.000+00:00, BlobLastModified: 2025-10-31T00:36:14.000+00:00
[2025-10-31T00:36:16.827Z] Calling downstream at http://localhost:7071/api/InvokePipelineMock
[2025-10-31T00:36:16.829Z] Start processing HTTP request POST http://localhost:7071/api/InvokePipelineMock
[2025-10-31T00:36:16.829Z] Sending HTTP request POST http://localhost:7071/api/InvokePipelineMock
[2025-10-31T00:36:16.982Z] Executing 'Functions.InvokePipelineMock' (Reason='This function was programmatically called via the host APIs.', Id=c1fe2644-e6f0-47b5-81d3-7ea5950fd033)
[2025-10-31T00:36:17.020Z] Mock received body: {"Container":"samples-workitems","Name":"test.txt","Size":12266}
[2025-10-31T00:36:17.025Z] MOCK: would start ADF for samples-workitems/test.txt (size 12266 bytes).
[2025-10-31T00:36:17.071Z] Executed 'Functions.InvokePipelineMock' (Succeeded, Id=c1fe2644-e6f0-47b5-81d3-7ea5950fd033, Duration=96ms)
[2025-10-31T00:36:17.102Z] Received HTTP response headers after 263.2055ms - 202
[2025-10-31T00:36:17.103Z] End processing HTTP request after 280.9688ms - 202
[2025-10-31T00:36:17.106Z] Downstream responded Accepted with body: Accepted (mock). ADF call would happen here.
[2025-10-31T00:36:17.110Z] Executed 'Functions.BlobWatcher' (Succeeded, Id=40c2f9b9-b1d6-41b2-b1ba-e1745914e9a1, Duration=627ms)
[2025-10-31T00:39:54.986Z] Executing 'Functions.BlobWatcher' (Reason='New blob detected(LogsAndContainerScan): samples-workitems/ofertas.pdf', Id=9ebc7c63-6f14-46d7-86df-d3f062cfbf5c)
[2025-10-31T00:39:54.986Z] Trigger Details: MessageId: 339ddf79-860e-4e36-a373-1976f7eefe07, DequeueCount: 1, InsertedOn: 2025-10-31T00:39:54.000+00:00, BlobCreated: 2025-10-31T00:39:54.000+00:00, BlobLastModified: 2025-10-31T00:39:54.000+00:00
[2025-10-31T00:39:55.003Z] Executing 'Functions.InvokePipelineMock' (Reason='This function was programmatically called via the host APIs.', Id=d641580d-9782-43cd-b22d-349bf873c68b)
[2025-10-31T00:39:55.005Z] Calling downstream at http://localhost:7071/api/InvokePipelineMock
[2025-10-31T00:39:55.005Z] Start processing HTTP request POST http://localhost:7071/api/InvokePipelineMock
[2025-10-31T00:39:55.005Z] Sending HTTP request POST http://localhost:7071/api/InvokePipelineMock
[2025-10-31T00:39:55.010Z] Mock received body: {"Container":"samples-workitems","Name":"ofertas.pdf","Size":2832844}
[2025-10-31T00:39:55.014Z] MOCK: would start ADF for samples-workitems/ofertas.pdf (size 2832844 bytes).
[2025-10-31T00:39:55.017Z] Executed 'Functions.InvokePipelineMock' (Succeeded, Id=d641580d-9782-43cd-b22d-349bf873c68b, Duration=14ms)
[2025-10-31T00:39:55.022Z] Received HTTP response headers after 24.299ms - 202
[2025-10-31T00:39:55.023Z] End processing HTTP request after 24.5256ms - 202
[2025-10-31T00:39:55.023Z] Downstream responded Accepted with body: Accepted (mock). ADF call would happen here.
[2025-10-31T00:39:55.023Z] Executed 'Functions.BlobWatcher' (Succeeded, Id=9ebc7c63-6f14-46d7-86df-d3f062cfbf5c, Duration=39ms)




1) Make the trigger use its own connection name (don’t reuse AzureWebJobsStorage)

In production it’s common to keep the runtime storage (AzureWebJobsStorage) separate from the blob account you’re watching. Change your trigger attribute to use a new setting name—e.g. InputStorage—and put the real container path you want in Azure:

// BlobWatcher.cs
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class BlobWatcher
{
    private readonly ILogger<BlobWatcher> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _downstreamUrl;
    private readonly string? _downstreamKey; // optional: function key for auth

    public BlobWatcher(
        ILogger<BlobWatcher> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration config)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _downstreamUrl = config["DOWNSTREAM_FUNCTION_URL"]
            ?? throw new System.InvalidOperationException("DOWNSTREAM_FUNCTION_URL is missing.");
        _downstreamKey = config["DOWNSTREAM_FUNCTION_KEY"]; // optional
    }

    // Use your real container here instead of samples-workitems
    [Function("BlobWatcher")]
    public async Task Run(
        [BlobTrigger("my-container/{name}", Connection = "InputStorage")] Stream blob,
        string name)
    {
        var size = blob.Length;

        _logger.LogInformation("Blob created: {Name}, size {Size} bytes.", name, size);

        var payload = new
        {
            Container = "my-container",
            Name = name,
            Size = size
        };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var client = _httpClientFactory.CreateClient();

        // If calling an Azure Function secured with a function key:
        if (!string.IsNullOrWhiteSpace(_downstreamKey))
        {
            // Either send as header:
            client.DefaultRequestHeaders.Remove("x-functions-key");
            client.DefaultRequestHeaders.Add("x-functions-key", _downstreamKey);

            // …or append as a query string: ?code=... (pick one approach)
            // _downstreamUrl = $"{_downstreamUrl}{(_downstreamUrl.Contains("?") ? "&" : "?")}code={_downstreamKey}";
        }

        _logger.LogInformation("Calling downstream at {Url}", _downstreamUrl);
        var resp = await client.PostAsync(_downstreamUrl, content);

        var body = await resp.Content.ReadAsStringAsync();
        _logger.LogInformation("Downstream responded {Status} with body: {Body}", resp.StatusCode, body);

        // If you want the function to fail the invocation on non-2xx:
        // resp.EnsureSuccessStatusCode();
    }
}

What changed from your local version

Connection = "InputStorage" (new app setting for the watched account).

Hardcoded path updated from samples-workitems to your real container (my-container here—change it).

Optional support for a function key if the downstream HTTP function requires it.

2) Settings: local vs Azure
local.settings.json (dev)

Keep Azurite for local runs, but add an entry for InputStorage so you can test with a real or a second Azurite container if you want:

{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",

    // watched storage (local dev): can be Azurite or a real connection string
    "InputStorage": "UseDevelopmentStorage=true",

    // the function you need to call locally (your mock or local endpoint)
    "DOWNSTREAM_FUNCTION_URL": "http://localhost:7071/api/InvokePipelineMock"

    // If you want to test a function key locally:
    // "DOWNSTREAM_FUNCTION_KEY": "yourlocaltestkey"
  }
}

local.settings.json is not deployed. In Azure you set these as Application settings.

Azure App Settings (in the Function App)

Set these in Configuration:

AzureWebJobsStorage → the Function App’s runtime storage account (required).

InputStorage → connection string (or identity-based config) to the storage account that contains the container you’re watching.

DOWNSTREAM_FUNCTION_URL → the real URL of the already-deployed function you must call (HTTP trigger).
Example: https://my-downstream-func.azurewebsites.net/api/InvokePipeline

DOWNSTREAM_FUNCTION_KEY (optional) → the function key if it’s protected by function/admin authlevel.

If both the Function App and the watched Storage account are in the same tenant/subscription, consider managed identity instead of a connection string. For identity:

Assign the Function App a system-assigned identity.

Grant it “Storage Blob Data Reader” on the watched container/account.

Replace InputStorage with:

InputStorage__accountName = <yourstorageaccountname>

(This uses MSI with the v6+ Storage extension you’re already on.)

3) Program.cs is fine

You’re already on .NET 8 isolated and registering HttpClientFactory. Nothing to change there.

4) (Optional) Add a minimal host.json

Not required, but good to have:

{
  "version": "2.0",
  "logging": {
    "applicationInsights": {
      "samplingSettings": { "isEnabled": true }
    }
  },
  "extensions": {
    "blobs": {
      "maxDegreeOfParallelism": 4
    }
  }
}

Place it at the project root next to the .csproj.

5) Publish

Pick your favorite path:

Azure Functions Core Tools

func azure functionapp publish <your-func-app-name> --dotnet-cli-params "--configuration Release"

Azure CLI / GitHub Actions: just make sure the app settings above are in place before starting the app.



Quick checklist

    Update [BlobTrigger("my-container/{name}", Connection="InputStorage")].

    Set DOWNSTREAM_FUNCTION_URL (and DOWNSTREAM_FUNCTION_KEY if needed) in Azure.

    Provide InputStorage via connection string or MSI (InputStorage__accountName + RBAC).

    Ensure the downstream function accepts your JSON payload { Container, Name, Size } (or adjust).

    Deploy.


