// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

( function($) {

    var defaultContent = `@{
    var foo = "Hello World!";
}

<div>@foo</div>`;

    $(document).ready(function() {
        // Set initial content.
        $('textarea#source').val(defaultContent);
        getParseTree(defaultContent);
        getParseIRTree(defaultContent);
        getNewParseTree(defaultContent);

        cm = CodeMirror.fromTextArea($('textarea#source')[0], {
            lineNumbers: true,
            mode: "htmlmixed",
            indentUnit: 4,
            autofocus: true
        });
        cm.on("change", function (cm, obj) {
            getParseTree(cm.getValue());
            getParseIRTree(cm.getValue());
            getNewParseTree(cm.getValue());
        });
    });

    function markText(start, length) {
        cm.markText(cm.posFromIndex(start), cm.posFromIndex(start + length), {
            clearOnEnter: true,
            className: 'highlight'
        });
    }

    function resetMark(start, length) {
        var marks = cm.findMarks(cm.posFromIndex(start), cm.posFromIndex(start + length));
        for (var mark of marks) {
            mark.clear();
        }
    }

    function getParseTree(input) {
        $.ajax({
            url: 'api/Parse',
            dataType: 'json',
            type: 'post',
            cache: false,
            contentType: 'application/json',
            data: JSON.stringify({ "content": input }),
            success: function (html) {
                var result = $('<ul />');
                transform(html, result, 0);
                $("#results").html(result);
            },
            error: function(jqXhr, textStatus, errorThrown){
                console.log("error: " + errorThrown);
            }
        });
    }

    function getParseIRTree(input) {
        $.ajax({
            url: 'api/ParseIR',
            dataType: 'json',
            type: 'post',
            cache: false,
            contentType: 'application/json',
            data: JSON.stringify({ "content": input }),
            success: function (html) {
                var result = $('<ul />');
                //transform(html, result, 0);
                $("#irtree").html(html);
            },
            error: function(jqXhr, textStatus, errorThrown){
                console.log("error: " + errorThrown);
            }
        });
    }

    function getNewParseTree(input) {
        $.ajax({
            url: 'api/NewParse',
            dataType: 'json',
            type: 'post',
            cache: false,
            contentType: 'application/json',
            data: JSON.stringify({ "content": input }),
            success: function (html) {
                var result = $('<ul />');
                transform(html, result, 0);
                $("#newtree").html(result);
            },
            error: function(jqXhr, textStatus, errorThrown){
                console.log("error: " + errorThrown);
            }
        });
    }

    function htmlEncode(value) {
        // JQuery automatically encodes this.
        return $('<div/>').text(value).html();
    }

    function transform(obj, parent, count) {
        if (obj == null) return;

        var collapseTarget = 'children' + count;
        var span = $('<span />').html(htmlEncode(obj["Content"]));
        span.hover(function () {
            markText(obj["Start"], obj["Length"]);
        }, function() {
            resetMark(obj["Start"], obj["Length"]);
        });
        var li = $('<li />').attr({ 'data-toggle': 'collapse', 'data-target': '#' + collapseTarget }).html(span);
        parent.append(li);

        var children = obj["Children"];
        if (children && children.length)
        {
            li.attr('class', 'block');
            var newParent = $('<ul />').attr({ class: 'collapse in', id: collapseTarget });
            for (var item of children) {
                transform(item, newParent, count + 1);
                if (item["Children"] && item["Children"].length) {
                    count += item["Children"].length;
                }
            }
            parent.append(newParent);
        }
    }



} ) ( jQuery );
