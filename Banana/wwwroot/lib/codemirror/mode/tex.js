// This file contains CodeMirror config for the TeX language.

(function (mod) {
    if (typeof exports == "object" && typeof module == "object") // CommonJS
        mod(require("../../lib/codemirror"));
    else if (typeof define == "function" && define.amd) // AMD
        define(["../../lib/codemirror"], mod);
    else // Plain browser env
        mod(CodeMirror);
})(function (CodeMirror) {
    "use strict";

    var commands = ["\\AA","\\above","\\abovewithdelims","\\acute","\\addtocounter","\\adef","\\ae","\\AE","\\aedef","\\alef","\\aleph","\\alet","\\alph","\\Alph","\\alpha","\\Alpha","\\amalg","\\and","\\And","\\ang","\\angle","\\approx","\\approxeq","\\arabic","\\arccos","\\arcsin","\\arctan","\\arg","\\array","\\arrowvert","\\Arrowvert","\\ast","\\asymp","\\atop","\\atopwithdelims","\\backepsilon","\\backprime","\\backsim","\\backsimeq","\\backslash","\\bar","\\barwedge","\\Bbb","\\Bbbk","\\bbox","\\because","\\begin","\\beta","\\Beta","\\beth","\\between","\\bf","\\big","\\Big","\\bigcap","\\bigcirc","\\bigcup","\\bigg","\\Bigg","\\biggl","\\Biggl","\\biggm","\\Biggm","\\biggr","\\Biggr","\\bigl","\\Bigl","\\bigm","\\Bigm","\\bigodot","\\bigoplus","\\bigotimes","\\bigr","\\Bigr","\\bigsqcup","\\bigstar","\\bigtriangledown","\\bigtriangleup","\\biguplus","\\bigvee","\\bigwedge","\\binom","\\blacklozenge","\\blacksquare","\\blacktriangle","\\blacktriangledown","\\blacktriangleleft","\\blacktriangleright","\\bmod","\\bold","\\boldsymbol","\\bot","\\bowtie","\\Box","\\boxdot","\\boxed","\\boxminus","\\boxplus","\\boxtimes","\\brace","\\bracevert","\\brack","\\breve","\\buildrel","\\bull","\\bullet","\\Bumpeq","\\cal","\\cap","\\Cap","\\cases","\\cdot","\\cdotp","\\cdots","\\centerdot","\\cfrac","\\check","\\checkmark","\\chi","\\Chi","\\choose","\\circ","\\circeq","\\circlearrowleft","\\circlearrowright","\\circledast","\\circledcirc","\\circleddash","\\circledR","\\circledS","\\class","\\clubs","\\clubsuit","\\cnums","\\colon","\\color","\\complement","\\Complex","\\cong","\\coppa","\\Coppa","\\coprod","\\cos","\\cosh","\\cot","\\coth","\\cr","\\csc","\\cssId","\\cup","\\Cup","\\curlyeqprec","\\curlyeqsucc","\\curlyvee","\\curlywedge","\\curvearrowleft","\\dagger","\\Dagger","\\daleth","\\Darr","\\dashleftarrow","\\dashrightarrow","\\dashv","\\dbinom","\\ddagger","\\ddddot","\\dddot","\\ddot","\\ddots","\\DeclareMathOperator","\\def","\\defa","\\deg","\\delta","\\Delta","\\det","\\dfrac","\\diagdown","\\diagup","\\diamond","\\Diamond","\\diamonds","\\diamondsuit","\\digamma","\\Digamma","\\dim","\\displaylines","\\displaystyle","\\div","\\divideontimes","\\dot","\\doteq","\\Doteq","\\doteqdot","\\dotplus","\\dots","\\dotsb","\\dotsc","\\dotsi","\\dotsm","\\dotso","\\doublebarwedge","\\doublecap","\\doublecup","\\downarrow","\\Downarrow","\\downdownarrows","\\downharpoonleft","\\edef","\\edefa","\\ell","\\em","\\emph","\\empty","\\emptyset","\\end","\\enspace","\\epsilon","\\Epsilon","\\eqalign","\\eqalignno","\\eqcirc","\\eqno","\\eqref","\\eqsim","\\eqslantgtr","\\eqslantless","\\equiv","\\eta","\\Eta","\\eth","\\euro","\\exist","\\exists","\\fallingdotseq","\\fbox","\\Finv","\\flat","\\floatleft","\\floatright","\\forall","\\frac","\\frak","\\Game","\\gamma","\\Gamma","\\gcd","\\ge","\\geneuro","\\geneuronarrow","\\geneurowide","\\genfrac","\\geq","\\geqq","\\geqslant","\\gets","\\gg","\\ggg","\\gggtr","\\gimel","\\gnapprox","\\gneq","\\gneqq","\\gnsim","\\grave","\\gt","\\gtrapprox","\\gtrdot","\\gtreqless","\\gtreqqless","\\gtrless","\\gtrsim","\\hAar","\\harr","\\Harr","\\hat","\\hbar","\\hbox","\\hdashline","\\hearts","\\heartsuit","\\hline","\\hom","\\hookleftarrow","\\hookrightarrow","\\hphantom","\\href","\\hskip","\\hslash","\\hspace","\\huge","\\Huge","\\iff","\\iiiint","\\iiint","\\iint","\\Im","\\image","\\imath","\\impliedby","\\implies","\\in","\\inf","\\infin","\\infty","\\injlim","\\int","\\intercal","\\intop","\\iota","\\Iota","\\isin","\\it","\\item","\\jmath","\\kappa","\\Kappa","\\ker","\\kern","\\koppa","\\label","\\lambda","\\Lambda","\\land","\\lang","\\langle","\\large","\\Large","\\LARGE","\\larr","\\lArr","\\Larr","\\LaTeX","\\lbrace","\\lbrack","\\lceil","\\ldotp","\\ldots","\\le","\\leadsto","\\left","\\leftarrow","\\Leftarrow","\\leftarrowtail","\\leftharpoondown","\\leftharpoonup","\\leftleftarrows","\\leftrightarrow","\\Leftrightarrow","\\leftrightarrows","\\leftrightharpoons","\\leftrightsquigarrow","\\leftroot","\\leftthreetimes","\\leq","\\leqalignno","\\leqq","\\leqslant","\\lessapprox","\\lessdot","\\lesseqgtr","\\lesseqqgtr","\\lessgtr","\\lesssim","\\let","\\leta","\\lfloor","\\lg","\\lgroup","\\lhd","\\lim","\\liminf","\\limits","\\limsup","\\ll","\\llap","\\llcorner","\\Lleftarrow","\\lll","\\llless","\\lmoustache","\\ln","\\lnapprox","\\lneq","\\lneqq","\\lnot","\\lnsim","\\log","\\longleftarrow","\\Longleftarrow","\\longleftrightarrow","\\Longleftrightarrow","\\longmapsto","\\longrightarrow","\\Longrightarrow","\\looparrowleft","\\looparrowright","\\lor","\\lower","\\lozenge","\\lrarr","\\lrArr","\\Lrarr","\\lrcorner","\\Lsh","\\lt","\\ltimes","\\lvert","\\lVert","\\maltese","\\mapsto","\\mathbb","\\mathbf","\\mathbin","\\mathcal","\\mathchoice","\\mathclose","\\mathfrak","\\mathinner","\\mathit","\\mathop","\\mathopen","\\mathord","\\mathpunct","\\mathrel","\\mathring","\\mathrm","\\mathscr","\\mathsf","\\mathstrut","\\mathtt","\\matrix","\\max","\\mbox","\\md","\\measuredangle","\\mho","\\mid","\\middle","\\min","\\mit","\\mkern","\\mod","\\models","\\moveleft","\\moveright","\\mp","\\mskip","\\mspace","\\mu","\\Mu","\\nabla","\\natnums","\\natural","\\ncong","\\ne","\\nearrow","\\neg","\\negmedspace","\\negthickspace","\\negthinspace","\\neq","\\newcommand","\\newcounter","\\newenvironment","\\newline","\\newproof","\\newtheorem","\\nexists","\\ngeq","\\ngeqq","\\ngeqslant","\\ngtr","\\ni","\\nleftarrow","\\nLeftarrow","\\nleftrightarrow","\\nLeftrightarrow","\\nleq","\\nleqq","\\nleqslant","\\nless","\\nmid","\\nobreakspace","\\nolimits","\\normalsize","\\not","\\notag","\\notin","\\nparallel","\\nprec","\\npreceq","\\nrightarrow","\\nRightarrow","\\nshortmid","\\nshortparallel","\\nsim","\\nsubseteq","\\nsubseteqq","\\nsucc","\\nsucceq","\\nsupseteq","\\nsupseteqq","\\ntriangleleft","\\ntrianglelefteq","\\ntriangleright","\\ntrianglerighteq","\\nu","\\Nu","\\nvdash","\\nvDash","\\nVdash","\\nVDash","\\odot","\\oe","\\OE","\\officialeuro","\\oint","\\oiint","\\oiiint","\\oldstyle","\\omega","\\Omega","\\omicron","\\Omicron","\\ominus","\\operatorname","\\oplus","\\or","\\oslash","\\otimes","\\over","\\overbrace","\\overleftarrow","\\overleftrightarrow","\\overline","\\overrightarrow","\\overset","\\overwithdelims","\\pagecolor","\\par","\\parallel","\\part","\\partial","\\perp","\\phantom","\\phi","\\Phi","\\pi","\\Pi","\\pitchfork","\\plusmn","\\pm","\\pmatrix","\\pmb","\\pmod","\\pod","\\Pr","\\prec","\\precapprox","\\preccurlyeq","\\preceq","\\precnapprox","\\precneqq","\\precnsim","\\precsim","\\prime","\\prod","\\projlim","\\propto","\\Psi","\\qed","\\qedhere","\\qedsymbol","\\qquad","\\raise","\\rang","\\rangle","\\rarr","\\rArr","\\Rarr","\\rbrace","\\rbrack","\\rceil","\\Re","\\real","\\reals","\\Reals","\\ref","\\refstepcounter","\\renewcommand","\\renewenvironment","\\reqno","\\restriction","\\rfloor","\\rgroup","\\rhd","\\rho","\\Rho","\\right","\\rightarrow","\\Rightarrow","\\rightarrowtail","\\rightharpoondown","\\rightharpoonup","\\rightleftarrows","\\rightleftharpoons","\\rightrightarrows","\\rightsquigarrow","\\rightthreetimes","\\risingdotseq","\\rlap","\\rm","\\rmoustache","\\roman","\\Roman","\\root","\\Rrightarrow","\\Rsh","\\rtimes","\\rVert","\\sampi","\\Sampi","\\scr","\\scriptscriptstyle","\\scriptsize","\\scriptstyle","\\sdot","\\searrow","\\sec","\\sect","\\setcounter","\\setminus","\\setparsep","\\sf","\\sharp","\\shortmid","\\shortparallel","\\shoveleft","\\shoveright","\\sideset","\\sigma","\\Sigma","\\sim","\\simeq","\\sin","\\sinh","\\skew","\\small","\\smallfrown","\\smallint","\\smallsetminus","\\smallsmile","\\smash","\\smile","\\space","\\spades","\\spadesuit","\\sphericalangle","\\sqcap","\\sqcup","\\sqrt","\\sqsubset","\\sqsubseteq","\\sqsupset","\\sqsupseteq","\\square","\\ss","\\SS","\\stackrel","\\star","\\stepcounter","\\stigma","\\Stigma","\\strut","\\style","\\sub","\\sube","\\subset","\\Subset","\\subseteq","\\subseteqq","\\subsetneq","\\subsetneqq","\\substack","\\succ","\\succapprox","\\succcurlyeq","\\succeq","\\succnapprox","\\succneqq","\\succnsim","\\succsim","\\sum","\\sup","\\supe","\\supset","\\Supset","\\supseteq","\\supseteqq","\\supsetneq","\\supsetneqq","\\surd","\\tag","\\tan","\\tanh","\\tau","\\Tau","\\tbinom","\\tdef","\\TeX","\\text","\\textbf","\\textcolor","\\textit","\\textmd","\\textrm","\\textsf","\\textstyle","\\texttt","\\textvisiblespace","\\tfrac","\\theequation","\\theoremstyle","\\therefore","\\theta","\\Theta","\\thetasym","\\thetheorem","\\thickapprox","\\thicksim","\\thinspace","\\thmprefix","\\tilde","\\times","\\tiny","\\to","\\top","\\triangle","\\triangledown","\\triangleleft","\\trianglelefteq","\\triangleq","\\triangleright","\\trianglerighteq","\\tt","\\twoheadleftarrow","\\uarr","\\uArr","\\Uarr","\\ulcorner","\\underbrace","\\underleftarrow","\\underleftrightarrow","\\underline","\\underrightarrow","\\underset","\\unicode","\\unlhd","\\unrhd","\\uparrow","\\Uparrow","\\updownarrow","\\Updownarrow","\\upharpoonleft","\\upharpoonright","\\uplus","\\uproot","\\upsilon","\\Upsilon","\\upuparrows","\\varcoppa","\\varDelta","\\varepsilon","\\varGamma","\\varinjlim","\\varkappa","\\varLambda","\\varliminf","\\varlimsup","\\varnothing","\\varOmega","\\varphi","\\varPhi","\\varpi","\\varPi","\\varprojlim","\\varpropto","\\varPsi","\\varrho","\\varsigma","\\varSigma","\\varstigma","\\varsubsetneq","\\varsubsetneqq","\\varsupsetneq","\\varsupsetneqq","\\vartheta","\\varTheta","\\vartriangle","\\vartriangleleft","\\vartriangleright","\\varUpsilon","\\varXi","\\vcenter","\\vdash","\\vDash","\\Vdash","\\vdots","\\vec","\\vee","\\veebar","\\verb","\\vert","\\Vert","\\vline","\\vphantom","\\wedge","\\weierp","\\widehat","\\widetilde","\\wp","\\xi","\\Xi","\\xleftarrow","\\xrightarrow","\\zeta","\\Zeta"];
    var environments = ["corollary","definition","enumerate","equation","equation*","itemize","lemma","proof","proposition","remark","theorem"];

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
                    result[1].match(/^(@defenv|newtheorem|newproof)$/) ? "environment" : null;
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

    // auto completion
    function hintHelper(cm, options) {
        if (!cm.allCommands)
            cm.allCommands = commands;
        if (!cm.allEnvironments)
            cm.allEnvironments = environments;

        if (cm.state.hintMode == "command") {
            var pos = { line: cm.state.hintFrom.line, ch: cm.state.hintFrom.ch + 1 };
            var token = cm.getTokenAt(pos, true);
            if (token.start != cm.state.hintFrom.ch)
                return null;

            var text = token.string.substring(0, cm.getCursor().ch - cm.state.hintFrom.ch);
            var matches = [];

            var exact = -1, recent = -1, recentIndex = -1, selectedIndex = 0;
            cm.allCommands.forEach(s => {
                if (s.toLowerCase().startsWith(text.toLowerCase())) {
                    var len = matches.push(s);
                    if (s == "\\end") {
                        var match = scanForMatching(cm, pos, -1, "environment");
                        if (match) {
                            len = matches.push("\\end{" + match.token.string + "}");
                            if (len == 2) selectedIndex = 1;
                        }
                    }
                    if (s == text)
                        exact = len - 1;
                    var index = cm.state.recentCommands.indexOf(s);
                    if (index != -1 && (recent == -1 || index < recent)) {
                        recent = index;
                        recentIndex = len - 1;
                    }
                }
            });

            if (exact != -1) 
                selectedIndex = exact;
            else if (recentIndex != -1)
                selectedIndex = recentIndex;

            options.completeSingle = false;
            cm.state.pickHintOnClose = text.length >= 2;
            cm.state.selectedHint = selectedIndex;

            if (matches.length == 0)
                cm.state.hintMode = null; // prevents competion from reopening when user hits backspace

            return {
                list: matches,
                from: cm.state.hintFrom,
                to: cm.getCursor()
                // 'selectedHint: selectedIndex' should be here,
                // but the selected item would not be scrolled into view, so we do this on "shown"
            };
        }
        else if (cm.state.hintMode == "environment") {
            var pos = cm.getCursor();
            var token = cm.getTokenAt(pos, true);
            if (token.string != "{" &&
                ((token.type != "environment begin" && token.type != "wrong-environment begin") ||
                    token.start != cm.state.hintFrom.ch))
                return null;

            var text = token.string == "{" ? "" : token.string.substring(0, cm.getCursor().ch - cm.state.hintFrom.ch);
            var matches = [];

            var exact = -1;
            var recent = -1, recentIndex = -1;
            cm.allEnvironments.forEach(s => {
                if (s.toLowerCase().startsWith(text.toLowerCase())) {
                    var len = matches.push({ text: s + "}", displayText: s });
                    if (s == text)
                        exact = len - 1;
                    var index = cm.state.recentEnvironments.indexOf(s);
                    if (index != -1 && (recent == -1 || index < recent)) {
                        recent = index;
                        recentIndex = len - 1;
                    }
                }
            });

            var selectedIndex = 0;
            if (exact != -1)
                selectedIndex = exact;
            else if (recentIndex != -1)
                selectedIndex = recentIndex;

            options.completeSingle = false;
            cm.state.pickHintOnClose = true;
            cm.state.selectedHint = selectedIndex;

            if (matches.length == 0)
                cm.state.hintMode = null;

            return {
                list: matches,
                from: cm.state.hintFrom,
                to: cm.getCursor()
            };
        }
    }

    CodeMirror.registerHelper("hint", "tex", (cm, options) => {
        var result = hintHelper(cm, options);
        // subscribe to events
        if (result) {
            CodeMirror.on(result, "shown", () => {
                var completion = cm.state.completionActive;
                if (completion && completion.widget)
                    completion.widget.changeActive(cm.state.selectedHint || 0, false);
            });

            if (cm.state.hintMode == "command") {
                CodeMirror.on(result, "pick", item => { // item: string
                    if (item.startsWith("\\end{")) return;
                    cm.state.recentCommands = cm.state.recentCommands.filter(s => s != item);
                    cm.state.recentCommands.unshift(item);
                    while (cm.state.recentCommands.length > 100)
                        cm.state.recentCommands.pop();
                });
            } else if (cm.state.hintMode == "environment") {
                CodeMirror.on(result, "pick", item => {
                    cm.state.recentEnvironments = cm.state.recentEnvironments.filter(s => s != item.displayText);
                    cm.state.recentEnvironments.unshift(item.displayText);
                    while (cm.state.recentEnvironments.length > 100)
                        cm.state.recentEnvironments.pop();
                    cm.state.beginEnvLine = cm.getCursor().line;
                });
            }
        }
        return result;
    });

    function wait(timeout) {
        return new Promise(resolve => {
            setTimeout(() => resolve(), timeout);
        });
    }

    function stringCompare(a, b) {
        var result = a.localeCompare(b, 'en', { sensitivity: 'base' });
        if (result != 0) return result;
        return a.localeCompare(b);
    }

    function doUpdateTokens(cm) {
        var allCommands = [], allEnvironments = [];
        commands.forEach(s => allCommands.push(s));
        environments.forEach(s => allEnvironments.push(s));

        var pos = cm.getCursor();
        for (var i = 0; i < cm.lineCount(); i++) {
            cm.getLineTokens(i).forEach(token => {
                if (token.type == "command" &&
                    token.string.match(/^\\[a-zA-Z]+$/) &&
                    !allCommands.includes(token.string) &&
                    !(i == pos.line && token.start <= pos.ch && token.end >= pos.ch))
                    allCommands.push(token.string);
                if (token.type && token.type.startsWith("environment") && token.type != "environment end" &&
                    !allEnvironments.includes(token.string) &&
                    !(i == pos.line && token.start <= pos.ch && token.end >= pos.ch))
                    allEnvironments.push(token.string);
            });
        }
        allCommands.sort(stringCompare);
        allEnvironments.sort(stringCompare);
        cm.allCommands = allCommands;
        cm.allEnvironments = allEnvironments;
    }

    async function updateTokens(cm) {
        cm.needsUpdatingTokens = true;
        if (cm.workerStarted) 
            return;

        cm.workerStarted = true;
        while (true) {
            if (cm.needsUpdatingTokens)
                doUpdateTokens(cm);
            cm.needsUpdatingTokens = false;
            await wait(1000);
        }
    }

    function updateTokensAsync(cm) {
        setTimeout(() => updateTokens(cm), 0);
    }

    CodeMirror.defineExtension("init", cm => {
        updateTokensAsync(cm);
        cm.state.recentCommands = [];
        cm.state.recentEnvironments = [];
        cm.on("endCompletion", cm => {
            if (!cm.state.hintUpdating)
                cm.state.hintMode = null;
        });
        cm.on("beforeChange", (cm, change) => {
            var pos = cm.getCursor();
            var token = cm.getTokenAt(pos);
            if (change.text == "}" && cm.state.hintMode == "environment" && change.origin == "+input") {
                var completion = cm.state.completionActive;
                if (completion && completion.widget) {
                    change.cancel();
                    completion.widget.pick();
                }
            }
            if (/\b(begin|end)\b/.test(token.type) && (token.string == "{" || /\benvironment\b/.test(token.type))) {
                var dir = /\bbegin\b/.test(token.type) ? 1 : -1;
                var range = getEnvironmentRange(cm, pos);
                var match = range && scanForMatching(cm, { line: pos.line, ch: range.end - 1 }, dir, "environment");
                cm.state.envSync = null;
                if (match && match.token.string == range.string) {
                    var selections = cm.listSelections();
                    if (selections.length == 1) {
                        var sel = selections[0];
                        if (sel.head.line == sel.anchor.line && sel.head.ch > range.start && sel.head.ch < range.end &&
                            sel.anchor.ch > range.start && sel.anchor.ch < range.end)
                            cm.state.envSync = match;
                    }
                }
            }
        })
        cm.on("change", (cm, change) => {
            var pos = cm.getCursor();
            var token = cm.getTokenAt(pos);
            if (cm.state.nextHintMode) {
                // when user types '\xxx' and hits '\', a command is picked and a change is fired, 
                // causing the new completion widget to disappear. this fixes it.
                cm.state.hintMode = cm.state.nextHintMode;
                if (cm.state.hintMode == "command")
                    cm.state.hintFrom = { line: pos.line, ch: pos.ch - 1 };
                else
                    cm.state.hintFrom = pos;
                cm.state.hintUpdating = true;
                cm.state.nextHintMode = null;
                return;
            }
            cm.state.nextHintMode = null;
            if (change.origin == "+input" && change.text == "\\") {
                // completion for commands
                var completion = cm.state.completionActive;
                if (completion && completion.widget && cm.state.pickHintOnClose) {
                    completion.widget.pick();
                    cm.state.nextHintMode = "command";
                }
                cm.state.hintFrom = change.from;
                cm.state.hintMode = "command";
                cm.state.hintUpdating = true;
            } else if ((change.origin == "+input" && change.text.length == 1 &&
                /^[^0-9`~!#$%^&*()\-_=+[\]{}\\|;:'",.<>\/?\s]$/.test(change.text[0])) ||
                (change.text == "" && change.removed.length == 1 && /^[^\\]$/.test(change.removed[0]))) {
                cm.state.hintUpdating = true;
                // keep hintMode unchanged
            } else {
                var picked = false;
                cm.state.hintMode = null;
                var completion = cm.state.completionActive;
                if (completion && completion.widget && cm.state.pickHintOnClose) {
                    completion.widget.pick();
                    picked = true;
                }

                // completion for environments
                if (change.origin == "+input" && change.text == "{" && cm.getTokenAt({ line: pos.line, ch: pos.ch - 1 }).string == "\\begin") {
                    cm.state.hintFrom = pos;
                    cm.state.hintMode = "environment";
                    if (picked) cm.state.nextHintMode = "environment";
                    cm.state.hintUpdating = true;
                } else if (change.text == "}" && /\bbegin\b/.test(token.type)) {
                    cm.state.beginEnvLine = pos.line;
                }
            }

            // auto insert '\end{...}' when user hits enter
            if (change.origin == "+input" && change.removed == "" &&
                change.text.length == 2 && change.text[0] + change.text[1] == "" &&
                cm.getSelections().length == 1) {
                var tokens = cm.getLineTokens(pos.line - 1, true);
                var i = tokens.length;
                var depth = 0;
                while (i--) {
                    if (!tokens[i].type) depth += (tokens[i].string == "{" ? -1 : tokens[i].string == "}" ? 1 : 0);
                    if (depth == 0 && tokens[i].type == "environment begin") break;
                }
                if (i >= 0) {
                    var string = tokens[i].string;
                    if (cm.state.beginEnvLine != pos.line - 1) {
                        var match = scanForMatching(cm, { line: pos.line - 1, ch: tokens[i].end }, 1, "environment");
                        if (match && match.token.string == string)
                            string = null; // don't insert \end
                    }
                    if (string) {
                        cm.replaceRange("\r\n" + " ".repeat(pos.ch) + "\\end{" + string + "}", pos, null, "+move");
                        cm.setCursor(pos);
                    }
                }
            }

            // sync matching environments
            if (cm.state.envSync && /\b(begin|end)\b/.test(token.type) && (token.string == "{" || /\benvironment\b/.test(token.type))) {
                var range = getEnvironmentRange(cm, pos);
                if (range)
                    cm.replaceRange(range.string, { line: cm.state.envSync.line, ch: cm.state.envSync.token.start },
                        { line: cm.state.envSync.line, ch: cm.state.envSync.token.end }, "+move");
            }
            cm.state.envSync = null;

            updateTokensAsync(cm);
            window.onbeforeunload = () => true;
        });
        cm.on("changes", cm => {
            if (cm.state.hintMode)
                cm.showHint();
            else if (!cm.state.hintUpdating)
                cm.closeHint();
            cm.state.hintUpdating = false;
        });
        cm.state.matchBrackets = {};
        cm.on("cursorActivity", cm => {
            var pos = cm.getCursor();
            if (cm.state.beginEnvLine != pos.line)
                cm.state.beginEnvLine = null;

            doMatchBrackets(cm);
        });
    });

    // bracket matching: this is a modified version of the original addon.
    // we are not using the original one since we also want to match \begin{...} and \end{...}
    var matching = { "(": ")>", ")": "(<", "[": "]>", "]": "[<", "{": "}>", "}": "{<" };
    var matchingRegex = { "(": /[()[\]{}]/, ")": /[()[\]{}]/, "[": /[()[\]{}]/, "]": /[()[\]{}]/, "{": /[{}]/, "}": /[{}]/ };
    var maxHighlightLen = 1000, maxScanLen = 10000, maxScanLines = 1000; // disable matching in long lines
    function scanForMatching(cm, where, dir, mode, regex) {
        var depth = 0, depth2 = 0;
        var lineEnd = dir > 0 ? Math.min(where.line + maxScanLines, cm.lastLine() + 1)
            : Math.max(cm.firstLine() - 1, where.line - maxScanLines);
        for (var lineNo = where.line; lineNo != lineEnd; lineNo += dir) {
            var line = cm.getLine(lineNo);
            if (!line) continue;
            if (line.length > maxScanLen) continue;
            var tokens = cm.getLineTokens(lineNo);
            var i = dir > 0 ? 0 : tokens.length - 1, end = dir > 0 ? tokens.length : -1;
            if (lineNo == where.line) {
                for (i = 0; i < tokens.length; i++)
                    if (tokens[i].end >= where.ch) break;
                i += dir;
            }
            for (; i != end; i += dir) {
                var token = tokens[i];
                if (!token) break;
                if (mode == "bracket" && (token.type == null || /^(begin|end)$/.test(token.type)) && (regex || /^[()[\]{}]$/).test(token.string)) {
                    var match = matching[token.string];
                    if ((match.charAt(1) == ">") == (dir > 0)) depth++;
                    else if (depth == 0) return depth2 == 0 && { line: lineNo, token: token };
                    else depth--;
                    if (/^[{}]$/.test(token.string))
                        depth2 += (dir > 0) == (token.string == "{") ? 1 : -1;
                } else if (mode == "environment") {
                    var re = token.type && token.type.match(/^empty-(environment (begin|end))$/);
                    if (re) token = { type: re[1], string: "", start: token.start + 1, end: token.start + 1 };
                    if (token.type == "environment begin" || token.type == "empty-environment begin") {
                        if (dir > 0) depth++;
                        else if (depth == 0) return depth2 == 0 && { line: lineNo, token: token };
                        else depth--;
                    } else if (token.type == "environment end" || token.type == "empty-environment end") {
                        if (dir <= 0) depth++;
                        else if (depth == 0) return depth2 == 0 && { line: lineNo, token: token };
                        else depth--;
                    } else if (/^[{}]$/.test(token.string))
                        depth2 += (dir > 0) == (token.string == "{") ? 1 : -1;
                }
            }
        }
        return lineNo - dir == (dir > 0 ? cm.lastLine() : cm.firstLine()) ? false : null;
    }

    // gets the range of '\begin{xxx}' (4 tokens)
    function getEnvironmentRange(cm, where) {
        function tokenIndex(token) {
            if (token.type && /\b(begin|end)\b/.test(token.type)) {
                if (/\bcommand\b/.test(token.type))
                    return 1;
                if (/\bempty-environment\b/.test(token.type))
                    return 5;
                if (/\benvironment\b/.test(token.type))
                    return 3;
                return token.string == "{" ? 2 : token.string == "}" ? 4 : null;
            }
        }
        var tokens = cm.getLineTokens(where.line);
        var i = 0;
        while (tokens[i] && tokens[i].end < where.ch) i++;
        var index = tokens[i] && tokenIndex(tokens[i]);
        if (!index) return null;
        var start, end, string = "", k = index == 5 ? 2 : index;
        if (index == 3) string = tokens[i].string;
        if (index == 1) start = tokens[i].start; else {
            for (var j = i - 1; j >= 0; j--) {
                if (tokens[j].type && /\bwhitespace\b/.test(tokens[j].type)) continue;
                var ti = tokenIndex(tokens[j]);
                if (ti != --k) if (!(k == 3 && ti == --k)) break;
                if (k == 3) string = tokens[j].string;
                if (k == 1) { start = tokens[j].start; break; }
            }
            if (start === undefined) return null;
        }
        k = index;
        if (index == 4 || index == 5) end = tokens[i].end; else {
            for (var j = i + 1; tokens[j]; j++) {
                if (tokens[j].type && /\bwhitespace\b/.test(tokens[j].type)) continue;
                var ti = tokenIndex(tokens[j]);
                if (ti != ++k) if (!(k == 3 && ti == ++k)) break;
                if (k == 3) string = tokens[j].string;
                if (k == 4) { end = tokens[j].end; break; }
            }
            if (end === undefined) return null;
        }
        return { start: start, end: end, string: string };
    }

    function findMatchingBrackets(cm, where) {
        var marks = [], token = cm.getTokenAt(where);
        if (token.type == null && /^[()[\]{}]$/.test(token.string)) {
            var dir = /[([{]/.test(token.string) ? 1 : -1;
            var match = scanForMatching(cm, where, dir, "bracket", matchingRegex[token.string]);
            if (match && matching[token.string].charAt(0) != match.token.string) match = null;
            var style = match ? "CodeMirror-matchingbracket" : "CodeMirror-nonmatchingbracket";
            marks.push(cm.markText({ line: where.line, ch: token.start }, { line: where.line, ch: token.end }, { className: style }));
            if (match && cm.getLine(match.line).length <= maxHighlightLen)
                marks.push(cm.markText({ line: match.line, ch: match.token.start }, { line: match.line, ch: match.token.end }, { className: style }));
        } else if (/\bbegin|end\b/.test(token.type)) {
            var dir = /\bbegin\b/.test(token.type) ? 1 : -1;
            var range = getEnvironmentRange(cm, where);
            var match = range && scanForMatching(cm, { line: where.line, ch: range.end - 1 }, dir, "environment");
            var matchRange = match && getEnvironmentRange(cm, { line: match.line, ch: match.token.end });
            if (range) {
                if (match && (!matchRange || range.string != matchRange.string)) match = null;
                var style = match ? "CodeMirror-matchingtag" : "CodeMirror-nonmatchingtag";
                marks.push(cm.markText({ line: where.line, ch: range.start }, { line: where.line, ch: range.end }, { className: style }));
                if (match && cm.getLine(match.line).length <= maxHighlightLen)
                    marks.push(cm.markText({ line: match.line, ch: matchRange.start }, { line: match.line, ch: matchRange.end }, { className: style }));
            }
        }

        return marks;
    }

    function matchBrackets(cm) {
        var marks = [], ranges = cm.listSelections();
        for (var i = 0; i < ranges.length; i++) {
            if (!ranges[i].empty()) continue;
            var pos = ranges[i].head;
            var lineLen = cm.getLine(pos.line).length;
            if (lineLen > maxHighlightLen) continue;
            marks = marks.concat(findMatchingBrackets(cm, pos));
            if (pos.ch != lineLen)
                marks = marks.concat(findMatchingBrackets(cm, { line: pos.line, ch: pos.ch + 1 }));
        }

        if (marks.length) {
            return () => cm.operation(() => {
                for (var i = 0; i < marks.length; i++) marks[i].clear();
            });
        }
    }

    function doMatchBrackets(cm) {
        cm.operation(() => {
            if (cm.state.matchBrackets.currentlyHighlighted) {
                cm.state.matchBrackets.currentlyHighlighted();
                cm.state.matchBrackets.currentlyHighlighted = null;
            }
            cm.state.matchBrackets.currentlyHighlighted = matchBrackets(cm, false, cm.state.matchBrackets);
        });
    }
});