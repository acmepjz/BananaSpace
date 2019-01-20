using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Banana.Text
{
    public class CommandDefinition
    {
        public string Name { get; set; }
        public List<List<Token>> Patterns { get; set; }
        public List<List<Token>> Definitions { get; set; }

        public bool IsTextMode { get; set; }
        public bool IsSpecial { get; set; }
        public Token SpecialToken { get; set; }

        public CommandDefinition(string name)
        {
            Name = name;
            Patterns = new List<List<Token>>();
            Definitions = new List<List<Token>>();
        }
    }
}
