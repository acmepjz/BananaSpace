// This file contains CodeMirror config for the TeX language.
// The readonly version is in order for IE to correctly colorize code snippets, since it does not support async functions.

(function (mod) {
    if (typeof exports == "object" && typeof module == "object") // CommonJS
        mod(require("../../lib/codemirror"));
    else if (typeof define == "function" && define.amd) // AMD
        define(["../../lib/codemirror"], mod);
    else // Plain browser env
        mod(CodeMirror);
})(function (CodeMirror) {
    "use strict";

    CodeMirror.defineMode("tex", function () {
        function token(stream, state) {
            if (state.readingText) {
                var type = state.textType;
                state.textType = null;
                state.readingText = false;
                if (stream.match(/^[^~#$%^&_{}\\\s]*/)) {
                    if (stream.match(/(?=})/)) {
                        state.textType = type;
                        return type;
                    }
                    else
                        return "wrong-" + type;
                }
            }
            if (stream.match(/^\\\(|\\\)|\\\[|\\\]|\$|\^|_|~|&/)) {
                state.textType = null;
                return "special";
            }
            var result = stream.match(/^\\([^0-9`~!#$%^&*()\-_=+[\]{}\\|;:'",.<>\/?\s]+|.|$)/);
            if (result) {
                state.textType =
                    result[1] == "begin" ? "environment begin" :
                    result[1] == "end" ? "environment end" :
                    result[1].match(/^(@defenv|newtheorem|newproof)$/) ? "environment" : 
                    result[1].match(/^(ref|label|eqref)$/) ? "label" : null;
                state.readingText = false;
                return result[1] == "begin" ? "command begin" :
                    result[1] == "end" ? "command end" :
                    "command";
            }
            if (stream.eat("%")) {
                stream.skipToEnd();
                return "comment";
            }
            if (stream.eat("{")) {
                if (state.textType) {
                    if (stream.eat("}")) {
                        return state.textType == "environment begin" ? "empty-environment begin" :
                            state.textType == "environment end" ? "empty-environment end" : null;
                    }
                    state.readingText = true;
                    return state.textType == "environment begin" ? "begin" :
                        state.textType == "environment end" ? "end" : null;
                }
                return null;
            }
            if (stream.eat("}")) {
                var type = null;
                if (state.textType)
                    type = state.textType == "environment begin" ? "begin" :
                        state.textType == "environment end" ? "end" : null;
                state.textType = null;
                return type;
            }
            if (stream.eat(/\s/)) {
                return "whitespace";
            }
            state.textType = null;

            if (stream.match(/^#+./)) {
                return "argument";
            }
            stream.next();
            return null;
        }

        return {
            startState: function () {
                return { stringCmd: 0 };
            },
            token: token,
            lineComment: "%"
        };
    });
});