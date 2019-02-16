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

        public const int MaxBuffer = 1000000, MaxLength = 100000;
        const string DefaultColor = "000000",
            DefaultBlockBackground = "e8f6ff",
            DefaultBlockBorder = "44bbee";
        const double DefaultFontSize = 18;
        const string VariableNameRegex = @"^[\w\-]+$", ColorRegex = "^[0-9a-fA-F]{6}$";

        private static bool IsValidCodePoint(int point) // returns false for 0xd800..0xdfff, 0xfdd0..0xfdef, 0x{.}fffe, 0x{.}ffff where {.} = 0..f, 10
        {
            return point >= 0 &&
                ((point < 0xfdd0 && !(point >= 0xd800 && point < 0xe000)) ||
                    (point >= 0xfdf0 &&
                        ((point & 0xffff) != 0xffff) &&
                        ((point & 0xfffe) != 0xfffe) &&
                        point <= 0x10ffff));
        }

        public static List<Token> Parse(string s, string fileName)
        {
            if (s.Length > MaxLength)
                throw TextException.TextTooLong(MaxLength);

            List<Token> tokens = new List<Token>();
            s = s.Replace("\r\n", "\n").Replace("\r", "\n");
            s += " ";
            string t = "";
            bool ignoreNextWhitespace = false;
            int line = 0, lineStart = 0;

            TextPosition pos(int i)
                => new TextPosition(fileName, line, i - lineStart);

            for (int i = 0; i < s.Length - 1; i++)
            {
                int _i = i;

                if (char.IsWhiteSpace(s[i]))
                {
                    if (s[i] == '\n')
                    {
                        tokens.Add(new Token("\n", TokenType.Whitespace, pos(i), pos(i + 1)));
                        line++;
                        lineStart = i + 1;
                    }
                    else if (!ignoreNextWhitespace)
                        tokens.Add(new Token(s[i].ToString(), TokenType.Whitespace, pos(i), pos(i + 1)));
                    continue;
                }

                ignoreNextWhitespace = false;
                switch (s[i])
                {
                    case '{':
                        tokens.Add(new Token(null, TokenType.BeginGroup, pos(_i), pos(i + 1)));
                        break;
                    case '}':
                        tokens.Add(new Token(null, TokenType.EndGroup, pos(_i), pos(i + 1)));
                        break;
                    case '^':
                        tokens.Add(new Token(null, TokenType.Superscript, pos(_i), pos(i + 1)));
                        break;
                    case '_':
                        tokens.Add(new Token(null, TokenType.Subscript, pos(_i), pos(i + 1)));
                        break;
                    case '&':
                        tokens.Add(new Token(null, TokenType.Tab, pos(_i), pos(i + 1)));
                        break;
                    case '%':
                        while (i < s.Length - 1 && s[i] != '\n') i++;
                        ignoreNextWhitespace = true;
                        line++;
                        lineStart = i + 1;
                        break;
                    case '~':
                        tokens.Add(new Token(null, TokenType.Tilde, pos(_i), pos(i + 1)));
                        break;
                    case '$':
                        if (s[i + 1] == '$')
                        {
                            tokens.Add(new Token("$$", TokenType.MathDelim, pos(_i), pos(i + 2)));
                            i++;
                            break;
                        }
                        tokens.Add(new Token("$", TokenType.MathDelim, pos(_i), pos(i + 1)));
                        break;
                    case '\\':
                        i++;
                        if (s[i] == '\\')
                        {
                            tokens.Add(new Token(null, TokenType.LineBreak, pos(_i), pos(i + 1)));
                            break;
                        }
                        if ("()[]".Contains(s[i]))
                        {
                            tokens.Add(new Token("\\" + s[i], TokenType.MathDelim, pos(_i), pos(i + 1)));
                            break;
                        }

                        if (char.IsWhiteSpace(s[i]) || SpecialChars.Contains(s[i]))
                            tokens.Add(new Token(s[i].ToString(), TokenType.Command, pos(_i), pos(i + 1)));
                        else
                        {
                            t = "";
                            while (!(char.IsWhiteSpace(s[i]) || SpecialChars.Contains(s[i])))
                                t += s[i++];
                            i--;
                            tokens.Add(new Token(t, TokenType.Command, pos(_i), pos(i + 1)));
                            ignoreNextWhitespace = true;
                        }
                        break;
                    case '#':
                        t = "";
                        while (s[i] == '#')
                            t += s[i++];
                        if (char.IsWhiteSpace(s[i]) || SpecialChars.Contains(s[i]))
                            t += s[i];
                        else
                        {
                            while (!(char.IsWhiteSpace(s[i]) || SpecialChars.Contains(s[i])))
                                t += s[i++];
                            i--;
                            ignoreNextWhitespace = true;
                        }
                        tokens.Add(new Token(t, TokenType.Argument, pos(_i), pos(i + 1)));
                        break;
                    default:
                        if (char.IsSurrogatePair(s, i))
                        {
                            int point = 0x10000 + (s[i] - 0xd800) * 0x400 + s[i + 1] - 0xdc00;
                            if (IsValidCodePoint(point))
                                tokens.Add(new Token(s.Substring(i, 2), TokenType.Text, pos(_i), pos(i + 2)));
                            i++;
                        }
                        else
                        {
                            if (IsValidCodePoint(s[i]))
                                tokens.Add(new Token(s[i].ToString(), TokenType.Text, pos(_i), pos(i + 1)));
                        }
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
            if (nest > 0) throw TextException.UnmatchedBracket(tokens[start], "{");
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
                        ((result[i + 1].Type == TokenType.HtmlTag && result[i + 1].Text == "</span>") || // remove empty spans
                        (result[i + 1].Type == TokenType.Whitespace && i + 3 < result.Count && result[i + 2].Type == TokenType.HtmlTag && // remove whitespace spans at end of a div
                        result[i + 2].Text == "</span>" && result[i + 3].Type == TokenType.HtmlTag && result[i + 3].Text == "</div>")))
                    {
                        result.RemoveRange(i, result[i + 1].Type == TokenType.Whitespace ? 3 : 2);
                        i -= 2;  // as in <div><span></span></div>
                        if (i < -1) i = -1;
                        continue;
                    }
                    if (token.Text == "<div>" || token.Text.StartsWith("<div "))
                    {
                        // remove empty divs
                        bool flag = false;
                        int depth = 1, j;
                        for (j = i + 1; j < result.Count; j++)
                        {
                            var t = result[j];
                            if (t.Type == TokenType.Whitespace ||
                                (t.Type == TokenType.HtmlTag && (t.Text.StartsWith("<span") || t.Text.StartsWith("</span"))))
                                continue;
                            if (t.Type == TokenType.HtmlTag)
                            {
                                if (t.Text.StartsWith("<div")) { depth++; continue; }
                                if (t.Text.StartsWith("</div")) { depth--; if (depth == 0) { flag = true; break; } continue; }
                            }
                            break;
                        }

                        if (flag)
                        {
                            result.RemoveRange(i, j - i + 1);
                            i -= 2;
                            if (i < -1) i = -1;
                        }
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

            bool isMathMode = data.GetBool("math-mode", false, false),
                isTextOnly = data.GetBool("-text-only", false, false);

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
                        result.AddRange(Expand(tokens.GetRange(i + 1, groupEnd - i - 1), cloned));
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
                        throw TextException.UnmatchedBracket(t, "}");

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
                                    throw TextException.TokenExpected(t);

                            case "@@ne":
                                if (i + 1 < tokens.Count)
                                {
                                    tokens.RemoveAt(i);
                                    i--;
                                    goto iterend;
                                }
                                else
                                    throw TextException.TokenExpected(t);

                            case "@@csname":
                                string csName = ReadTextArgument(tokens, ref i, data, t, true);
                                if (csName == "")
                                    throw TextException.EmptyCsName(t);
                                tokens.RemoveRange(_i, i - _i + 1);
                                tokens.Insert(_i, new Token(csName, TokenType.Command, t));
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
                                if (i == tokens.Count)
                                    throw TextException.TokenExpected(t);
                                if (tokens[i].Type != TokenType.Command)
                                    throw TextException.InvalidMacroName(t, tokens[i].ToString());
                                name = tokens[i].Text;
                                if (Preamble.Predefined.Contains(name))
                                    throw TextException.InvalidMacroName(t, tokens[i].ToString());

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
                                                throw TextException.NoMatchingDefinition(token);
                                        }
                                        else if (token.Text == "@@csname")
                                        {
                                            int __i = i + 1;
                                            csName = ReadTextArgument(tokens, ref __i, data, token, true);
                                            tokens.RemoveRange(i, __i - i);
                                            tokens.Insert(i, new Token(csName, TokenType.Command));
                                        }
                                        else if (!Preamble.Predefined.Contains(token.Text))
                                        {
                                            throw TextException.UndefinedCommand(token);
                                        }
                                    }
                                    else
                                        patTokens.Add(ReplaceBgEg(tokens[i]));
                                }

                                if (i == tokens.Count) throw TextException.TokenExpected(t, "{");
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
                                                throw TextException.NoMatchingDefinition(token);
                                        }
                                        else if (token.Text == "@@csname")
                                        {
                                            int __i = i + 1;
                                            csName = ReadTextArgument(groupTokens, ref __i, data, token, true);
                                            groupTokens.RemoveRange(i, __i - i);
                                            groupTokens.Insert(i, new Token(csName, TokenType.Command));
                                        }
                                        else if (!Preamble.Predefined.Contains(token.Text))
                                        {
                                            throw TextException.UndefinedCommand(token);
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
                                if (i == tokens.Count)
                                    throw TextException.TokenExpected(t);
                                if (tokens[i].Type == TokenType.Argument)
                                    throw TextException.InvalidMacroName(t, tokens[i].ToString());
                                Token tt = tokens[i];
                                if (tt.Type == TokenType.Command && Preamble.Predefined.Contains(tt.Text))
                                    throw TextException.InvalidMacroName(t, tokens[i].ToString());

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

                                if (i == tokens.Count) throw TextException.TokenExpected(t, "{");
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
                                    throw TextException.TokenExpected(t);
                                name = tokens[i].Text;
                                if (Preamble.Predefined.Contains(name))
                                    throw TextException.InvalidMacroName(t, tokens[i].ToString());
                                var token2 = tokens[i + 1];
                                string name2 = token2.Text;

                                command = data.Commands[name];
                                if (Preamble.Predefined.Contains(name2))
                                {
                                    command2 = new CommandDefinition(name2)
                                    {
                                        Patterns = { new List<Token>() },
                                        Definitions = { new List<Token> { new Token(name2, TokenType.Command, token2.Start, token2.End) } }
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
                                Token tokText = new Token(ReadTextArgument(tokens, ref i, data, t, false), TokenType.Text);
                                tokens.RemoveRange(_i, i - _i + 1);
                                tokens.Insert(_i, tokText);
                                i = _i - 1;
                                goto iterend;

                            case "@@char":
                                string charCode = ReadTextArgument(tokens, ref i, data, t, false);
                                if (int.TryParse(charCode,
                                    System.Globalization.NumberStyles.HexNumber,
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    out int theChar))
                                {
                                    Token tokChar;
                                    if (theChar >= 0 && theChar <= 0xffff && !(theChar >= 0xd800 && theChar < 0xe000))
                                        tokChar = new Token(((char)theChar).ToString(), TokenType.Text);
                                    else if (theChar >= 0x10000 && theChar <= 0x10ffff)
                                        tokChar = new Token("" + ((char)(0xd800 + (theChar - 0x10000) / 0x400)) + ((char)(0xdc00 + theChar % 0x400)), TokenType.Text);
                                    else throw TextException.InvalidCharCode(t, charCode);

                                    tokens.RemoveRange(_i, i - _i + 1);
                                    tokens.Insert(_i, tokChar);
                                    i = _i - 1;
                                }
                                else throw TextException.InvalidCharCode(t, charCode);
                                goto iterend;

                            case "@@get":
                                int getOption = data.GetInt("get-format", 0, true);
                                varName = ReadTextArgument(tokens, ref i, data, t, true).Trim();
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
                                varName = ReadTextArgument(tokens, ref i, data, t, true);
                                if (!Regex.IsMatch(varName, VariableNameRegex)) throw TextException.InvalidVariableName(t, varName);
                                varValue = ReadTextArgument(tokens, ref i, data, t, true).Trim();
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
                                varName = ReadTextArgument(tokens, ref i, data, t, true).Trim();
                                if (!Regex.IsMatch(varName, VariableNameRegex)) throw TextException.InvalidVariableName(t, varName);
                                varValue = ReadTextArgument(tokens, ref i, data, t, true);
                                int intValue = data.GetInt(varName, 0, false),
                                    addValue = 0;
                                varValue = varValue.Replace("--", ""); // as in {-{key}}
                                int.TryParse(varValue, out addValue);
                                data.Variables[varName] = (intValue + addValue).ToString();
                                tokens.RemoveRange(_i, i - _i + 1);
                                i = _i - 1;
                                continue;

                            case "@@ifzero":
                                IfZero(tokens, ref i, t, data);
                                data.ExpansionCount++;
                                continue;

                            case "@@par":
                                result.Add(new Token("</span>", TokenType.HtmlTag));
                                result.Add(new Token("</div>", TokenType.HtmlTag));
                                result.Add(GetPTag(data));
                                result.Add(GetSpanTag(data));
                                continue;

                            case "@@list":
                                var content = ReadArgument(tokens, ref i, t);
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

                                content = Parse("\\@itemlabel", null);
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

                            case "@@block":
                                var blockBackground = data.GetString("block-background-color", DefaultBlockBackground, true, ColorRegex).ToLower();
                                var blockBorder = data.GetString("block-border-color", DefaultBlockBorder, true, ColorRegex).ToLower();

                                content = ReadArgument(tokens, ref i, t);
                                cloned = data.PassToSubgroup();
                                content = Expand(content, cloned);
                                data.UpdateFromSubgroup(cloned);

                                result.Add(new Token("</span>", TokenType.HtmlTag));
                                result.Add(new Token("</div>", TokenType.HtmlTag));
                                result.Add(new Token($"<div class=\"block-paragraph\" style=\"background-color:#{blockBackground};border-color:#{blockBorder}\">", TokenType.HtmlTag));
                                result.Add(GetPTag(data));
                                result.Add(GetSpanTag(data));
                                result.AddRange(content);
                                result.Add(new Token("</span>", TokenType.HtmlTag));
                                result.Add(new Token("</div>", TokenType.HtmlTag));
                                result.Add(new Token("</div>", TokenType.HtmlTag));
                                result.Add(GetPTag(data));
                                result.Add(GetSpanTag(data));
                                continue;

                            case "@@bmk":
                                int bmkId = data.BookmarkCount++;
                                result.Add(new Token(bmkId.ToString(), TokenType.Bookmark));
                                continue;

                            case "@@ref":
                                if (isMathMode)
                                    throw TextException.InvalidInMathMode(t);
                                string refText = data.GetString("ref-text", null, true)?.Trim();
                                if (string.IsNullOrEmpty(refText)) refText = null;
                                var argument = ReadTextArgument(tokens, ref i, data, t, true).Trim()
                                    .Replace(":TEXT:", ":text:");
                                if (refText != null)
                                    argument += ":TEXT:" + refText;
                                result.Add(new Token(argument, TokenType.Reference));
                                continue;

                            case "@@label":
                                argument = ReadTextArgument(tokens, ref i, data, t, true).Trim();
                                if (!Regex.IsMatch(argument, VariableNameRegex))
                                    throw TextException.InvalidLabelName(t, argument);

                                cloned = data.PassToSubgroup();
                                content = Expand(Parse("\\@currentlabel", null), cloned);
                                data.UpdateFromSubgroup(cloned);

                                data.Bookmarks.Add(new Bookmark
                                {
                                    Id = data.BookmarkCount - 1,
                                    Content = content,
                                    Label = argument
                                });
                                continue;

                            case "@@file":
                                argument = ReadTextArgument(tokens, ref i, data, t, true).Trim();
                                result.Add(new Token($"<:FILE:{HttpUtility.HtmlEncode(argument)}>", TokenType.Placeholder));
                                continue;

                            case "@@code":
                                if (isMathMode)
                                    throw TextException.InvalidInMathMode(t);
                                if (i + 1 == tokens.Count)
                                    throw TextException.TokenExpected(t);

                                bool displayCode = data.GetBool("display-code", false, true),
                                    noColoring = data.GetBool("code-no-coloring", false, true);
                                char mode = displayCode ? 'D' : noColoring ? 'i' : 'I'; // display vs inline
                                TextPosition start, end;
                                if (tokens[i + 1].Type == TokenType.BeginGroup)
                                {
                                    _i = GetGroupEnd(tokens, i + 1);
                                    start = tokens[i + 1].End;
                                    end = tokens[_i].Start;
                                }
                                else
                                {
                                    _i = i + 1;
                                    start = tokens[i + 1].Start;
                                    end = tokens[i + 1].End;
                                }

                                if (start.FileName != "user" || end.FileName != "user" ||
                                    start.Line > end.Line ||
                                    (start.Line == end.Line && start.Ch > end.Ch))
                                    throw TextException.CustomMessage(t, "代码片段的输入格式不正确。");

                                if (mode == 'D')
                                {
                                    result.Add(new Token("</span>", TokenType.HtmlTag));
                                    result.Add(new Token("</div>", TokenType.HtmlTag));
                                }
                                // records only the start and end positions
                                result.Add(new Token($"{mode}/{start.Line}:{start.Ch}/{end.Line}:{end.Ch}", TokenType.CodeSnippet));
                                if (mode == 'D')
                                {
                                    result.Add(GetPTag(data));
                                    result.Add(GetSpanTag(data));
                                }
                                i = _i;
                                continue;

                            case "@@error":
                                argument = ReadTextArgument(tokens, ref i, data, t, true).Trim();
                                throw TextException.CustomMessage(t, argument);
                        }

                        command = data.Commands[t.Text];
                        if (command == null)
                        {
                            if (!isMathMode)
                                throw TextException.UndefinedCommand(t);
                            else
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
                                throw TextException.NoMatchingDefinition(t);

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
                                var newTokens = Parse("\\@@par\\floatleft{\\rm\\@beforedisplay}\\floatright{\\@afterdisplay}\\@@def\\@beforedisplay{}\\@@def\\@afterdisplay{}", null);
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
                            throw TextException.UnexpectedToken(t);
                        break;
                }

            iterend:
                if (expandAfterPositions.Count > 0)
                    i = expandAfterPositions.Pop() - 1;
            }

            if (isTextOnly)
                result = result.Where(token => token.Type != TokenType.HtmlTag).ToList();
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
                    if (j == patt.Count - 1 && patt[j].Type == TokenType.Tilde)
                        j++; // TILDE AT END OF PATTERN IS IGNORED
                    int nest = 0;
                    if (j == patt.Count || patt[j].Type != TokenType.Tilde) // SHORT PARAM (WITHOUT TILDE)
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

            List<Token> pattern = patterns[matchedDef], definition = definitions[matchedDef],
                ttokens = new List<Token>();

            var ancestor = tokens[i];
            for (j = 0; j < definition.Count; j++)
            {
                if (definition[j].Type != TokenType.Argument)
                {
                    ttokens.Add(definition[j].AddAncestor(ancestor));
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
                        ttokens.Add(definition[j].AddAncestor(ancestor));
                    else
                        for (int k = 0; k < paramList[jj].Count; k++)
                            ttokens.Add(paramList[jj][k].AddAncestor(ancestor));
                }
            }
            ExpandSpecials(ttokens, data);
            var token = tokens[i];
            tokens.RemoveRange(i, _i - i);
            tokens.InsertRange(i, ttokens);

            i = i - 1;
            if (tokens.Count > MaxBuffer)
                throw TextException.MaxBufferExceeded(token, MaxBuffer);
            return true;
        }

        static Token ReplaceBgEg(Token t)
        {
            if (t.Type == TokenType.Command)
            {
                if (t.Text == "@@bg") return new Token(null, TokenType.BeginGroup, t);
                if (t.Text == "@@eg") return new Token(null, TokenType.EndGroup, t);
            }
            return t;
        }

        static List<Token> ReadArgument(List<Token> tokens, ref int i, Token t) // i should be at the command, or the end of the last argument. It will be set to the end of the current argument.
        {
            i++;
            while (i < tokens.Count && tokens[i].Type == TokenType.Whitespace) i++;
            if (i == tokens.Count) throw TextException.NoMatchingDefinition(t);
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
                if (nest < 0) throw TextException.UnmatchedBracket(tokens[ii], "}");
            }

            if (nest > 0 || ii == tokens.Count || tokens[ii].Text != expected)
                throw TextException.UnmatchedBracket(start, start.Text);
            int _i = i;
            i = ii;
            return tokens.GetRange(_i + 1, ii - _i - 1);
        }

        static string ReadTextArgument(List<Token> tokens, ref int i, ExpansionData data, Token tok, bool replaceValue)
        {
            i++;
            while (i < tokens.Count && tokens[i].Type == TokenType.Whitespace) i++;
            if (i == tokens.Count) throw TextException.NoMatchingDefinition(tok);
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
                    cloned.Variables["-text-only"] = "1";
                    ttokens = Expand(ttokens, cloned);
                    data.UpdateFromSubgroup(cloned);

                    for (int iii = 0; iii < ttokens.Count; iii++)
                    {
                        switch (ttokens[iii].Type)
                        {
                            case TokenType.BeginGroup:
                                if (!replaceValue) break;
                                int iiii = iii - 1;
                                ss = ReadTextArgument(ttokens, ref iiii, data, tok, true);
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
                                throw TextException.PlainStringExpected(tok);
                        }
                    }
                    i = ii;
                    return s;

                default:
                    throw TextException.PlainStringExpected(tok);
            }
        }

        static void IfZero(List<Token> tokens, ref int i, Token t, ExpansionData data)
        {
            int _i = i;
            string varName = ReadTextArgument(tokens, ref i, data, tokens[i], true);
            List<Token> lt1 = ReadArgument(tokens, ref i, t),
                lt2 = ReadArgument(tokens, ref i, t);

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
            string color = data.GetString("color", DefaultColor, false, ColorRegex),
                _float = data.GetString("float", null, false, "^(left|right)$");
            int bold = data.GetInt("bold", -1, false);
            bool italic = data.GetBool("italic", false, false);

            if (fontSize != DefaultFontSize)
            {
                if (fontSize < 6) fontSize = 6;
                if (fontSize > 48) fontSize = 48;
                styles.Add($"font-size: {fontSize / 10}rem");
            }
            if (color != DefaultColor)
                styles.Add($"color: #{color}");
            if (bold == -2 || bold == 1)
                styles.Add("font-weight: " + (bold == -2 ? "300" : "bold"));
            if (italic)
                styles.Add("font-style: italic");
            if (_float != null)
                styles.Add($"float: {_float}");

            if (styles.Count == 0)
                return new Token("<span>", TokenType.HtmlTag);
            string s = "";
            foreach (var style in styles)
                s += style + "; ";
            s = s.Substring(0, s.Length - 2);
            return new Token($"<span style=\"{s}\">", TokenType.HtmlTag);
        }

        static Token GetPTag(ExpansionData data, string name = "div", string _class = "paragraph")
        {
            var styles = new List<string>();

            double parSepAbove = data.GetDouble("par-sep-above", 0, false),
                parSepBelow = data.GetDouble("par-sep-below", 18, false);
            int textAlign = data.GetInt("text-align", 0, false); // 0 = justify, 1 = l, 2 = c, 3 = r.
            bool noPageBreak = data.GetBool("no-page-break", false, true);

            if (parSepAbove != 0)
            {
                if (parSepAbove < 0) parSepAbove = 0;
                if (parSepAbove > 48) parSepAbove = 48;
                styles.Add($"margin-top: {parSepAbove / 10}rem");
            }
            if (parSepBelow != 18)
            {
                if (parSepBelow < 0) parSepBelow = 0;
                if (parSepBelow > 48) parSepBelow = 48;
                styles.Add($"margin-bottom: {parSepBelow / 10}rem");
            }
            if (textAlign >= 1 && textAlign <= 3)
            {
                styles.Add("text-align: " +
                    (textAlign == 1 ? "left" :
                    textAlign == 2 ? "center" : "right"));
            }
            if (noPageBreak)
                styles.Add("page-break-after: avoid");

            if (styles.Count == 0)
                return new Token($"<{name} class=\"{_class}\">", TokenType.HtmlTag);
            string s = "";
            foreach (var style in styles)
                s += style + "; ";
            s = s.Substring(0, s.Length - 2);
            return new Token($"<{name} class=\"{_class}\" style=\"{s}\">", TokenType.HtmlTag);
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
