using TheSwarm.Common;
using TheSwarm.Utils;
using TheSwarm.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheSwarm.Components.Listener;

/// <summary>
/// One of the main workhorses of the system - it collects data gathered by executors, calculates averages, 
/// prints current values (if specified) and generates output report as either raw JSON or single-page HTML.
/// </summary>
public class ResultsListener {
    private LoggingChannel log {get; init;}
    private SwarmListenerMode mode {get; init;}
    private Dictionary<string, ResultTracker> resultTrackers {get; init;} = new Dictionary<string, ResultTracker>();
    private List<DateTime> timestamps {get; init;} = new List<DateTime>();
    private Thread watcherThread {get; set;}
    private bool threadActive {get; set;}

    private bool printData {get; set;}

    public ResultsListener(SwarmListenerMode mode = SwarmListenerMode.Local, bool printData = true, LoggingLevel logLevel = LoggingLevel.INFO) {
        this.log = Logger.CreateChannel("ResultsListener", logLevel);
        this.mode = mode;
        this.printData = printData;

        watcherThread = new Thread(() => {
            while (threadActive) {
                Thread.Sleep(1000);

                TickResults();
            }
        });
        threadActive = false;
    }

    public void Start() {
        threadActive = true;
        watcherThread.Start();
    }

    public void Stop() {
        threadActive = false;
    }

    public void LogEntry(Response response) {
        string key = $"({response.Method}){response.Name}";
        if (!resultTrackers.ContainsKey(key)) {
            resultTrackers[key] = new ResultTracker(response.Name, response.Method);
        }

        ResultTracker tracker = resultTrackers[key];
        tracker.LogEntry(response.ResponseTimeMs, response.ContentLengthBytes);
        if (response.IsFailed) {
            // Since responses might be a full-on HTML pages or long query responses, we trim it down to a first 100 symbols
            tracker.LogFailure(response.FailureMessage.Substring(0, 100));
        }
    }

    public void GenerateReport() {
        if (mode == SwarmListenerMode.Local || mode == SwarmListenerMode.Hub) {
            // TODO: We'll need to parametrize it during listener initialization in SwarmBuilder. For the time being we hard-code it
            string dirName = $"Results/{DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss")}";
            DirectoryInfo reportDir;
            if (!Directory.Exists(dirName))
                reportDir = Directory.CreateDirectory(dirName);
            else
                reportDir = new DirectoryInfo(dirName);
            
            // Copying static files
            DirectoryInfo reporterFiles = new DirectoryInfo("Resources/Reporter");
            FileUtils.CopyAll(reporterFiles, reportDir);
            
            // We'll do away with anonymous objects here, since these aren't used anywhere else
            var results = new List<object>();
            foreach (KeyValuePair<string, ResultTracker> tracker in resultTrackers)
                results.Add(new {
                    Name = tracker.Key,
                    Data = new {
                        Headers = Results.DataHeaders,
                        Values = tracker.Value.results.ResultsList
                    }
                });

            string serializedData = JsonSerializer.Serialize(
                    new {
                        Results = results,
                        Timestamps = timestamps
                    }, 
                    new JsonSerializerOptions() {
                        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
                        });
            string dataFileContents = $"const results_data = {serializedData}";
            
            File.WriteAllText(
                $"{dirName}/res/data.js",
                dataFileContents);
        }
    }


    // ###########################################################################################
    // Service methods
    // ###########################################################################################
    
    /// <summary>
    /// Method used by watcher thread - it iterates over existing result trackers while generating a result
    /// entry for each of those. These result entries will later be used to generate a report.
    /// </summary>
    /// <param name="resetData">Flag on whether to reset accumulated data or not</param>
    private void TickResults(bool resetData = true) {
        DateTime timestamp = DateTime.Now;
        timestamps.Add(timestamp);

        // TODO: Add logger to the class and replace WriteLine calls with it.
        if (printData)
            log.Info("------------------------------------------------------------------------------------------");
        // Working off the copy of dict keys - in case additional entries are added during execution
        foreach(string key in resultTrackers.Keys.ToArray()) {
            ResultTracker tracker = resultTrackers[key];
            lock (tracker) {
                tracker.GenerateResultEntry(timestamp);
                if (printData)
                    log.Info($"({tracker.method}){tracker.name} - {tracker.reqCount} requests - {tracker.averageResponseTime}ms avg - {tracker.averageRequestsPerSecond} rps avg - {tracker.averageContentLength} bytes avg - {tracker.failRate}% failed");
                if (resetData)
                    tracker.Reset();
            }
        }
        if (printData)
            log.Info("------------------------------------------------------------------------------------------\n\n");
    }
}