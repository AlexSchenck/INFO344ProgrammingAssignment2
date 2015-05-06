$("#query").keyup(function () {
    var query = $("#query").val();
    if (query.length > 0) {
        $.ajax({
            type: "POST",
            url: "getQuerySuggestions.asmx/searchTrie",
            data: "{query: '" + query + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var result = "";
                for (var i = 0; i < msg.d.length; i++) {
                    result += msg.d[i] + "<br>";
                }

                $("#resultsDiv").html(result);
            },
            error: function (msg) {
                $("#resultsDiv").html("Could not get results.");
            }
        });
    }
    else
        $("#resultsDiv").html("");
});