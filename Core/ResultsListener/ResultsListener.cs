using TheSwarm.Common;
using TheSwarm.Utils;

namespace TheSwarm.Components.Listener;

/// <summary>
/// One of the main workhorses of the system - it collects data gathered by executors, calculates averages, 
/// prints current values (if specified) and generates output report as either raw JSON or single-page HTML.
/// </summary>
public class ResultsListener {
    private Dictionary<string, ResultTracker> resultTrackers {get; set;} = new Dictionary<string, ResultTracker>();
    private Thread watcherThread {get; set;}
    private bool threadActive {get; set;}

    private bool printData {get; set;}

    public ResultsListener(bool printData = true) {
        this.printData = printData;

        watcherThread = new Thread(() => {
            while (threadActive) {
                Thread.Sleep(1000);

                TickResults();
                if (this.printData) {
                    PrintStats();
                }
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
            tracker.LogFailure(response.FailureMessage);
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
        DateTime timestamp = DateTime.Now;;

        foreach(string key in resultTrackers.Keys) {
            ResultTracker tracker = resultTrackers[key];
            lock (tracker) {
                tracker.GenerateResultEntry(timestamp, resetData);
            }
        }
    }

    private void PrintStats() {
        // TODO: Add logger to the class and replace WriteLine calls with it.
        Console.WriteLine("------------------------------------------------------------------------------------------");
        String timestamp = DateTime.Now.ToString(Constants.GRAPH_DATETIME_FORMAT);
        foreach (string request in resultTrackers.Keys) {
            ResultTracker tracker = resultTrackers[request];

            Console.WriteLine($"({tracker.method}){tracker.name} - {tracker.reqCount} requests - {tracker.averageResponseTime}ms avg - {tracker.averageRequestsPerSecond} rps avg - {tracker.averageContentLength} bytes avg - {tracker.failRate}% failed");
        }
        Console.WriteLine("------------------------------------------------------------------------------------------\n\n");
    }
}