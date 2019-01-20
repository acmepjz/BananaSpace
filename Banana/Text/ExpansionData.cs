using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Banana.Text
{
    public class ExpansionData
    {
        public const int MaxMacros = 100000;

        private int _expansionCount = 0;
        public int ExpansionCount
        {
            get => _expansionCount;
            set
            {
                _expansionCount = value;
                if (_expansionCount >= MaxMacros)
                    throw new TextException("Maximum times of macro expansion reached. Probably you have a dead loop.");
            }
        }

        public int BookmarkCount { get; set; } = 0;

        public CommandDictionary Commands { get; }
        public VariableDictionary Variables { get; }
        public List<Bookmark> Bookmarks { get; }

        public ExpansionData()
        {
            Commands = new CommandDictionary();
            Variables = new VariableDictionary();
            Bookmarks = new List<Bookmark>();
        }

        private ExpansionData(CommandDictionary commands, VariableDictionary variables, List<Bookmark> bookmarks)
        {
            Commands = commands;
            Variables = variables;
            Bookmarks = bookmarks;
        }

        public ExpansionData PassToSubgroup()
        {
            return new ExpansionData(Commands, Variables.Clone(), Bookmarks)
            {
                _expansionCount = ExpansionCount,
                BookmarkCount = BookmarkCount
            };
        }

        public void UpdateFromSubgroup(ExpansionData data)
        {
            ExpansionCount = data.ExpansionCount;
            BookmarkCount = data.BookmarkCount;

            // make deletion of variables global
            List<string> toErase = new List<string>();
            foreach (var kvp in Variables)
                if (data.Variables[kvp.Key] == null)
                    toErase.Add(kvp.Key);
            foreach (string key in toErase)
                Variables.Remove(key);

            // make global variables global
            foreach (var kvp in data.Variables)
                if (kvp.Key.StartsWith("g."))
                    Variables[kvp.Key] = kvp.Value;
        }

        public bool GetBool(string key, bool @default, bool erase)
            => Variables.GetBool(key, @default, erase);

        public int GetInt(string key, int @default, bool erase)
            => Variables.GetInt(key, @default, erase);

        public double GetDouble(string key, double @default, bool erase)
            => Variables.GetDouble(key, @default, erase);

        public string GetString(string key, string @default, bool erase)
            => Variables.GetString(key, @default, erase);
    }

    public class CommandDictionary : IEnumerable<CommandDefinition>
    {
        private Dictionary<string, CommandDefinition> _dict = new Dictionary<string, CommandDefinition>();

        public CommandDefinition this[string commandName]
        {
            get
            {
                if (_dict.TryGetValue(commandName, out var cd))
                    return cd;
                return null;
            }
            set
            {
                if (_dict.ContainsKey(commandName))
                    _dict.Remove(commandName);
                if (value != null)
                    _dict.Add(commandName, value);
            }
        }

        public CommandDictionary Clone()
        {
            var cloned = new CommandDictionary();
            foreach (var kvp in _dict)
                cloned._dict.Add(kvp.Key, kvp.Value);
            return cloned;
        }

        public void Add(CommandDefinition command)
        {
            _dict.Add(command.Name, command);
        }

        public IEnumerator<CommandDefinition> GetEnumerator()
            => _dict.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    public class VariableDictionary : IEnumerable<KeyValuePair<string, string>>
    {
        private Dictionary<string, string> _dict = new Dictionary<string, string>();

        public string this[string key]
        {
            get
            {
                if (_dict.TryGetValue(key, out string s))
                    return s;
                return null;
            }
            set
            {
                if (_dict.ContainsKey(key))
                    _dict.Remove(key);
                if (value != null)
                    _dict.Add(key, value);
            }
        }

        public VariableDictionary Clone()
        {
            var cloned = new VariableDictionary();
            foreach (var kvp in _dict)
                cloned._dict.Add(kvp.Key, kvp.Value);
            return cloned;
        }

        public void Remove(string key)
        {
            if (_dict.ContainsKey(key))
                _dict.Remove(key);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            => _dict.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public bool GetBool(string key, bool @default, bool erase)
        {
            if (_dict.TryGetValue(key, out string s))
            {
                if (erase) _dict.Remove(key);
                if (s == "1") return true;
                if (s == "-1") return false;
            }
            return @default;
        }

        public int GetInt(string key, int @default, bool erase)
        {
            if (_dict.TryGetValue(key, out string s))
            {
                if (erase) _dict.Remove(key);
                if (int.TryParse(s, out int i))
                    return i;
            }
            return @default;
        }

        public double GetDouble(string key, double @default, bool erase)
        {
            if (_dict.TryGetValue(key, out string s))
            {
                if (erase) _dict.Remove(key);
                return ParseLength(s, 18, @default);
            }
            return @default;
        }

        public string GetString(string key, string @default, bool erase)
        {
            if (_dict.TryGetValue(key, out string s))
            {
                if (erase) _dict.Remove(key);
                return s;
            }
            return @default;
        }

        static double ParseLength(string s, double em, double @default)
        {
            // TODO. COMPLETE THE LIST
            if (double.TryParse(s, out double d))
                return d;
            if (s.EndsWith("em"))
            {
                s = s.Substring(0, s.Length - 2);
                if (double.TryParse(s, out d))
                    return d * em;
            }
            else if (s.EndsWith("ex"))
            {
                s = s.Substring(0, s.Length - 2);
                if (double.TryParse(s, out d))
                    return .5 * d * em;
            }
            else if (s.EndsWith("mu"))
            {
                s = s.Substring(0, s.Length - 2);
                if (double.TryParse(s, out d))
                    return d / 18 * em;
            }
            else if (s.EndsWith("pt"))
            {
                s = s.Substring(0, s.Length - 2);
                if (double.TryParse(s, out d))
                    return d;
            }

            return @default;
        }
    }
    
    public class Bookmark
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public List<Token> Content { get; set; }
    }
}
