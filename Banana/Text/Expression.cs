using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Banana.Text
{
    public static class Expression
    {
        public const string SpecialChars = "0123456789`~!#$%^&*()-_=+[]{}\\|;:'\",.<>/?";
        //ActiveChars = "~&",
        //SymbolChars = "+-=|/<>*\u00ac\u00b7\u00d7\u2016\u2020\u2021\u2190\u2191\u2192\u2193\u2194\u2195\u2196\u2197\u2198\u2199\u21d0\u21d1\u21d2\u21d3\u21d4\u21d5\u2205\u2208\u220a\u220b\u220d\u2218\u2219\u221d\u221e\u2223\u2224\u2225\u2226\u2227\u2228\u2229\u222a\u223c\u223d\u224e\u224f\u2250\u2251\u2252\u2253\u2254\u2255\u2256\u2261\u2263\u2276\u2277\u227a\u227b\u227c\u227d\u2282\u2283\u228d\u228e\u228f\u2290\u2293\u2294\u2295\u2296\u2297\u2298\u2299\u229a\u229b\u229c\u229d\u229e\u229f\u22a0\u22a1\u22a2\u22a3\u22a4\u22a5\u22b0\u22b1\u22b2\u22b3\u22b4\u22b5\u22be\u22bf\u22c4\u22c6\u22c8\u22c9\u22ca\u22cb\u22cc\u22ce\u22cf\u22d0\u22d1\u2322\u2323\u2a7d\u2a7e";

        const int MaxBuffer = 1000000;
        const string DefaultColor = "000000";
        const double DefaultFontSize = 18;

        public static List<Token> Parse(string s)
        {
            List<Token> tokens = new List<Token>();
            s = s.Replace("\r\n", "\n").Replace("\r", "\n");
            s += " ";
            string t = "";
            bool ignoreNextWhitespace = false;

            for (int i = 0; i < s.Length - 1; i++)
            {
                if (char.IsWhiteSpace(s[i]))
                {
                    if (!ignoreNextWhitespace || s[i] == '\n')
                        tokens.Add(new Token(s[i].ToString(), TokenType.Whitespace));
                    continue;
                }

                ignoreNextWhitespace = false;
                switch (s[i])
                {
                    case '{':
                        tokens.Add(new Token(null, TokenType.BeginGroup));
                        break;
                    case '}':
                        tokens.Add(new Token(null, TokenType.EndGroup));
                        break;
                    case '^':
                        tokens.Add(new Token(null, TokenType.Superscript));
                        ignoreNextWhitespace = true;
                        break;
                    case '_':
                        tokens.Add(new Token(null, TokenType.Subscript));
                        ignoreNextWhitespace = true;
                        break;
                    case '&':
                        tokens.Add(new Token(null, TokenType.Tab));
                        break;
                    case '%':
                        while (i < s.Length - 1 && s[i] != '\n') i++;
                        ignoreNextWhitespace = true;
                        break;
                    case '~':
                        tokens.Add(new Token(null, TokenType.Tilde));
                        break;
                    case '$':
                        if (s[i + 1] == '$')
                        {
                            tokens.Add(new Token("$$", TokenType.MathDelim));
                            i++;
                            break;
                        }
                        tokens.Add(new Token("$", TokenType.MathDelim));
                        break;
                    case '\\':
                        i++;
                        if (s[i] == '\\')
                        {
                            tokens.Add(new Token(null, TokenType.LineBreak));
                            break;
                        }
                        if (s[i] == '/')
                        {
                            tokens.Add(new Token("\\", TokenType.Text));
                            break;
                        }
                        if ("()[]".Contains(s[i]))
                        {
                            tokens.Add(new Token("\\" + s[i], TokenType.MathDelim));
                            break;
                        }

                        if (char.IsWhiteSpace(s[i]) || SpecialChars.Contains(s[i]))
                            tokens.Add(new Token(s[i].ToString(), TokenType.Command));
                        else
                        {
                            t = "";
                            while (!(char.IsWhiteSpace(s[i]) || SpecialChars.Contains(s[i])))
                                t += s[i++];
                            i--;
                            tokens.Add(new Token(t, TokenType.Command));
                            ignoreNextWhitespace = true;
                        }
                        break;
                    case '#':
                        t = "";
                        while (s[i] == '#')
                            t += s[i++];
                        t += s[i];
                        tokens.Add(new Token(t, TokenType.Argument));
                        break;
                    default:
                        if (char.IsSurrogatePair(s, i))
                            tokens.Add(new Token(s.Substring(i, 2), TokenType.Text));
                        else
                            tokens.Add(new Token(s[i].ToString(), TokenType.Text));
                        break;
                }
            }

            return tokens;
        }

        private static int GetGroupEnd(List<Token> tokens, int start)
        {
            int nest = 0, i;
            for (i = start; i < tokens.Count; i++)
            {
                if (tokens[i].Type == TokenType.BeginGroup) nest++;
                if (tokens[i].Type == TokenType.EndGroup) nest--;
                if (nest == 0) break;
            }
            if (nest > 0) throw new Exception("There are unmatched {'s.");
            return i;
        }

        public static void ExpandSpecials(List<Token> tokens, ExpansionData data)
        {
            var specialDefs = new Dictionary<Token, CommandDefinition>();
            foreach (var command in data.Commands)
                if (command.IsSpecial)
                    specialDefs.Add(command.SpecialToken, command);

            for (int i = 0; i < tokens.Count; i++)
            {
                if (specialDefs.TryGetValue(tokens[i], out var command))
                {
                    var token = tokens[i];
                    MatchAndReplace(tokens, ref i, command, data);
                    data.ExpansionCount++;
                }
            }
        }

        public static List<Token> ExpandFinal(List<Token> tokens, ExpansionData data)
        {
            var result = new List<Token>();
            result.Add(new Token("<div class=\"paragraph\">", TokenType.HtmlTag));
            result.Add(new Token("<span>", TokenType.HtmlTag));

            result.AddRange(Expand(tokens, data));

            result.Add(new Token("</span>", TokenType.HtmlTag));
            result.Add(new Token("</div>", TokenType.HtmlTag));

            // remove whitespaces, empty spans and ps
            bool lastTokenIsWhitespace = false;
            for (int i = 0; i < result.Count; i++)
            {
                var token = result[i];
                if (!token.VerbatimOutput &&
                    (token.Type == TokenType.BeginGroup ||
                    token.Type == TokenType.EndGroup))
                {
                    result.RemoveAt(i--);
                    continue;
                }
                if (lastTokenIsWhitespace && token.Type == TokenType.Whitespace)
                {
                    result.RemoveAt(i--);
                    continue;
                }
                if (token.Type == TokenType.Whitespace)
                {
                    token.Text = " ";
                    lastTokenIsWhitespace = true;
                }
                else
                    lastTokenIsWhitespace = false;
            }

            for (int i = 0; i < result.Count; i++)
            {
                var token = result[i];
                if (token.Type == TokenType.HtmlTag)
                {
                    if ((token.Text == "<span>" || token.Text.StartsWith("<span ")) &&
                        result[i + 1].Type == TokenType.HtmlTag &&
                        result[i + 1].Text == "</span>")
                    {
                        result.RemoveRange(i, 2);
                        i -= 2;  // as in <div><span></span></div>
                        if (i < -1) i = -1;
                        continue;
                    }
                    if ((token.Text == "<div>" || token.Text.StartsWith("<div ")) &&
                        result[i + 1].Type == TokenType.HtmlTag &&
                        result[i + 1].Text == "</div>")
                    {
                        result.RemoveRange(i, 2);
                        i -= 2;
                        if (i < -1) i = -1;
                        continue;
                    }
                }
            }

            return result;
        }

        // to get valid html, write <div><span> { result } </span></div>
        // result may contain empty spans and ps, which need to be removed
        private static List<Token> Expand(List<Token> tokens, ExpansionData data)
        {
            if (tokens.Count == 0) return new List<Token>();

            Token t;
            var result = new List<Token>();
            var expandAfterPositions = new Stack<int>();
            CommandDefinition command, command2;
            string name;
            List<Token> patTokens, defTokens;

            bool isMathMode = data.GetBool("math-mode", false, false);

            for (int i = 0; i < tokens.Count; i++)
            {
                t = tokens[i];
                int _i = i;

                switch (t.Type)
                {
                    case TokenType.BeginGroup:
                        if (expandAfterPositions.Count > 0)
                            break;

                        int groupEnd = GetGroupEnd(tokens, i);

                        var cloned = data.PassToSubgroup();
                        result.Add(new Token(null, TokenType.BeginGroup));
                        result.AddRange(Expand(
                            tokens.GetRange(i + 1, groupEnd - i - 1),
                            cloned));
                        data.UpdateFromSubgroup(cloned);

                        if (cloned.GetBool("style-changed", false, false))
                        {
                            result.Add(new Token("</span>", TokenType.HtmlTag));
                            result.Add(GetSpanTag(data));
                        }
                        result.Add(new Token(null, TokenType.EndGroup));

                        i = groupEnd;
                        break;

                    case TokenType.EndGroup:
                        if (expandAfterPositions.Count > 0)
                            break;
                        throw new Exception("Too many }'s.");

                    case TokenType.Command:
                        string varName, varValue;
                        switch (t.Text)
                        {
                            case "@@ea":
                                if (i + 2 < tokens.Count)
                                {
                                    tokens.RemoveAt(i);
                                    expandAfterPositions.Push(i);
                                    continue;
                                }
                                else
                                    throw new Exception("'\\@@ea' must be followed by two tokens.");

                            case "@@ne":
                                if (i + 1 < tokens.Count)
                                {
                                    tokens.RemoveAt(i);
                                    i--;
                                    goto iterend;
                                }
                                else
                                    throw new Exception("'\\@@ne' must be followed by a token.");

                            case "@@csname":
                                string csName = ReadTextArgument(tokens, ref i, data, "@@csname", true);
                                tokens.RemoveRange(_i, i - _i + 1);
                                tokens.Insert(_i, new Token(csName, TokenType.Command));
                                i = _i - 1;
                                goto iterend;

                            #region \@@def etc
                            case "@@def":
                            case "@@defa":
                            case "@@adef":
                            case "@@edef":
                            case "@@edefa":
                            case "@@aedef":
                                i++;
                                if (i == tokens.Count || tokens[i].Type != TokenType.Command)
                                    throw new Exception($"'\\@@def' must be followed by a command, but it is followed by '{tokens[i]}'.");
                                name = tokens[i].Text;
                                if (Preamble.Predefined.Contains(name))
                                    throw new Exception($"'\\{name}' is not a valid command name.");

                                patTokens = new List<Token>();
                                for (i++; i < tokens.Count && tokens[i].Type != TokenType.BeginGroup; i++)
                                {
                                    if (tokens[i].Type == TokenType.Command && tokens[i].Text == "@@ne"
                                        && i + 1 < tokens.Count && tokens[i + 1].Type == TokenType.Command)
                                    {
                                        i++;
                                        patTokens.Add(tokens[i]);
                                    }
                                    else if (tokens[i].Type == TokenType.Command && tokens[i].Text == "@@ex"
                                        && i + 1 < tokens.Count && tokens[i + 1].Type == TokenType.Command)
                                    {
                                        var token = tokens[i + 1];
                                        command = data.Commands[token.Text];
                                        if (command != null)
                                        {
                                            int __i = i + 1;
                                            if (!MatchAndReplace(tokens, ref __i, command, data))
                                                throw new Exception("\\@@ex: can't expand.");
                                        }
                                        else if (token.Text == "@@csname")
                                        {
                                            int __i = i + 1;
                                            csName = ReadTextArgument(tokens, ref __i, data, "@@csname", true);
                                            tokens.RemoveRange(i, __i - i);
                                            tokens.Insert(i, new Token(csName, TokenType.Command));
                                        }
                                        else if (Preamble.Predefined.Contains(token.Text))
                                        {
                                            throw new Exception("'\\@@ex' cannot be followed by a primitive command other than '\\@@csname'.");
                                        }
                                    }
                                    else
                                        patTokens.Add(ReplaceBgEg(tokens[i]));
                                }

                                if (i == tokens.Count) throw new Exception("\\@@def: { expected.");
                                groupEnd = GetGroupEnd(tokens, i);
                                defTokens = new List<Token>();
                                var groupTokens = tokens.GetRange(i + 1, groupEnd - i - 1);
                                for (i = 0; i < groupTokens.Count; i++)
                                {
                                    if (groupTokens[i].Type == TokenType.Command && groupTokens[i].Text == "@@ne"
                                        && i + 1 < groupEnd && groupTokens[i + 1].Type == TokenType.Command)
                                    {
                                        i++;
                                        defTokens.Add(groupTokens[i]);
                                    }
                                    else if (groupTokens[i].Type == TokenType.Command && groupTokens[i].Text == "@@ex"
                                        && i + 1 < groupEnd && groupTokens[i + 1].Type == TokenType.Command)
                                    {
                                        var token = groupTokens[i + 1];
                                        command = data.Commands[token.Text];
                                        if (command != null)
                                        {
                                            int __i = i + 1;
                                            if (!MatchAndReplace(groupTokens, ref __i, command, data))
                                                throw new Exception("\\@@ex: can't expand.");
                                        }
                                        else if (token.Text == "@@csname")
                                        {
                                            int __i = i + 1;
                                            csName = ReadTextArgument(groupTokens, ref __i, data, "@@csname", true);
                                            groupTokens.RemoveRange(i, __i - i);
                                            groupTokens.Insert(i, new Token(csName, TokenType.Command));
                                        }
                                        else if (Preamble.Predefined.Contains(token.Text))
                                        {
                                            throw new Exception("'\\@@ex' cannot be followed by a primitive command other than '\\@@csname'.");
                                        }
                                    }
                                    else
                                        defTokens.Add(ReplaceBgEg(groupTokens[i]));
                                }

                                if (t.Text == "@@edef" || t.Text == "@@edefa" || t.Text == "@@aedef")
                                    defTokens = Expand(defTokens, data);

                                command = data.Commands[name];
                                if (command == null || t.Text == "@@def" || t.Text == "@@edef")
                                {
                                    command = new CommandDefinition(name)
                                    {
                                        IsTextMode = data.GetBool("text-def", false, true)
                                    };
                                    data.Commands[name] = command;
                                }
                                if (t.Text == "@@adef" || t.Text == "@@aedef")
                                {
                                    command.Patterns.Insert(0, patTokens);
                                    command.Definitions.Insert(0, defTokens);
                                }
                                else
                                {
                                    command.Patterns.Add(patTokens);
                                    command.Definitions.Add(defTokens);
                                }

                                tokens.RemoveRange(_i, groupEnd - _i + 1);
                                i = _i - 1;
                                goto iterend;

                            case "@@sdef":
                            case "@@asdef":
                            case "@@sdefa":
                                i++;
                                if (i == tokens.Count || tokens[i].Type == TokenType.Argument)
                                    throw new Exception("\\@@sdef: Token expected.");
                                Token tt = tokens[i];
                                if (tt.Type == TokenType.Command && Preamble.Predefined.Contains(tt.Text))
                                    throw new Exception($"'\\{tt.Text}' is not a valid command name.");

                                i++;
                                patTokens = new List<Token>();
                                while (i < tokens.Count && tokens[i].Type != TokenType.BeginGroup)
                                {
                                    if (tokens[i].Type == TokenType.Command && tokens[i].Text == "@@ne"
                                        && i + 1 < tokens.Count && tokens[i + 1].Type == TokenType.Command)
                                    {
                                        i++;
                                        patTokens.Add(tokens[i++]);
                                    }
                                    else
                                        patTokens.Add(ReplaceBgEg(tokens[i++]));
                                }

                                if (i == tokens.Count) throw new Exception("\\@@sdef: { expected.");
                                groupEnd = GetGroupEnd(tokens, i);
                                defTokens = new List<Token>();
                                for (i++; i < groupEnd; i++)
                                {
                                    if (tokens[i].Type == TokenType.Command && tokens[i].Text == "@@ne"
                                        && i + 1 < groupEnd && tokens[i + 1].Type == TokenType.Command)
                                    {
                                        i++;
                                        defTokens.Add(tokens[i]);
                                    }
                                    else
                                        defTokens.Add(ReplaceBgEg(tokens[i]));
                                }

                                string key =
                                    tt.Type == TokenType.Command ? "\\" + tt.Text :
                                    tt.Type == TokenType.Text ? tt.Text :
                                    tt.Type.ToString();
                                key = "{" + key + "}";
                                command = data.Commands[key];
                                if (command == null || t.Text == "@@sdef")
                                {
                                    command = new CommandDefinition(tt.Type == TokenType.Command ? tt.Text : key)
                                    {
                                        IsSpecial = true,
                                        SpecialToken = tt
                                    };
                                    data.Commands[command.Name] = command;
                                }
                                if (t.Text == "@@asdef")
                                {
                                    command.Patterns.Insert(0, patTokens);
                                    command.Definitions.Insert(0, defTokens);
                                }
                                else
                                {
                                    command.Patterns.Add(patTokens);
                                    command.Definitions.Add(defTokens);
                                }
                                
                                tokens.RemoveRange(_i, groupEnd - _i + 1);
                                i = _i - 1;
                                goto iterend;

                            case "@@let":
                            case "@@alet":
                            case "@@leta":
                                i++;
                                if (!(i + 1 < tokens.Count && tokens[i].Type == TokenType.Command && tokens[i + 1].Type == TokenType.Command))
                                    throw new Exception("'\\@@let' must be followed by two commands.");
                                name = tokens[i].Text;
                                if (Preamble.Predefined.Contains(name))
                                    throw new Exception($"'\\{name}' is not a valid command name.");
                                string name2 = tokens[i + 1].Text;

                                command = data.Commands[name];
                                if (Preamble.Predefined.Contains(name2))
                                {
                                    command2 = new CommandDefinition(name2)
                                    {
                                        Patterns = { new List<Token>() },
                                        Definitions = { new List<Token> { new Token(name2, TokenType.Command) } }
                                    };
                                }
                                else
                                    command2 = data.Commands[name2];
                                if (command == null || t.Text == "@@let")
                                {
                                    command = new CommandDefinition(name);
                                    data.Commands[name] = command;
                                }
                                if (command2 == null) break;

                                if (t.Text == "@@alet")
                                {
                                    for (int ii = 0; ii < command2.Definitions.Count; ii++)
                                    {
                                        command.Patterns.Insert(ii, command2.Patterns[ii]);
                                        command.Definitions.Insert(ii, command2.Definitions[ii]);
                                    }
                                }
                                else
                                {
                                    for (int ii = 0; ii < command2.Definitions.Count; ii++)
                                    {
                                        command.Patterns.Add(command2.Patterns[ii]);
                                        command.Definitions.Add(command2.Definitions[ii]);
                                    }
                                }

                                tokens.RemoveRange(_i, 3);
                                i = _i - 1;
                                goto iterend;
                            #endregion

                            case "@@text":
                                Token tokText = new Token(ReadTextArgument(tokens, ref i, data, "@@text", false), TokenType.Text);
                                tokens.RemoveRange(_i, i - _i + 1);
                                tokens.Insert(_i, tokText);
                                i = _i - 1;
                                goto iterend;

                            case "@@char":
                                if (int.TryParse(ReadTextArgument(tokens, ref i, data, "@@char", false),
                                    System.Globalization.NumberStyles.HexNumber,
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    out int theChar))
                                {
                                    Token tokChar;
                                    if (theChar >= 0 && theChar <= 0xffff)
                                        tokChar = new Token(((char)theChar).ToString(), TokenType.Text);
                                    else if (theChar >= 0x10000 && theChar <= 0x10ffff)
                                        tokChar = new Token("" + ((char)(0xd800 + (theChar - 0x10000) / 0x400)) + ((char)(0xdc00 + theChar % 0x400)), TokenType.Text);
                                    else throw new Exception("\\@@char: Invalid code.");

                                    tokens.RemoveRange(_i, i - _i + 1);
                                    tokens.Insert(_i, tokChar);
                                    i = _i - 1;
                                }
                                else throw new Exception("\\@@char: Invalid code.");
                                goto iterend;

                            case "@@get":
                                int getOption = data.GetInt("get-format", 0, true);
                                varName = ReadTextArgument(tokens, ref i, data, "@@get", true).Trim();
                                string value = data.Variables[varName];
                                if (value != null)
                                {
                                    if (int.TryParse(value, out int number))
                                    {
                                        string output = "";
                                        switch (getOption)
                                        {
                                            case 1: // alph
                                            case 2: // Alph
                                                int abs = number > 0 ? number : -number;
                                                if (abs == 0)
                                                {
                                                    output = "0";
                                                    break;
                                                }
                                                long p = 1;
                                                for (int q = 1; q < 8; q++)
                                                {
                                                    abs -= (int)p;
                                                    p *= 26;
                                                    if (p <= abs) continue;

                                                    while (p > 1)
                                                    {
                                                        p /= 26;
                                                        output += (char)('a' + (abs / p));
                                                        abs %= (int)p;
                                                    }
                                                    break;
                                                }

                                                if (getOption == 2)
                                                    output = output.ToUpper();
                                                if (number < 0)
                                                    output = "-" + output;
                                                break;

                                            case 3: // roman
                                            case 4: // Roman
                                                output = ToRoman(number);
                                                if (getOption == 4)
                                                    output = output.ToUpper();
                                                break;

                                            default:
                                                output = number.ToString();
                                                break;
                                        }

                                        foreach (char c in output)
                                            result.Add(new Token(c.ToString(), TokenType.Text));
                                    }
                                    else
                                    {
                                        foreach (char c in value)
                                            result.Add(new Token(c.ToString(), TokenType.Text));
                                    }
                                }
                                else
                                {
                                    result.Add(new Token("??", TokenType.Text));
                                }
                                tokens.RemoveRange(_i, i - _i + 1);
                                i = _i - 1;

                                continue;

                            case "@@set":
                                varName = ReadTextArgument(tokens, ref i, data, "@@set", true).Trim();
                                if (varName.Contains(',')) throw new Exception("\\@@set: name must not contain ','.");
                                varValue = ReadTextArgument(tokens, ref i, data, "@@set", true);
                                data.Variables[varName] = varValue;
                                tokens.RemoveRange(_i, i - _i + 1);
                                i = _i - 1;

                                if (StyleVariables.Contains(varName))
                                {
                                    data.Variables["style-changed"] = "1";

                                    if (!isMathMode)
                                    {
                                        result.Add(new Token("</span>", TokenType.HtmlTag));
                                        result.Add(GetSpanTag(data));
                                    }
                                }

                                continue;

                            case "@@add":
                                varName = ReadTextArgument(tokens, ref i, data, "@@add", true).Trim();
                                if (varName.Contains(',')) throw new Exception("\\@@add: name must not contain ','.");
                                varValue = ReadTextArgument(tokens, ref i, data, "@@add", true);
                                int intValue = data.GetInt(varName, 0, false),
                                    addValue = 0;
                                varValue = varValue.Replace("--", ""); // as in {-{key}}
                                int.TryParse(varValue, out addValue);
                                data.Variables[varName] = (intValue + addValue).ToString();
                                tokens.RemoveRange(_i, i - _i + 1);
                                i = _i - 1;
                                continue;

                            case "@@ifzero":
                                IfZero(tokens, ref i, data);
                                data.ExpansionCount++;
                                continue;

                            case "@@par":
                                result.Add(new Token("</span>", TokenType.HtmlTag));
                                result.Add(new Token("</div>", TokenType.HtmlTag));
                                result.Add(GetPTag(data));
                                result.Add(GetSpanTag(data));
                                continue;

                            case "@@list":
                                var content = ReadArgument(tokens, ref i, "@@list");
                                cloned = data.PassToSubgroup();
                                cloned.Variables["is-first-item"] = "1";
                                content = Expand(content, cloned);
                                data.UpdateFromSubgroup(cloned);

                                // structure:
                                // <div class="paragraph">
                                //   <div class="enum-item"> ... </div>
                                //   ...
                                // </div>

                                result.Add(new Token("</span>", TokenType.HtmlTag));
                                result.Add(new Token("</div>", TokenType.HtmlTag));
                                result.Add(GetPTag(data));
                                result.AddRange(content);
                                result.Add(new Token("</span>", TokenType.HtmlTag));
                                result.Add(new Token("</div>", TokenType.HtmlTag));
                                result.Add(new Token("</div>", TokenType.HtmlTag));
                                result.Add(new Token("</div>", TokenType.HtmlTag));
                                result.Add(new Token("</div>", TokenType.HtmlTag));
                                result.Add(GetPTag(data));
                                result.Add(GetSpanTag(data));
                                continue;

                            case "@@item":
                                // structure:
                                // <div class="enum-item">
                                //   <div class="enum-item-label">
                                //     <div class="paragraph"><span> ... </span></div>
                                //   </div>
                                //   <div class="enum-item-body">
                                //     <div class="paragraph"><span> ... [here is \@@item] </span></div>
                                //   </div>
                                // </div>

                                bool isFirstItem = data.GetBool("is-first-item", false, true);

                                content = Parse("\\@itemlabel");
                                cloned = data.PassToSubgroup();
                                content = Expand(content, cloned);
                                data.UpdateFromSubgroup(cloned);

                                if (!isFirstItem)
                                {
                                    result.Add(new Token("</span>", TokenType.HtmlTag));
                                    result.Add(new Token("</div>", TokenType.HtmlTag));
                                    result.Add(new Token("</div>", TokenType.HtmlTag));
                                    result.Add(new Token("</div>", TokenType.HtmlTag));
                                }
                                result.Add(new Token("<div class=\"enum-item\">", TokenType.HtmlTag));
                                result.Add(new Token("<div class=\"enum-item-label\">", TokenType.HtmlTag));
                                result.Add(GetPTag(data));
                                result.Add(GetSpanTag(data));
                                result.AddRange(content);
                                result.Add(new Token("</span>", TokenType.HtmlTag));
                                result.Add(new Token("</div>", TokenType.HtmlTag));
                                result.Add(new Token("</div>", TokenType.HtmlTag));
                                result.Add(new Token("<div class=\"enum-item-content\">", TokenType.HtmlTag));
                                result.Add(GetPTag(data));
                                result.Add(GetSpanTag(data));
                                continue;

                            case "@@bmk":
                                int bmkId = data.BookmarkCount++;
                                result.Add(new Token(bmkId.ToString(), TokenType.Bookmark));
                                continue;

                            case "@@ref":
                                if (isMathMode)
                                    throw new Exception("\\@@ref: cannot use in math mode.");

                                var argument = ReadTextArgument(tokens, ref i, data, "@@ref", true).Trim();
                                result.Add(new Token(argument, TokenType.Reference));
                                continue;

                            case "@@label":
                                argument = ReadTextArgument(tokens, ref i, data, "@@label", true).Trim();
                                if (data.BookmarkCount == 0)
                                    throw new Exception("\\@@label: no bookmarks found.");
                                if (!Regex.IsMatch(argument, "^[\\w\\-:]+$"))
                                    throw new Exception("\\@@label: invalid label name.");

                                cloned = data.PassToSubgroup();
                                content = Expand(Parse("\\@currentlabel"), cloned);
                                data.UpdateFromSubgroup(cloned);

                                data.Bookmarks.Add(new Bookmark
                                {
                                    Id = data.BookmarkCount - 1,
                                    Content = content,
                                    Label = argument
                                });
                                continue;

                            case "@@error":
                                argument = ReadTextArgument(tokens, ref i, data, "@@error", true).Trim();
                                throw new TextException(argument);
                        }

                        command = data.Commands[t.Text];
                        if (command == null)
                        {
                            // throw new Exception($"Undefined command '\\{t.Text}'");
                            result.Add(t);
                        }
                        else
                        {
                            if (isMathMode && command.IsTextMode)
                            {
                                result.Add(t);
                                break;
                            }

                            if (!MatchAndReplace(tokens, ref i, command, data))
                                throw new Exception($"The use of the command '\\{t.Text}' does not match any of its definitions.");

                            data.ExpansionCount++;
                        }

                        break;

                    case TokenType.MathDelim:
                        if (isMathMode)
                        {
                            // nested math is supported by MathJax, e.g. $\text{$math$}$
                            result.Add(t);
                        }
                        else
                        {
                            var math = ReadMathFormula(tokens, ref i);
                            cloned = data.PassToSubgroup();
                            cloned.Variables["math-mode"] = "1";
                            var expanded = Expand(math, cloned);
                            data.UpdateFromSubgroup(cloned);

                            foreach (var token in expanded)
                                if (token.Type != TokenType.Bookmark) // bookmarks will be moved out of the equation
                                    token.VerbatimOutput = true;

                            // equation tags etc.
                            if (t.Text == "$$" || t.Text == "\\[")
                            {
                                var newTokens = Parse("\\@@par\\floatleft{\\rm\\@beforedisplay}\\floatright{\\@afterdisplay}\\def\\@beforedisplay{}\\def\\@afterdisplay{}");
                                var cloned2 = data.PassToSubgroup();
                                newTokens = Expand(newTokens, cloned2);
                                data.UpdateFromSubgroup(cloned2);

                                for (int ii = 0; ii < expanded.Count; ii++)
                                    if (expanded[ii].Type == TokenType.Bookmark)
                                    {
                                        newTokens.Add(expanded[ii]);
                                        expanded.RemoveAt(ii--);
                                    }

                                result.AddRange(newTokens);
                                result.Add(t);
                                result.AddRange(expanded);
                                result.Add(tokens[i]);
                            }
                            else
                            {
                                var newTokens = new List<Token>();
                                for (int ii = 0; ii < expanded.Count; ii++)
                                    if (expanded[ii].Type == TokenType.Bookmark)
                                    {
                                        newTokens.Add(expanded[ii]);
                                        expanded.RemoveAt(ii--);
                                    }
                                result.Add(t);
                                result.AddRange(expanded);
                                result.Add(tokens[i]);
                                result.AddRange(newTokens);
                            }

                            // if style is changed in the formula, the </span> tag should be added after the formula
                            if (cloned.GetBool("style-changed", false, false))
                            {
                                result.Add(new Token("</span>", TokenType.HtmlTag));
                                result.Add(GetSpanTag(data));
                            }
                        }

                        break;

                    case TokenType.LineBreak:
                        if (isMathMode)
                            result.Add(t);
                        else
                            result.Add(new Token("<br />", TokenType.HtmlTag));
                        break;

                    case TokenType.Subscript:
                    case TokenType.Superscript:
                    case TokenType.Text:
                    case TokenType.Whitespace:
                    case TokenType.Tilde:
                        result.Add(t);

                        break;

                    default:
                        if (isMathMode)
                            result.Add(t);
                        else
                            throw new Exception($"Unexpected token '{t.ToString()}'");
                        break;
                }

            iterend:
                if (expandAfterPositions.Count > 0)
                    i = expandAfterPositions.Pop() - 1;
            }

            return result;
        }

        static bool MatchAndReplace(
            List<Token> tokens,
            ref int i,
            CommandDefinition command,
            ExpansionData data)
        {
            int _i = i, j = 0, jj = 0;
            int matchedDef = -1;
            var patterns = command.Patterns;
            var definitions = command.Definitions;
            List<int> argStartIndices = new List<int>(),
                argEndIndices = new List<int>();

            foreach (List<Token> patt in patterns)
            {
                _i = i + 1;

                int argNum = 0;
                foreach (Token t in patt) if (t.Type == TokenType.Argument) argNum++;
                argStartIndices.Clear();
                argEndIndices.Clear();
                j = 0;

                while (j < patt.Count) // EVERY LOOP MATCHES A PARAM
                {
                    if (_i >= tokens.Count) break; // MATCH FAILS

                    if (patt[j].Type != TokenType.Whitespace || patt[j].Text != "\n")
                    {
                        while (_i < tokens.Count && tokens[_i].Type == TokenType.Whitespace) _i++;
                        while (j < patt.Count && patt[j].Type == TokenType.Whitespace) j++;
                    }

                    bool flag = false; // flag = match fail
                    while (j < patt.Count && patt[j].Type != TokenType.Argument)
                    {
                        if (_i >= tokens.Count || tokens[_i] != patt[j]) { flag = true; break; }
                        _i++; j++;
                    }
                    if (flag) break;

                    if (j == patt.Count) break; // match completed

                    j++; // NOW J IS THE NEXT TOKEN OF #N
                    int nest = 0;
                    if (j == patt.Count || patt[j].Type != TokenType.Tilde) // NO SEPARATOR AFTER THE PARAM, I.E. \FOO#1#2 OR \FOO#1
                    {
                        // READ ONE TOKEN OR GROUP
                        while (_i < tokens.Count && tokens[_i].Type == TokenType.Whitespace) _i++;
                        if (_i == tokens.Count) break; // MATCH FAILS

                        if (tokens[_i].Type == TokenType.EndGroup) // MATCH FAILS
                        {
                            break;
                        }
                        else if (tokens[_i].Type != TokenType.BeginGroup) // MATCH SUCCEEDS
                        {
                            argStartIndices.Add(_i);
                            argEndIndices.Add(_i + 1);
                            _i++;
                            continue;
                        }
                        else
                        {
                            nest = 0;
                            for (int ii = _i; ii < tokens.Count; ii++)
                            {
                                if (tokens[ii].Type == TokenType.BeginGroup) nest++;
                                if (tokens[ii].Type == TokenType.EndGroup) nest--;
                                if (nest == 0) // MATCH SUCCEEDS
                                {
                                    argStartIndices.Add(_i + 1); // EXCLUDE THE TOKEN {
                                    argEndIndices.Add(ii);
                                    _i = ii + 1;
                                    break;
                                }
                            }
                            if (nest == 0) continue;
                            else return false; // SHOULD BE UNREACHABLE
                        }
                    }

                    // NOW THE ARGUMENT IS A LONG ONE, IE MORE THAN ONE GROUP.
                    j++; // jump over the '~'
                    nest = 0;
                    bool sepMatchSucc = false;
                    for (int ii = _i; ii < tokens.Count; ii++)
                    {
                        if (tokens[ii].Type == TokenType.BeginGroup) nest++;
                        if (tokens[ii].Type == TokenType.EndGroup) nest--;
                        if (patt[j].Type == TokenType.BeginGroup)
                        {
                            if (nest > 1) continue;
                            if (nest < 0) break;
                        }
                        else if (patt[j].Type == TokenType.EndGroup)
                        {
                            if (nest > 0) continue;
                            if (nest < -1) break;
                            if (nest == -1 && tokens[ii].Type != TokenType.EndGroup) break;
                        }
                        else
                        {
                            if (nest > 0) continue;
                            if (nest < 0) break;
                        }
                        if (tokens[ii] != patt[j]) continue;

                        jj = 0;
                        flag = false; // FLAG = UNMATCHED
                        while (jj + j < patt.Count && patt[jj + j].Type != TokenType.Argument)
                        {
                            if (ii + jj >= tokens.Count || tokens[ii + jj] != patt[jj + j]) { flag = true; break; }
                            jj++;
                        }
                        if (flag) continue;

                        j = jj + j;
                        argStartIndices.Add(_i);
                        argEndIndices.Add(ii);
                        _i = ii + jj;
                        sepMatchSucc = true;
                        break;
                    }
                    if (!sepMatchSucc)
                        break;
                }

                if (j == patt.Count && argStartIndices.Count == argNum) // MATCH SUCCEEDS
                {
                    matchedDef = patterns.IndexOf(patt);
                    break;
                }
                // ELSE CONTINUE;
            }

            if (matchedDef == -1)
                return false;

            // NOTE THAT i IS THE (EXCUSIVE) END OF THE RANGE 
            List<List<Token>> paramList = new List<List<Token>>();
            for (int k = 0; k < argStartIndices.Count; k++)
            {
                List<Token> matchedParam = new List<Token>();
                for (int kk = argStartIndices[k]; kk < argEndIndices[k]; kk++) matchedParam.Add(tokens[kk]);
                paramList.Add(matchedParam);
            }
            tokens.RemoveRange(i, _i - i);

            List<Token> pattern = patterns[matchedDef], definition = definitions[matchedDef],
                ttokens = new List<Token>();

            for (j = 0; j < definition.Count; j++)
            {
                if (definition[j].Type != TokenType.Argument)
                {
                    ttokens.Add(definition[j]);
                }
                else
                {
                    jj = 0;
                    int jjj;
                    for (jjj = 0; jjj < pattern.Count; jjj++)
                    {
                        if (pattern[jjj] == definition[j]) break;
                        if (pattern[jjj].Type == TokenType.Argument)
                            jj++;
                    }
                    if (jjj == pattern.Count) // DON'T throw an exception since there may be nested defs.
                        ttokens.Add(definition[j]);
                    else
                        for (int k = 0; k < paramList[jj].Count; k++)
                            ttokens.Add(paramList[jj][k]);
                }
            }
            ExpandSpecials(ttokens, data);
            tokens.InsertRange(i, ttokens);

            i = i - 1;
            return true;
        }

        static Token ReplaceBgEg(Token t)
        {
            if (t.Type == TokenType.Command)
            {
                if (t.Text == "@@bg") return new Token(null, TokenType.BeginGroup);
                if (t.Text == "@@eg") return new Token(null, TokenType.EndGroup);
            }
            return t;
        }

        static List<Token> ReadArgument(List<Token> tokens, ref int i, string commandName) // i should be at the command, or the end of the last argument. It will be set to the end of the current argument.
        {
            i++;
            while (i < tokens.Count && tokens[i].Type == TokenType.Whitespace) i++;
            if (i == tokens.Count) throw commandName == null ? new Exception("'^' or '_' does not get enough arguments.") :
                    new Exception($"'\\{commandName}' does not get enough arguments.");
            switch (tokens[i].Type)
            {
                case TokenType.BeginGroup:
                    int ii = GetGroupEnd(tokens, i);
                    int _i = i;
                    i = ii;
                    return tokens.GetRange(_i + 1, ii - _i - 1);
                default:
                    return tokens.GetRange(i, 1);
            }
        }

        static List<Token> ReadMathFormula(List<Token> tokens, ref int i)
        {
            var start = tokens[i];
            string expected = start.Text == "\\(" ? "\\)" :
                start.Text == "\\[" ? "\\]" : start.Text;

            int nest = 0, ii;
            for (ii = i + 1; ii < tokens.Count; ii++)
            {
                if (tokens[ii].Type == TokenType.BeginGroup) nest++;
                if (tokens[ii].Type == TokenType.EndGroup) nest--;
                if (nest == 0 && tokens[ii].Type == TokenType.MathDelim) break;
            }

            if (nest > 0 || tokens[ii].Text != expected)
                throw new Exception($"'{start.Text}' is unmatched.");
            int _i = i;
            i = ii;
            return tokens.GetRange(_i + 1, ii - _i - 1);
        }

        static string ReadTextArgument(List<Token> tokens, ref int i, ExpansionData data, string commandName, bool replaceValue)
        {
            i++;
            while (i < tokens.Count && tokens[i].Type == TokenType.Whitespace) i++;
            if (i == tokens.Count) throw new Exception($"'\\{commandName}' does not get enough arguments.");
            string ss, t = null;
            switch (tokens[i].Type)
            {
                case TokenType.Text:
                    return tokens[i].Text;
                case TokenType.BeginGroup:
                    string s = "";
                    int ii = GetGroupEnd(tokens, i);
                    List<Token> ttokens = tokens.GetRange(i + 1, ii - i - 1);

                    var cloned = data.PassToSubgroup();
                    Expand(ttokens, cloned);
                    data.UpdateFromSubgroup(cloned);

                    for (int iii = 0; iii < ttokens.Count; iii++)
                    {
                        switch (ttokens[iii].Type)
                        {
                            case TokenType.BeginGroup:
                                if (!replaceValue) break;
                                int iiii = iii - 1;
                                ss = ReadTextArgument(ttokens, ref iiii, data, commandName, true);
                                t = data.Variables[ss];
                                if (t != null)
                                {
                                    iii = iiii;
                                    s += t;
                                }
                                else
                                {
                                    s += ss;
                                }
                                break;
                            case TokenType.EndGroup:
                                break;
                            case TokenType.Text:
                            case TokenType.Whitespace:
                                s += ttokens[iii].Text;
                                break;
                            default:
                                throw new Exception($"Argument of '\\{commandName}' is not a plain string.");
                        }
                    }
                    i = ii;
                    return s;

                default:
                    throw new Exception($"Argument of '\\{commandName}' is not a plain string.");
            }
        }

        static void IfZero(List<Token> tokens, ref int i, ExpansionData data)
        {
            int _i = i;
            string varName = ReadTextArgument(tokens, ref i, data, "@@ifzero", true);
            List<Token> lt1 = ReadArgument(tokens, ref i, "@@ifzero"),
                lt2 = ReadArgument(tokens, ref i, "@@ifzero");

            tokens.RemoveRange(_i, i - _i + 1);
            if (data.GetInt(varName, 1, false) == 0)
                tokens.InsertRange(_i, lt1);
            else
                tokens.InsertRange(_i, lt2);

            i = _i - 1;
        }

        static readonly string[] StyleVariables =
        {
            "bold",
            "color",
            "float",
            "font-size",
            "italic"
        };

        static Token GetSpanTag(ExpansionData data)
        {
            var styles = new List<string>();

            double fontSize = data.GetDouble("font-size", DefaultFontSize, false);
            string color = data.GetString("color", DefaultColor, false),
                _float = data.GetString("float", null, false);
            bool bold = data.GetBool("bold", false, false),
                italic = data.GetBool("italic", false, false);

            if (fontSize != DefaultFontSize)
            {
                if (fontSize < 6) fontSize = 6;
                if (fontSize > 48) fontSize = 48;
                styles.Add($"font-size: {fontSize}px");
            }
            if (color != DefaultColor)
            {
                color = color.ToLower();
                if (Regex.IsMatch(color, "^[0-9a-f]{6}$"))
                    styles.Add($"color: #{color}");
            }
            if (bold)
                styles.Add("font-weight: bold");
            if (italic)
                styles.Add("font-style: italic");
            if (_float == "right" || _float == "left")
                styles.Add($"float: {_float}");

            if (styles.Count == 0)
                return new Token("<span>", TokenType.HtmlTag);
            string s = "";
            foreach (var style in styles)
                s += style + "; ";
            s = s.Substring(0, s.Length - 2);
            return new Token($"<span style=\"{s}\">", TokenType.HtmlTag);
        }

        static Token GetPTag(ExpansionData data, string name = "div")
        {
            var styles = new List<string>();

            double parSep = data.GetDouble("par-sep", 18, false);

            if (parSep != 18)
            {
                if (parSep < 0) parSep = 0;
                if (parSep > 48) parSep = 48;
                styles.Add($"margin-bottom: {parSep}px");
            }

            if (styles.Count == 0)
                return new Token($"<{name} class=\"paragraph\">", TokenType.HtmlTag);
            string s = "";
            foreach (var style in styles)
                s += style + "; ";
            s = s.Substring(0, s.Length - 2);
            return new Token($"<{name} class=\"paragraph\" style=\"{s}\">", TokenType.HtmlTag);
        }

        private static readonly string[] RomanThousands =
            { "", "m", "mm", "mmm" };
        private static readonly string[] RomanHundreds =
            { "", "c", "cc", "ccc", "cd", "d", "dc", "dcc", "dccc", "cm" };
        private static readonly string[] RomanTens =
            { "", "x", "xx", "xxx", "xl", "l", "lx", "lxx", "lxxx", "xc" };
        private static readonly string[] RomanOnes =
            { "", "i", "ii", "iii", "iv", "v", "vi", "vii", "viii", "ix" };

        private static string ToRoman(int i)
        {
            if (i == 0)
                return "0";
            if (i < 0)
                return "-" + ToRoman(-i);

            // See if it's >= 4000.
            if (i >= 4000)
            {
                // Use parentheses.
                int thou = i / 1000;
                i %= 1000;
                return "(" + ToRoman(thou) + ")" +
                    (i == 0 ? "" : ToRoman(i));
            }

            string result = "";

            int num;
            num = i / 1000;
            result += RomanThousands[num];
            i %= 1000;

            num = i / 100;
            result += RomanHundreds[num];
            i %= 100;

            num = i / 10;
            result += RomanTens[num];
            i %= 10;

            result += RomanOnes[i];

            return result;
        }
    }
}
