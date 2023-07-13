namespace TheSwarm.Common; 

/// <summary>
/// Object, used to provide ResultsListener with information on request's execution.
/// We use separate type to encapsulate all required fields into it, instead of providing a bunch of scatterred values.
/// </summary>
public class Response {
    public string name {get;}
    public string method {get;}
    public int responseTimeMs {get;}
    public long contentLengthBytes {get;}
    public bool isFailed {get;}
    public string failureMessage {get;}

    public Response(string name, string method, int responseTimeMs, long contentLengthBytes, bool isFailed = false, string failureMessage = "") {
        this.name               = name;
        this.method             = method;
        this.responseTimeMs     = responseTimeMs;
        this.contentLengthBytes = contentLengthBytes;
        this.isFailed           = isFailed;
        this.failureMessage     = failureMessage;
    }
}