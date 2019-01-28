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
    var environments = ["definition","theorem"];

    CodeMirror.defineMode("tex", function () {
        function token(stream, state) {
            if (state.readingText) {
                var type = state.textType;
                state.textType = null;
                state.readingText = false;
                if (stream.match(/^[^~#$%^&_{}\\\s]+/)) {
                    if (stream.match(/(?=})/))
                        return type;
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
                return "command";
            }
            if (stream.eat("%")) {
                stream.skipToEnd();
                return "comment";
            }
            if (stream.eat("{")) {
                if (state.textType) 
                    state.readingText = true;
                return null;
            }
            if (stream.eat("}")) {
                state.textType = null;
                return null;
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
    function hintHelper(instance, options) {
        if (!instance.allCommands)
            instance.allCommands = commands;
        if (!instance.allEnvironments)
            instance.allEnvironments = environments;

        if (instance.state.hintMode == "command") {
            var pos = { line: instance.state.hintFrom.line, ch: instance.state.hintFrom.ch + 1 };
            var token = instance.getTokenAt(pos, true);
            if (token.start != instance.state.hintFrom.ch)
                return null;

            var text = token.string.substring(0, instance.getCursor().ch - instance.state.hintFrom.ch);
            var matches = [];

            var exact = -1;
            var recent = -1, recentIndex = -1;
            instance.allCommands.forEach(s => {
                if (s.toLowerCase().startsWith(text.toLowerCase())) {
                    var len = matches.push(s);
                    if (s == text)
                        exact = len - 1;
                    var index = instance.state.recentCommands.indexOf(s);
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
            instance.state.pickHintOnClose = text.length >= 2;
            instance.state.selectedHint = selectedIndex;

            if (matches.length == 0)
                instance.state.hintMode = null; // prevents competion from reopening when user hits backspace

            return {
                list: matches,
                from: instance.state.hintFrom,
                to: instance.getCursor()
                // 'selectedHint: selectedIndex' should be here,
                // but the selected item would not be scrolled into view, so we do this on "shown"
            };
        }
        else if (instance.state.hintMode == "environment") {
            var pos = instance.getCursor();
            var token = instance.getTokenAt(pos, true);
            if (token.string != "{" &&
                ((token.type != "environment begin" && token.type != "wrong-environment begin") ||
                    token.start != instance.state.hintFrom.ch))
                return null;

            var text = token.string == "{" ? "" : token.string.substring(0, instance.getCursor().ch - instance.state.hintFrom.ch);
            var matches = [];

            var exact = -1;
            var recent = -1, recentIndex = -1;
            instance.allEnvironments.forEach(s => {
                if (s.toLowerCase().startsWith(text.toLowerCase())) {
                    var len = matches.push(s);
                    if (s == text)
                        exact = len - 1;
                    var index = instance.state.recentEnvironments.indexOf(s);
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
            instance.state.pickHintOnClose = true;
            instance.state.selectedHint = selectedIndex;

            if (matches.length == 0)
                instance.state.hintMode = null;

            return {
                list: matches,
                from: instance.state.hintFrom,
                to: instance.getCursor()
            };
        }
    }

    CodeMirror.registerHelper("hint", "tex", (instance, options) => {
        var result = hintHelper(instance, options);
        // subscribe to events
        if (result) {
            CodeMirror.on(result, "shown", () => {
                var completion = instance.state.completionActive;
                if (completion && completion.widget)
                    completion.widget.changeActive(instance.state.selectedHint || 0, false);
            });

            if (instance.state.hintMode == "command") {
                CodeMirror.on(result, "pick", item => { // item: string
                    instance.state.recentCommands = instance.state.recentCommands.filter(s => s != item);
                    instance.state.recentCommands.unshift(item);
                    while (instance.state.recentCommands.length > 100)
                        instance.state.recentCommands.pop();
                });
            } else if (instance.state.hintMode == "environment") {
                CodeMirror.on(result, "pick", item => {
                    instance.state.recentEnvironments = instance.state.recentEnvironments.filter(s => s != item);
                    instance.state.recentEnvironments.unshift(item);
                    while (instance.state.recentEnvironments.length > 100)
                        instance.state.recentEnvironments.pop();

                    var pos = instance.getCursor();
                    instance.replaceRange("}", pos);
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

    function doUpdateTokens(instance) {
        var allCommands = [], allEnvironments = [];
        commands.forEach(s => allCommands.push(s));
        environments.forEach(s => allEnvironments.push(s));

        var pos = instance.getCursor();
        for (var i = 0; i < instance.lineCount(); i++) {
            instance.getLineTokens(i).forEach(token => {
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
        instance.allCommands = allCommands;
        instance.allEnvironments = allEnvironments;
    }

    async function updateTokens(instance) {
        instance.needsUpdatingTokens = true;
        if (instance.workerStarted) 
            return;

        instance.workerStarted = true;
        while (true) {
            if (instance.needsUpdatingTokens)
                doUpdateTokens(instance);
            instance.needsUpdatingTokens = false;
            await wait(1000);
        }
    }

    function updateTokensAsync(instance) {
        setTimeout(() => updateTokens(instance), 0);
    }

    CodeMirror.defineExtension("init", instance => {
        updateTokensAsync(instance);
        instance.state.recentCommands = [];
        instance.state.recentEnvironments = [];
        instance.on("endCompletion", instance => {
            if (!instance.state.hintUpdating)
                instance.state.hintMode = null; // prevents completion from showing when e.g. user hits esc and continues typing
        });
        instance.on("change", (instance, change) => {
            var pos = instance.getCursor();
            if (instance.state.nextHintMode) {
                // when user types '\xxx' and hits '\', a command is picked and a change is fired, 
                // causing the new completion widget to disappear. this fixes it.
                instance.state.hintMode = instance.state.nextHintMode;
                if (instance.state.hintMode == "command")
                    instance.state.hintFrom = { line: pos.line, ch: pos.ch - 1 };
                else
                    instance.state.hintFrom = pos;
                instance.state.hintUpdating = true;
                instance.state.nextHintMode = null;
                return;
            }
            instance.state.nextHintMode = null;
            if (change.text == "\\") {
                // completion for commands
                var completion = instance.state.completionActive;
                if (completion && completion.widget && instance.state.pickHintOnClose) {
                    completion.widget.pick();
                    instance.state.nextHintMode = "command";
                }
                instance.state.hintFrom = change.from;
                instance.state.hintMode = "command";
                instance.state.hintUpdating = true;
            } else if ((change.text.length == 1 &&
                change.text[0].match(/^[^0-9`~!#$%^&*()\-_=+[\]{}\\|;:'",.<>\/?\s]$/)) ||
                (change.text[0].length == 0 &&
                    change.removed.length == 1 &&
                    change.removed[0].match(/^[^\\]$/))) {
                instance.state.hintUpdating = true;
                // keep hintMode unchanged
            } else {
                var picked = false;
                instance.state.hintMode = null;
                var completion = instance.state.completionActive;
                if (completion && completion.widget && instance.state.pickHintOnClose) {
                    completion.widget.pick();
                    picked = true;
                }

                // completion for environments
                if (change.text == "{" && instance.getTokenAt({ line: pos.line, ch: pos.ch - 1 }).string == "\\begin") {
                    instance.state.hintFrom = pos;
                    instance.state.hintMode = "environment";
                    if (picked) instance.state.nextHintMode = "environment";
                    instance.state.hintUpdating = true;
                }
            }
            updateTokensAsync(instance);
            window.onbeforeunload = () => true;
        });
        instance.on("changes", function (instance) {
            if (instance.state.hintMode)
                instance.showHint();
            else if (!instance.state.hintUpdating)
                instance.closeHint();
            instance.state.hintUpdating = false;
        });
    });
});