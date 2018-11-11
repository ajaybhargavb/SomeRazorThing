window.razorlabJs = {
    getElementValue: function (element) {
        var cm = $(element).next('.CodeMirror')[0].CodeMirror;
        return cm.getValue();
    },
    initializeCodeMirror: function (element, onChangeTarget) {
        var cm = CodeMirror.fromTextArea($(element)[0], {
            lineNumbers: true,
            mode: "htmlmixed",
            indentUnit: 4,
            autofocus: true
        });
        cm.on("change", function (cm, obj) {
            onChangeTarget.invokeMethodAsync('SourceChanged');
        });
    },
    markText: function(element, start, length) {
        var cm = $(element).next('.CodeMirror')[0].CodeMirror;
        cm.markText(cm.posFromIndex(start), cm.posFromIndex(start + length), {
            clearOnEnter: true,
            className: 'highlight'
        });
    },
    resetMark: function(element, start, length) {
        var cm = $(element).next('.CodeMirror')[0].CodeMirror;
        var marks = cm.findMarks(cm.posFromIndex(start), cm.posFromIndex(start + length));
        for (var mark of marks) {
            mark.clear();
        }
    },
  };