using TheSwarmClient.Common;
using TheSwarmClient.Utils;
using TheSwarmClient.Extendables;

namespace TheSwarmClient.Clients;

/// <summary>
/// Main workhorse - HTTP client wrapper with exposed client instance to allow user for more fine-grained control over the context.
/// Default methods work in synchronous mode, but in case we need to chain requests into transaction, it also uses async layer to
/// provide concurrency.
/// </summary>
public class SwarmHttpClient : SwarmClient
{
    public string BaseURL { get; set; } = "";
    private HttpClient client { get; }

    public SwarmHttpClient()
    {
        this.client = new HttpClient();
    }

    public SwarmHttpClient(string baseURL)
    {
        this.client = new HttpClient();
        this.BaseURL = baseURL;
    }

    /// <summary>
    /// Main caller for GET requests. After executing the request - reports the results to ResultsListener and returns a Response object.
    /// </summary>
    /// <param name="name">Request name. Will be used in reporter</param>
    /// <param name="url">URL to open</param>
    /// <returns>Response object</returns>
    public Response Get(string name, string url)
    {
        HttpRequestMessage request;
        if (BaseURL != "")
            request = new HttpRequestMessage(HttpMethod.Get, this.BaseURL + url);
        else
            request = new HttpRequestMessage(HttpMethod.Get, url);

        return SendHttpRequest(name, request);
    }

    /// <summary>
    /// Main caller for POST requests. After executing the request - reports the results to ResultsListener and returns a Response object.
    /// </summary>
    /// <param name="name">Request name. Will be used in reporter</param>
    /// <param name="url">URL to open</param>
    /// <param name="requestBody">Request body</param>
    /// <param name="contentType">Content-Type header value</param>
    /// <returns></returns>
    public Response Post(string name, string url, string requestBody, string contentType = "text/plain")
    {
        HttpRequestMessage request;
        if (BaseURL != "")
            request = new HttpRequestMessage(HttpMethod.Post, this.BaseURL + url);
        else
            request = new HttpRequestMessage(HttpMethod.Post, url);

        request.Content = new StringContent(requestBody);
        request.Headers.Add("Content-Type", contentType);

        return SendHttpRequest(name, request);
    }

    /// <summary>
    /// Main caller for PUT requests. After executing the request - reports the results to ResultsListener and returns a Response object.
    /// </summary>
    /// <param name="name">Request name. Will be used in reporter</param>
    /// <param name="url">URL to open</param>
    /// <param name="requestBody">Request body</param>
    /// <param name="contentType">Content-Type header value</param>
    /// <returns></returns>
    public Response Put(string name, string url, string requestBody, string contentType = "text/plain")
    {
        HttpRequestMessage request;
        if (BaseURL != "")
            request = new HttpRequestMessage(HttpMethod.Put, this.BaseURL + url);
        else
            request = new HttpRequestMessage(HttpMethod.Put, url);

        request.Content = new StringContent(requestBody);
        request.Headers.Add("Content-Type", contentType);

        return SendHttpRequest(name, request);
    }

    /// <summary>
    /// Main caller for PATCH requests. After executing the request - reports the results to ResultsListener and returns a Response object.
    /// </summary>
    /// <param name="name">Request name. Will be used in reporter</param>
    /// <param name="url">URL to open</param>
    /// <param name="requestBody">Request body</param>
    /// <param name="contentType">Content-Type header value</param>
    /// <returns></returns>
    public Response Patch(string name, string url, string requestBody, string contentType = "text/plain")
    {
        HttpRequestMessage request;
        if (BaseURL != "")
            request = new HttpRequestMessage(HttpMethod.Patch, this.BaseURL + url);
        else
            request = new HttpRequestMessage(HttpMethod.Patch, url);

        request.Content = new StringContent(requestBody);
        request.Headers.Add("Content-Type", contentType);

        return SendHttpRequest(name, request);
    }

    /// <summary>
    /// Main caller for DELETE requests. After executing the request - reports the results to ResultsListener and returns a Response object.
    /// </summary>
    /// <param name="name">Request name. Will be used in reporter</param>
    /// <param name="url">URL to open</param>
    /// <param name="requestBody">Request body</param>
    /// <param name="contentType">Content-Type header value</param>
    /// <returns></returns>
    public Response Delete(string name, string url, string requestBody, string contentType = "text/plain")
    {
        HttpRequestMessage request;
        if (BaseURL != "")
            request = new HttpRequestMessage(HttpMethod.Delete, this.BaseURL + url);
        else
            request = new HttpRequestMessage(HttpMethod.Delete, url);

        request.Content = new StringContent(requestBody);
        request.Headers.Add("Content-Type", contentType);

        return SendHttpRequest(name, request);
    }

    /// <summary>
    /// Main workhorse - sends given request, generates Response object and logs the request to results listener.
    /// In case user would need to have a more fine-grained control over the request - this method is left public.
    /// </summary>
    /// <param name="httpRequest">HttpRequestMessage instance</param>
    /// <param name="name">Request name. Will be used in reporter</param>
    /// <param name="method">Request method. Will be used in reporter</param>
    /// <returns></returns>
    public Response SendHttpRequest(string name, HttpRequestMessage httpRequest)
    {
        WaitForGreenLight();
        HttpResponseMessage httpResponse = null;
        int executionTime = MeasureExecutionTime(() => httpResponse = client.Send(httpRequest));
        TaskExecutor.ReportRequestExecuted();

        string responseContent = new StreamReader(httpResponse.Content.ReadAsStream()).ReadToEnd();

        Response response = new Response(name,
                                         httpRequest.Method.ToString(),
                                         executionTime,
                                         httpResponse,
                                         httpResponse.Content.ReadAsByteArrayAsync().Result,
                                         true,
                                         (int)httpResponse.StatusCode >= 400 ? true : false,
                                         (int)httpResponse.StatusCode >= 400 ? responseContent : "");
        TaskExecutor.LogEntry(response);

        return response;
    }
}
