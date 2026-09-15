using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Protocol;

namespace Sonban.Mcp.Tools
{
    internal class BaseTools
    {
        private readonly HttpClient client;
        private readonly JsonSerializerOptions responseSerializationOptions;
        
        protected BaseTools(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            client = httpClientFactory.CreateClient("LibraryApi");
            var authHeader = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authHeader);

            responseSerializationOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };
            responseSerializationOptions.Converters.Add(new JsonStringEnumConverter());
        }

        private CallToolResult GetToolResult<T>(T data)
        {
            if (data is CallToolResult callToolResult)
                return callToolResult;

            string textData;
            if (data == null)
            {
                textData = "No data";
            }
            else
            {
                textData = data as string ?? JsonSerializer.Serialize(data, responseSerializationOptions);
            }
            
            return new CallToolResult {Content = [new TextContentBlock {Text = textData}]};
        }

        protected async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return JsonSerializer.Deserialize<T>(content, responseSerializationOptions);
            throw string.IsNullOrEmpty(content)
                ? new Exception($"Request failed: {(int) response.StatusCode}")
                : new Exception(content);
        }

        protected async Task PostAsync(string url, object data, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var json = JsonSerializer.Serialize(data, responseSerializationOptions);
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            using var response = await client.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw string.IsNullOrEmpty(content)
                    ? new Exception($"Request failed: {(int) response.StatusCode}")
                    : new Exception(content);
        }

        protected async Task<T> PostAsync<T>(string url, object data, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var json = JsonSerializer.Serialize(data, responseSerializationOptions);
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            using var response = await client.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return JsonSerializer.Deserialize<T>(content, responseSerializationOptions);
            throw string.IsNullOrEmpty(content)
                ? new Exception($"Request failed: {(int) response.StatusCode}")
                : new Exception(content);
        }

        protected CallToolResult GetSafeResult(Func<object> action)
        {
            try
            {
                return GetToolResult(action?.Invoke());
            }
            catch (Exception ex)
            {
                return new CallToolResult
                {
                    IsError = true,
                    Content = [new TextContentBlock {Text = ex.Message}],
                };
            }
        }

        protected async Task<CallToolResult> GetSafeResult(Func<Task<object>> action)
        {
            try
            {
                return GetToolResult(await action.Invoke());
            }
            catch (Exception ex)
            {
                return new CallToolResult
                {
                    IsError = true,
                    Content = [new TextContentBlock {Text = ex.Message}],
                };
            }
        }
    }
}
