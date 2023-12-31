/*
############################## Total flow charts ##############################
*/

// We store the datasets for use in alasysis sections
var averages_datasets = [];
var rps_datasets = [];

var total_averages_chart = null;
var total_rps_chart = null;
var total_failures_chart = null;

function draw_all_average_response_times() {
    var ctx = document.getElementById("all_average_response_times_chart");

    results_data.Results.forEach(entry => {
        let resultSet = new SwarmResultSet(entry.Data)
        let dataset = {
            label: entry.Name,
            data: []
        }

        for (let index = 0; index < resultSet.Size; index++) {
            dataset.data.push({
                x: resultSet.GetItem("timestamp", index),
                y: resultSet.GetItem("averageResponseTime", index),
            })
        }

        averages_datasets.push(dataset)
    })

    total_averages_chart = draw_line_chart(ctx, averages_datasets, "Response time, ms")
}

function draw_all_rps() {
    var ctx = document.getElementById("all_requests_per_second_chart");

    results_data.Results.forEach(entry => {
        let resultSet = new SwarmResultSet(entry.Data)
        let dataset = {
            label: entry.Name,
            data: []
        }

        for (let index = 0; index < resultSet.Size; index++) {
            dataset.data.push({
                x: resultSet.GetItem("timestamp", index),
                y: resultSet.GetItem("averageRequestsPerSecond", index),
            })
        }

        rps_datasets.push(dataset)
    })

    total_rps_chart = draw_line_chart(ctx, rps_datasets, "Requests per second")
}

function draw_all_failures() {
    var ctx = document.getElementById("all_failures_chart");

    var failure_datasets = [];
    results_data.Results.forEach(entry => {
        let resultSet = new SwarmResultSet(entry.Data)
        let dataset = {
            label: entry.Name,
            data: []
        }

        for (let index = 0; index < resultSet.Size; index++) {
            dataset.data.push({
                x: resultSet.GetItem("timestamp", index),
                y: resultSet.GetItem("failuresCount", index),
            })
        }

        failure_datasets.push(dataset)
    })

    total_failures_chart = draw_line_chart(ctx, failure_datasets, "Failures")
}

function draw_line_chart(canvas, datasets, valueName, annotations = {}) {
    return new Chart(canvas, {
        type: 'line',
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: {
                    type: "time",
                    display: true,
                    title: {
                        display: true,
                        text: "Timeline"
                    },
                    ticks: {
                        maxTicksLimit: 20
                    }
                },
                y: {
                    title: {
                        display: true,
                        text: valueName
                    }
                },
                xAxis: {
                    type: 'time',
                    ticks: {
                        source: "labels"
                    },
                    time: {
                        tooltipFormat: "HH:mm:ss",
                    }
                }
            },
            plugins: {
                zoom: {
                    pan: {
                        enabled: true,
                        mode: 'xy',
                    },
                    zoom: {
                        wheel: {
                            enabled: false
                        },
                        drag: {
                            enabled: true,
                            modifierKey: "alt",
                            drawTime: "afterDraw",
                            backgroundColor: "rgba(0,0,0,0.3)"
                        },

                        mode: 'xy',
                    }
                },
                annotation: annotations
            }
        },
        data: {
            datasets: datasets
        }
    });
}

/*
############################## Response times analysis ##############################
*/

var fast_requests_threshold = 2000;
var normal_requests_threshold = 5000;
var slow_requests_threshold = 15000;

var response_times_list = [];

var responses_analysis_chart = null;
var response_analysis_doughnut_chart = null;

function display_thresholds() {
    document.getElementById("analysis_fast_threshold").value = fast_requests_threshold;
    document.getElementById("analysis_normal_threshold").value = normal_requests_threshold;
    document.getElementById("analysis_slow_threshold").value = slow_requests_threshold;
}

/**
 * Applies the thresholds from the text inputs
 */
function apply_new_thresholds() {
    let new_fast_threshold = parseInt(document.getElementById("analysis_fast_threshold").value),
        new_normal_threshold = parseInt(document.getElementById("analysis_normal_threshold").value),
        new_slow_threshold = parseInt(document.getElementById("analysis_slow_threshold").value);

    if (new_slow_threshold > new_normal_threshold && new_slow_threshold > new_fast_threshold) {
        slow_requests_threshold = new_slow_threshold;
    } else {
        alert("Slow threshold can't be less than normal or fast thresholds");
    }

    if (new_normal_threshold > new_fast_threshold) {
        normal_requests_threshold = new_normal_threshold;
    } else {
        alert("Normal threshold can't be less than fast threshold");
    }

    fast_requests_threshold = new_fast_threshold;

    create_response_analysis_buttons();
    recreate_response_distribution_table();
    populate_response_distribution_table();
    populate_responses_summary_table();
}

function create_response_analysis_buttons() {
    let list = document.getElementById("analysis_calls_list")

    // First we clear the list by removing existing buttons - we need it in case we changed thresholds
    while (list.firstChild) {
        list.removeChild(list.lastChild);
    }

    response_times_list = [];
    // Then we proceed to analyze the amount of requests that fall into pre-specified categories
    averages_datasets.forEach(entry => {
        let call_data = {
            Name: entry.label,
            ResponseTimes: [],
            AverageResponseTime: 0,
            FastResponses: 0,
            NormalResponses: 0,
            SlowResponses: 0,
            UnacceptableResponses: 0
        }

        entry.data.forEach(dataset => {
            call_data.ResponseTimes.push(dataset.y);

            if (dataset.y >= slow_requests_threshold) {
                call_data.UnacceptableResponses += 1;
            } else if (dataset.y >= normal_requests_threshold && dataset.y < slow_requests_threshold) {
                call_data.SlowResponses += 1;
            } else if (dataset.y >= fast_requests_threshold && dataset.y < normal_requests_threshold) {
                call_data.NormalResponses += 1;
            } else {
                call_data.FastResponses += 1;
            }
        });

        // Since there might be zeroes - we eleminate them from calculation by filtering them out
        let real_values = call_data.ResponseTimes.filter(Number);
        call_data.AverageResponseTime = ((real_values.reduce((a, b) => a + b, 0)) / real_values.length).toFixed(2);

        response_times_list.push(call_data);

        var list_container = document.createElement("li");
        var button = document.createElement("button");
        if (call_data.AverageResponseTime > slow_requests_threshold) {
            button.setAttribute("class", "unacceptable_call_group");
        } else if (call_data.AverageResponseTime >= normal_requests_threshold && call_data.AverageResponseTime < slow_requests_threshold) {
            button.setAttribute("class", "slow_call_group");
        } else if (call_data.AverageResponseTime >= fast_requests_threshold && call_data.AverageResponseTime < normal_requests_threshold) {
            button.setAttribute("class", "normal_call_group");
        } else {
            button.setAttribute("class", "fast_call_group");
        }
        button.onclick = function () {
            draw_response_analysis_chart(call_data.Name);
            draw_response_analysis_doughnut(call_data.Name)
            updateAverageResponseTime(call_data.Name)
        };
        button.innerHTML = call_data.Name;
        list_container.appendChild(button);
        list.appendChild(list_container);
    });

    // We auto-initialize all the sections, using first item from the list
    list.firstChild.firstChild.click();
}

function draw_response_analysis_chart(request_name) {
    if (responses_analysis_chart) {
        responses_analysis_chart.destroy();
    }

    let annotations = {
        annotations: {
            line1: {
                type: 'line',
                label: {
                    display: true,
                    content: "Fast",
                    position: "start"
                },
                position: "start",
                yMin: fast_requests_threshold,
                yMax: fast_requests_threshold,
                borderColor: 'rgb(0, 153, 0)',
                borderWidth: 2,
            },
            line2: {
                type: 'line',
                label: {
                    display: true,
                    content: "Normal",
                    position: "start"
                },
                position: "start",
                yMin: normal_requests_threshold,
                yMax: normal_requests_threshold,
                borderColor: 'rgb(255, 255, 0)',
                borderWidth: 2,
            },
            line3: {
                type: 'line',
                label: {
                    display: true,
                    content: "Slow",
                    position: "start"
                },
                yMin: slow_requests_threshold,
                yMax: slow_requests_threshold,
                borderColor: 'rgb(255, 153, 51)',
                borderWidth: 2,
            }
        }
    }

    responses_analysis_chart = draw_line_chart(
        document.getElementById("responses_history_analysis"),
        [averages_datasets.find(element => element.label == request_name)],
        "Response time, ms",
        annotations);
}

function draw_response_analysis_doughnut(request_name) {
    let entry = response_times_list.find(val => val.Name == request_name);
    let data = {
        labels: [
            'Fast response (0-{0} ms)'.f(fast_requests_threshold),
            'Normal response ({0}-{1} ms)'.f(fast_requests_threshold, normal_requests_threshold),
            'Slow response ({0}-{1} ms)'.f(normal_requests_threshold, slow_requests_threshold),
            'Unacceptable response ({0}+ ms)'.f(slow_requests_threshold)
        ],
        datasets: [
            {
                data: [
                    entry.FastResponses,
                    entry.NormalResponses,
                    entry.SlowResponses,
                    entry.UnacceptableResponses
                ],
                backgroundColor: [
                    'rgb(0, 153, 0)',
                    'rgb(255, 255, 0)',
                    'rgb(255, 153, 51)',
                    'rgb(255, 51, 51)'
                ]
            }
        ]
    }

    if (response_analysis_doughnut_chart) {
        response_analysis_doughnut_chart.destroy();
    }
    response_analysis_doughnut_chart = draw_doughnut_chart(document.getElementById("responses_analysis_doughnut"), data, 'Avg: {0} ms'.f(entry.AverageResponseTime));
}

function draw_doughnut_chart(canvas, data) {
    return new Chart(canvas, {
        plugins: [ChartDataLabels],
        type: 'doughnut',
        data: data,
        options: {
            layout: {
                padding: {
                    bottom: 25
                }
            },
            plugins: {
                legend: {
                    position: 'top',
                    display: false,
                },
                tooltip: {
                    enabled: true,
                    callbacks: {
                        footer: (ttItem) => {
                            let sum = 0;
                            let dataArr = ttItem[0].dataset.data;
                            dataArr.map(data => {
                                sum += Number(data);
                            });

                            let percentage = (ttItem[0].parsed * 100 / sum).toFixed(2) + '%';
                            return `${percentage}`;
                        }
                    }
                },
                datalabels: {
                    formatter: (value, ctx) => {
                        let sum = 0;
                        let dataArr = ctx.chart.data.datasets[0].data;
                        dataArr.map(data => {
                            sum += data;
                        });
                        let percentage = (value * 100 / sum).toFixed(2) + "%";
                        return percentage;
                    },
                    color: '#fff',
                }
            }
        },
    });
}

function updateAverageResponseTime(request_name) {
    document.getElementById("average_response_timve_val").innerHTML = 'Avg. {0} ms'.f(response_times_list.find(val => val.Name == request_name).AverageResponseTime);
}

/**
 * We re-create sortables due to funky behavior when it gets initialized twice (reverse sorting stops working)
 */
function recreate_response_distribution_table() {
    document.querySelector("#responses_table").remove();

    let headers = [
        { Text: "Call" },
        {
            Text: "Fast response entries",
            Class: "sorttable_numeric"
        },
        {
            Text: "Normal response entries",
            Class: "sorttable_numeric"
        },
        {
            Text: "Slow response entries",
            Class: "sorttable_numeric"
        },
        {
            Text: "Unacceptable response entries",
            Class: "sorttable_numeric"
        },
    ];

    createTable("responses_table", document.getElementById("responses_table_container"), headers);
}

function populate_response_distribution_table() {
    let table_body = document.querySelector("#responses_table tbody");

    response_times_list.forEach(entry => {
        let row = document.createElement("tr");
        let name = document.createElement("td");
        let fast_response = document.createElement("td");
        let normal_response = document.createElement("td");
        let slow_response = document.createElement("td");
        let unacceptable_response = document.createElement("td");

        name.innerHTML = entry.Name;
        fast_response.innerHTML = entry.FastResponses;
        normal_response.innerHTML = entry.NormalResponses;
        slow_response.innerHTML = entry.SlowResponses;
        unacceptable_response.innerHTML = entry.UnacceptableResponses;

        row.appendChild(name);
        row.appendChild(fast_response);
        row.appendChild(normal_response);
        row.appendChild(slow_response);
        row.appendChild(unacceptable_response);
        table_body.appendChild(row);
    })

    sorttable.makeSortable(document.querySelector("#responses_table"));
}

function populate_responses_summary_table() {
    // First, we define the container variables
    var unacceptable_responses = [];
    var slow_responses = [];
    var fastest_response = null;
    var slowest_response = null;
    var fastest_response_group = null;
    var slowest_response_group = null;

    // Then we populate the data
    response_times_list.forEach(call => {
        if (call.UnacceptableResponses > 0) {
            unacceptable_responses.push(call);
        }
        if (call.SlowResponses > 0) {
            slow_responses.push(call);
        }
        if (fastest_response == null) {
            fastest_response = call;
        } else {
            if (call.ResponseTimes.min() < fastest_response.ResponseTimes.min()) {
                fastest_response = call;
            }
        }
        if (slowest_response == null) {
            slowest_response = call;
        } else {
            if (call.ResponseTimes.max() > slowest_response.ResponseTimes.max()) {
                slowest_response = call;
            }
        }
        if (fastest_response_group == null) {
            fastest_response_group = call;
        } else {
            if (call.AverageResponseTime < fastest_response_group.AverageResponseTime) {
                fastest_response_group = call;
            }
        }
        if (slowest_response_group == null) {
            slowest_response_group = call;
        } else {
            if (call.AverageResponseTime > slowest_response_group.AverageResponseTime) {
                slowest_response_group = call;
            }
        }
    })

    // Finally, we extract and set the values
    // Immediate optimization
    var unacceptable_response_values = "";
    if (unacceptable_responses.length > 0) {
        for (var index = 0; index < unacceptable_responses.length; index++) {
            var call_data = unacceptable_responses[index];
            unacceptable_response_values += "{0} - <b>{1} unacceptable entries out of {2}</b><br>".
                format(call_data.Name, call_data.UnacceptableResponses,
                    call_data.ResponseTimes.length)
        }
    } else {
        unacceptable_response_values = "None";
    }
    document.getElementById("immediate_response_optimization_value").innerHTML = unacceptable_response_values;

    // Optimization
    var slow_response_values = "";
    if (slow_responses.length > 0) {
        for (var index = 0; index < slow_responses.length; index++) {
            var call_data = slow_responses[index];
            slow_response_values += "{0} - <b>{1} slow entries out of {2}</b><br>".
                format(call_data.Name, call_data.SlowResponses, call_data.ResponseTimes.length)
        }
    } else {
        slow_response_values = "None";
    }
    document.getElementById("response_optimization_value").innerHTML = slow_response_values;

    // Fastest call
    document.getElementById("fastest_call_value").innerHTML = "{0} - <b>{1} ms</b>\n".format(fastest_response.Name,
        fastest_response.ResponseTimes.minNotZero());

    // Slowest call
    document.getElementById("slowest_call_value").innerHTML = "{0} - <b>{1} ms</b>\n".format(slowest_response.Name,
        slowest_response.ResponseTimes.max());

    // Fastest call group
    document.getElementById("fastest_call_group_value").innerHTML = "{0} - <b>{1} ms average ({2} calls)</b>\n".format(fastest_response_group.Name,
        fastest_response_group.AverageResponseTime, fastest_response_group.ResponseTimes.length);

    // Slowest call group
    document.getElementById("slowest_call_group_value").innerHTML = "{0} - <b>{1} ms average ({2} calls)</b>\n".format(slowest_response_group.Name,
        slowest_response_group.AverageResponseTime, slowest_response_group.ResponseTimes.length);
}

/*
############################## Requests per second analysis ##############################
*/

var rps_percentage_doughnut_chart = null;
var rps_data = [];

function draw_rps_percentage_doughnut() {
    rps_datasets.forEach(entry => {
        let call_rps_data = {
            Name: entry.label,
            RPSEntries: [],
            AverageRPS: 0,
            PeakRPS: 0,
            TotalRequests: 0
        }

        entry.data.forEach(dataset => {
            call_rps_data.RPSEntries.push(dataset.y);
        })

        // Since there might be zeroes - we eleminate them from calculation by filtering them out
        real_average_rps = call_rps_data.RPSEntries.filter(Number);
        call_rps_data.AverageRPS = ((real_average_rps.reduce((a, b) => a + b, 0)) / real_average_rps.length).toFixed(2);
        call_rps_data.PeakRPS = call_rps_data.RPSEntries.max();
        call_rps_data.TotalRequests = call_rps_data.RPSEntries.reduce((a, b) => a + b, 0);

        rps_data.push(call_rps_data);
    })

    let doughnutLabels = [];
    let doughnutData = [];

    rps_data.forEach(entry => {
        doughnutLabels.push(entry.Name);
        doughnutData.push(entry.TotalRequests)
    })

    let data = {
        datasets: [
            {
                data: doughnutData
            }
        ],
        labels: doughnutLabels
    }

    rps_percentage_doughnut_chart = draw_doughnut_chart(document.getElementById("rps_percentage_doughnut"), data, "");
}

function populate_request_amounts_table() {
    let table_body = document.querySelector("#requests_table tbody");

    rps_data.forEach(entry => {
        let row = document.createElement("tr");
        let name = document.createElement("td");
        let requests_total = document.createElement("td");

        name.innerHTML = entry.Name;
        requests_total.innerHTML = entry.TotalRequests

        row.appendChild(name);
        row.appendChild(requests_total);
        table_body.appendChild(row);
    })
}

function populate_rps_summary_table() {
    var most_frequent_call = null;
    var least_frequent_call = null;
    var highest_rps = null;
    var lowest_rps = null;

    rps_data.forEach(entry => {
        if (most_frequent_call == null) {
            most_frequent_call = entry;
        } else {
            if (entry.TotalRequests > most_frequent_call.TotalRequests) {
                most_frequent_call = entry;
            }
        }
        if (least_frequent_call == null) {
            least_frequent_call = entry;
        } else {
            if (entry.TotalRequests < least_frequent_call.TotalRequests) {
                least_frequent_call = entry;
            }
        }
        if (highest_rps == null) {
            highest_rps = entry;
        } else {
            if (entry.PeakRPS > highest_rps.PeakRPS) {
                highest_rps = entry;
            }
        }
        if (lowest_rps == null) {
            lowest_rps = entry;
        } else {
            if (entry.PeakRPS < lowest_rps.PeakRPS) {
                lowest_rps = entry;
            }
        }
    });

    document.getElementById("most_frequent_call_value").innerHTML = "{0} - <b>{1} times</b>".format(most_frequent_call.Name, most_frequent_call.TotalRequests);
    document.getElementById("least_frequent_call_value").innerHTML = "{0} - <b>{1} times</b>".format(least_frequent_call.Name, least_frequent_call.TotalRequests);
    document.getElementById("highest_rps_value").innerHTML = "{0} - <b>{1} Requests per second peak</b>".format(highest_rps.Name, highest_rps.PeakRPS);
    document.getElementById("lowest_rps_value").innerHTML = "{0} - <b>{1} Requests per second peak</b>".format(lowest_rps.Name, lowest_rps.PeakRPS);
}

/*
############################## Failures analysis ##############################
*/

var failures_percentage_doughnut_chart = null;
var failures_data = [];
// Flag value - We do not draw any analysis graphs and tables if there's nothing to analyze
var failures_found = false;

function failures_distribution_doughnut_chart() {
    // Unlike previous 2 sections - analysis and total chart use different types of data, so we have to iterate over initial data dump once more
    results_data.Results.forEach(entry => {
        let resultSet = new SwarmResultSet(entry.Data)
        let failure_data = {
            Name: entry.Name,
            TotalFailuresByReason: [],
            TotalFailures: 0
        }

        let total_failures_by_reason = new Map();
        for (let index = 0; index < resultSet.Size; index++) {
            let occurrences = resultSet.GetItem("failuresOccurrences", index);
            for (const row in occurrences) {
                if(total_failures_by_reason.has(row)) {
                    total_failures_by_reason.set(row, total_failures_by_reason.get(row) + occurrences[row]);
                } else {
                    total_failures_by_reason.set(row, occurrences[row]);
                }
            }
            failure_data.TotalFailures += resultSet.GetItem("failuresCount", index);
            failure_data.TotalFailuresByReason = total_failures_by_reason;
        }

        if (!failures_found && failure_data.TotalFailures > 0) {
            failures_found = true;
        }
        failures_data.push(failure_data);
    });

    if (failures_found) {
        let doughnutLabels = [];
        let doughnutData = [];

        failures_data.forEach(entry => {
            doughnutLabels.push(entry.Name);
            doughnutData.push(entry.TotalFailures)
        })

        let data = {
            datasets: [
                {
                    data: doughnutData
                }
            ],
            labels: doughnutLabels
        }

        document.getElementById("failures_distribution_na").remove();
        let chart_canvas = document.createElement("canvas");
        chart_canvas.id = "failures_distribution_doughnut";
        document.getElementById("failures_distribution_container").appendChild(chart_canvas);
        failures_percentage_doughnut_chart = draw_doughnut_chart(document.getElementById("failures_distribution_doughnut"), data, "");
    }
}

function draw_failures_amounts_table() {
    if (!failures_found) {return;}

    document.getElementById("failures_amounts_na").remove();

    let headers = [
        { Text: "Call" },
        {
            Text: "Total failed requests",
            Class: "sorttable_numeric"
        }
    ];

    createTable("failures_amount_table", document.getElementById("failures_amounts_container"), headers);

    let table_body = document.querySelector("#failures_amount_table tbody");

    failures_data.forEach(entry => {
        let row = document.createElement("tr");
        let name = document.createElement("td");
        let failures = document.createElement("td");

        name.innerHTML = entry.Name;
        failures.innerHTML = entry.TotalFailures;

        row.appendChild(name);
        row.appendChild(failures);
        table_body.appendChild(row);
    });

    sorttable.makeSortable(document.querySelector("#failures_amount_table"));
}

function draw_failures_by_reason_table() {
    if (!failures_found) {return;}

    document.getElementById("failures_by_reason_na").remove();

    let headers = [
        { Text: "Call" },
        { Text: "Error"},
        {
            Text: "Occurrences",
            Class: "sorttable_numeric"
        }
    ];

    createTable("failures_by_reason_table", document.getElementById("failures_by_reason_container"), headers);

    let table_body = document.querySelector("#failures_by_reason_table tbody");

    failures_data.forEach(entry => {
        entry.TotalFailuresByReason.forEach((value, key) => {
            let row = document.createElement("tr");
            let name = document.createElement("td");
            let error = document.createElement("td");
            let occurrences = document.createElement("td");

            name.innerHTML = entry.Name;
            error.innerHTML = escapeHTML(key);
            occurrences.innerHTML = value;

            row.appendChild(name);
            row.appendChild(error);
            row.appendChild(occurrences);
            table_body.appendChild(row);
        })
    });

    sorttable.makeSortable(document.querySelector("#failures_by_reason_table"));
}

/*
############################## Service functions ##############################
*/

function reset_zoom(chart) {
    chart.resetZoom();
}

function load_data() {
    draw_all_average_response_times();
    display_thresholds();
    create_response_analysis_buttons();
    populate_response_distribution_table();
    populate_responses_summary_table();

    draw_all_rps();
    draw_rps_percentage_doughnut();
    populate_request_amounts_table();
    populate_rps_summary_table();

    draw_all_failures();
    failures_distribution_doughnut_chart();
    draw_failures_amounts_table();
    draw_failures_by_reason_table();
}

/**
 * Simplifies operation of Swarm result set object - it is turned into header-data formatted object to reduce the size.
 * @param {object} data - Data-containing section of the object
 */
function SwarmResultSet(data) {
    this.Headers = data.Headers;
    this.Values = data.Values;
    this.Size = this.Values.length;

    /**
     * Extracts specified value from a data entry at given index.
     * @param {string} valueName - Name of the value 
     * @param {int} index - Index of container from which to extract specified value
     * @returns {object} - Value
     */
    this.GetItem = function (valueName, index) {
        return this.Values[index][this.Headers[valueName]];
    }
}

/**
 * Simple string formatting add-on
 */
String.prototype.format = String.prototype.f = function () {
    var args = arguments;
    return this.replace(/\{(\d+)\}/g, function (m, n) {
        return args[n] ? args[n] : m;
    });
};

/**
 * Convenience add-on for array - allows to get the biggest value in the array
 * @returns - Biggest int value in the array
 */
Array.prototype.max = function () {
    return Math.max.apply(null, this);
};

/**
 * Convenience add-on for array - allows to get the smallest value in the array
 * @returns - Smallest int value in the array
 */
Array.prototype.min = function () {
    return Math.min.apply(null, this);
};

/**
 * Convenience add-on for array - allows to get the smallest non-zero value in the array
 * @returns - Smallest non-zero int value in the array
 */
Array.prototype.minNotZero = function () {
    return Math.min.apply(null, this.filter(Boolean));
};

/**
 * Utility function to create a new table at runtime
 * @param {string} id - ID of the table element 
 * @param {object} container - Element to put the table into (container)
 * @param {list of objects} headers - Table headers (rows of thead block) with desired attributes
 */
function createTable(id, container, headers) {
    let table = document.createElement("table");
    table.id = id;
    let head = document.createElement("thead");
    let body = document.createElement("tbody");
    let foot = document.createElement("tfoot");

    let header_row = document.createElement("tr");
    headers.forEach(header => {
        let entry = document.createElement("th");
        entry.innerHTML = header.Text;
        if (header.hasOwnProperty("Class")) {
            entry.classList.add(header.Class)
        }
        header_row.appendChild(entry);
    })
    head.appendChild(header_row);

    table.appendChild(head);
    table.appendChild(body);
    table.appendChild(foot);

    container.appendChild(table);
}

/**
 * Takes the string and escapes the parts of HTML tags to make sure they're displayed correctly
 * @param {string} value - String to process
 * @returns - Escaped string
 */
function escapeHTML(value) {
    return value.replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#39;");
}