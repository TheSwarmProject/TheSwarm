using TheSwarm.Common;
using TheSwarm.Utils;
using TheSwarm.Interfaces;

namespace TheSwarm.Clients;

/// <summary>
/// Main workhorse - HTTP client wrapper with exposed client instance to allow user for more fine-grained control over the context.
/// Default methods work in synchronous mode, but in case we need to chain requests into transaction, it also uses async layer to
/// provide concurrency.
/// </summary>
public class SwarmHttpClient : SwarmClient {
    public string BaseURI {get; set;} = "";
    private HttpClient client {get;}

    public SwarmHttpClient() {
        this.client = new HttpClient();
    }

    public SwarmHttpClient(string baseURI) {
        this.client = new HttpClient();
        this.BaseURI = baseURI;
    }

    /// <summary>
    /// Main caller for GET requests. After executing the request - reports the results to ResultsListener
    /// and returns a Response object.
    /// TODO: It might be worthy to return HttpResponseMessage instead, or at least attach it to Response object.
    /// </summary>
    /// <param name="name">Request name. Will be used in reporter</param>
    /// <param name="uri">URI to open</param>
    /// <returns>Response object</returns>
    public Response Get(string name, string uri) {
        HttpRequestMessage request;
        if (BaseURI != "") {
            request = new HttpRequestMessage(HttpMethod.Get, this.BaseURI + uri);
        } else {
            request = new HttpRequestMessage(HttpMethod.Get, uri);
        }

        DateTime startTime = DateTime.Now;
        WaitForGreenLight();
        HttpResponseMessage httpResponse = client.Send(request);
        TaskExecutor.ReportRequestExecuted();
        DateTime endTime = DateTime.Now;

        string responseContent = new StreamReader(httpResponse.Content.ReadAsStream()).ReadToEnd();

        Response response =  new Response(name, 
                                          Methods.GET, 
                                          (int)(endTime - startTime).TotalMilliseconds,
                                          httpResponse.Content.ReadAsStream().Length,
                                          (int)httpResponse.StatusCode >= 400 ? true : false,
                                          (int)httpResponse.StatusCode >= 400 ? responseContent : "");
        TaskExecutor.LogEntry(response);

        return response;
    }
}
