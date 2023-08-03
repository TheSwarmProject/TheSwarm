var total_averages_chart = null;
var total_rps_chart = null;
var total_failures_chart = null;

/*
############################## Total flow charts ##############################
*/
function draw_all_average_response_times() {
    var ctx = document.getElementById("all_average_response_times_chart");

    let datasets = [];
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

        datasets.push(dataset)
    })

    total_averages_chart = draw_total_chart(ctx, results_data.Timestamps, datasets, "Response time, ms")
}

function draw_all_rps() {
    var ctx = document.getElementById("all_requests_per_second_chart");

    let datasets = [];
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

        datasets.push(dataset)
    })

    total_rps_chart = draw_total_chart(ctx, results_data.Timestamps, datasets, "Requests per second")
}

function draw_all_failures() {
    var ctx = document.getElementById("all_failures_chart");

    let datasets = [];
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

        datasets.push(dataset)
    })

    total_failures_chart = draw_total_chart(ctx, results_data.Timestamps, datasets, "Failures")
}

function draw_total_chart(canvas, labels, datasets, valueName) {
    return new Chart(canvas, {
        type: 'line',
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: {
                    display: false,
                    title: {
                        display: true,
                        text: "Timeline"
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
                        enabled: false,
                        mode: 'xy',
                        speed: 20,
                        threshold: 10,
                        scaleMode: "xy"
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
                        speed: 0.1,
                        threshold: 2,
                        sensitivity: 3
                    }
                }
            }   
        },
        data: {
          labels: labels,
          datasets: datasets
        }
      });
}

/*
############################## Service functions ##############################
*/

function reset_zoom(chart) {
    chart.resetZoom();
}

function draw_all_graphs() {
    draw_all_average_response_times();
    draw_all_rps();
    draw_all_failures();
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
    this.GetItem = function(valueName, index){
        return this.Values[index][this.Headers[valueName]];
    }
}