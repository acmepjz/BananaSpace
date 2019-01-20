using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Banana.Text
{
    public static class Preamble
    {
        public static readonly string[] Predefined =
        {
            "@@add",
            "@@adef",
            "@@aedef",
            "@@alet",
            "@@asdef",
            "@@bmk",
            "@@char",
            "@@def",
            "@@defa",
            "@@ea",
            "@@ex",
            "@@edef",
            "@@edefa",
            "@@error",
            "@@get",
            "@@ifzero",
            "@@item",
            "@@label",
            "@@let",
            "@@leta",
            "@@list",
            "@@ne",
            "@@par",
            "@@ref",
            "@@sdef",
            "@@sdefa",
            "@@set",
            "@@text"
        };

        public static readonly string[] MathJaxEnvironments =
        {
            "align",
            "align*",
            "alignat",
            "alignat*",
            "array",
            "Bmatrix",
            "bmatrix",
            "cases",
            "eqnarray",
            "eqnarray*",
            "gather",
            "gather*",
            "matrix",
            "multline",
            "multline*",
            "pmatrix",
            "smallmatrix",
            "split",
            "subarray",
            "Vmatrix",
            "vmatrix"
        };

        private static CommandDictionary _commandDefinitions = null;

        public static ExpansionData GetInitialExpansionData()
        {
            if (_commandDefinitions == null)
                Initialize();

            var data = new ExpansionData();
            foreach (var command in _commandDefinitions)
                data.Commands.Add(command);
            return data;
        }

        private static void Initialize()
        {
            var assembly = Assembly.GetEntryAssembly();
            var resourceStream = assembly.GetManifestResourceStream("Banana.Text.Preamble.txt");
            string preamble = null;
            using (var reader = new StreamReader(resourceStream))
                preamble = reader.ReadToEnd();

            foreach (var env in MathJaxEnvironments)
                preamble += $"\\adef\\begin\\@@bg {env}\\@@eg{{\\@ensuremath\\@u005cbegin{{{env}}}}}" +
                    $"\\adef\\end\\@@bg {env}\\@@eg{{\\@u005cend{{{env}}}}}";
            
            var tokens = Expression.Parse(preamble);
            var data = new ExpansionData();
            Expression.ExpandFinal(tokens, data);
            _commandDefinitions = data.Commands;
        }
    }
}
