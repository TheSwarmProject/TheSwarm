using System.Text;

namespace TheSwarmClient.Common; 

/// <summary>
/// Object, used to provide ResultsListener with information on request's execution.
/// We use separate type to encapsulate all required fields into it, instead of providing a bunch of scatterred values.
/// </summary>
public class Response {
    public string Name {get;}
    public string Method {get;}
    public int ResponseTimeMs {get;}
    public byte[] ByteContent {get;}
    public string StringContent {get;} = "";
    public long ContentLengthBytes {get;}
    public bool IsFailed {get;}
    public string FailureMessage {get;}

    public Response(string name, string method, int responseTimeMs, byte[] byteContent, bool isString = true, bool isFailed = false, string failureMessage = "") {
        this.Name               = name;
        this.Method             = method;
        this.ResponseTimeMs     = responseTimeMs;
        this.ByteContent        = byteContent;
        if (isString && ByteContent is not null)
            this.StringContent  = Encoding.UTF8.GetString(this.ByteContent, 0, this.ByteContent.Length);
        if (ByteContent is not null)
            this.ContentLengthBytes = byteContent.Length;
        this.IsFailed           = isFailed;
        this.FailureMessage     = failureMessage;
    }
}