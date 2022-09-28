$(document).ready(function () {
    var connection = new signalR.HubConnectionBuilder().withUrl("/progressHub").build();

    connection.on("UpdateProgressBarFill", function (pctComplete) {
        var width = pctComplete + "%";
        document.getElementById("progressBar").style.width = width;
    });

    connection.on("UpdateProgressBarText", function (text) {
        document.getElementById("progressBarHeader").textContent = text;
    });

    connection.start();
});