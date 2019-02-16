using Banana.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Banana.Text
{
    public class Parser
    {
        // compiles tokens and returns html string
        // labels are used for \ref{} substituting
        // page can be null (e.g. when compiling a new course name)
        public static string ToHtml(string s, UserPage page, ParserOptions options, out ExpansionData data)
        {
            data = Preamble.GetInitialExpansionData();

            if (string.IsNullOrWhiteSpace(s))
                return "";

            try
            {
                var tokens = Expression.Parse(s, "user");

                // add \@secnum
                var secNum = new List<Token>();
                if (!string.IsNullOrWhiteSpace(page?.SectionNumber))
                {
                    foreach (char c in page.SectionNumber)
                        secNum.Add(new Token(c.ToString(), TokenType.Text, TextPosition.None, TextPosition.None));
                }
                var cd = new CommandDefinition("@secnum")
                {
                    Patterns = { new List<Token>() },
                    Definitions = { secNum }
                };
                data.Commands[cd.Name] = cd;

                Expression.ExpandSpecials(tokens, data);
                tokens = Expression.ExpandFinal(tokens, data);

                ReplaceCodeTokens(tokens, s);

                return ToHtmlFinal(tokens, options, data);
            }
            catch (TextException e)
            {
                if (options.HasFlag(ParserOptions.SingleLine))
                    return "[错误]";
                return "<p>错误！代码没有通过编译。以下是错误信息。</p><pre class=\"error-message\">" +
                    HttpUtility.HtmlEncode(e.GetMessage()) + "</pre>";
            }
            //catch (Exception e)
            //{
            //    if (options.HasFlag(ParserOptions.SingleLine))
            //        return "[错误]";
            //    return $"<p>错误！{e.Message}</p>";
            //}
        }

        // replaces CodeSnippet tokens, which only know the start and end positions, with the actual text
        private static void ReplaceCodeTokens(List<Token> tokens, string s)
        {
            var lines = s.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            foreach (var token in tokens)
            {
                if (token.Type == TokenType.CodeSnippet)
                {
                    string prefix = token.Text.Substring(0, 2), text = "";

                    var match = Regex.Match(token.Text.Substring(2), @"^(\d+):(\d+)/(\d+):(\d+)$");
                    if (!match.Success) throw new Exception();

                    int startLine = int.Parse(match.Groups[1].Value),
                        startCh = int.Parse(match.Groups[2].Value),
                        endLine = int.Parse(match.Groups[3].Value),
                        endCh = int.Parse(match.Groups[4].Value);
                    char tempLineSeparator = '\ufff0', // \ufff0 won't be removed by Trim()
                        tempBackslash = '\ufff1',
                        tempNumberSign = '\ufff2';

                    if (startLine == endLine)
                        text = lines[startLine].Substring(startCh, endCh - startCh);
                    else
                    {
                        text = lines[startLine].Substring(startCh) + tempLineSeparator;
                        for (int i = startLine + 1; i < endLine; i++)
                            text += lines[i] + tempLineSeparator;
                        text += lines[endLine].Substring(0, endCh);
                    }

                    // disallow empty lines at start / end of text
                    while (text.TrimStart().StartsWith(tempLineSeparator))
                        text = text.TrimStart().TrimStart(tempLineSeparator);
                    while (text.TrimEnd().EndsWith(tempLineSeparator))
                        text = text.TrimEnd().TrimEnd(tempLineSeparator);
                    text = text.Replace("\\\\", tempBackslash.ToString())
                        .Replace("\\#", tempNumberSign.ToString())
                        .Replace("#{", "{").Replace("#}", "}").Replace("#\\", "\\").Replace("#%", "%")
                        .Replace(tempNumberSign.ToString(), "\\#")
                        .Replace(tempBackslash.ToString(), "\\\\");

                    if (prefix == "D/")
                        text = text.Replace(tempLineSeparator.ToString(), "\r\n");
                    else // multi-line inline code will be weird
                        text = text.Replace(tempLineSeparator, ' ');

                    token.Text = prefix + text;
                }
            }
        }

        // converts compiled tokens to html string
        public static string ToHtmlFinal(List<Token> tokens, ParserOptions options, ExpansionData data = null)
        {
            string html = "";
            bool singleLine = options.HasFlag(ParserOptions.SingleLine);

            if (!singleLine)
            {
                foreach (var bookmark in data.Bookmarks)
                {
                    if (bookmark.Id == -1)
                        html += $"<a id=\"{bookmark.Label}\" class=\"bookmark-top\"></a>";
                }
            }

            Token last = null, lastText = null;
            foreach (var token in tokens)
            {
                if (token.VerbatimOutput)
                {
                    if (last?.Type == TokenType.Command && token.Type == TokenType.Text && !Expression.SpecialChars.Contains(token.Text[0]))
                        html += " ";

                    if (token.Type == TokenType.Whitespace)
                    {
                        if (last?.Type != TokenType.Whitespace)
                            html += " ";
                    }
                    // disable html tags etc. in verbatim output (i.e. math mode)
                    else if (token.Type != TokenType.HtmlTag && token.Type != TokenType.Bookmark && token.Type != TokenType.Placeholder)
                        html += HttpUtility.HtmlEncode(token.ToString());
                }
                else
                {
                    // add space between cjk and english/formulas
                    if (token.Type == TokenType.MathDelim || (token.Type == TokenType.Text && token.Text.Length == 1))
                    {
                        if (lastText != null)
                        {
                            char c0 = lastText.Type == TokenType.MathDelim ? '0' : lastText.Text[0],
                             c1 = token.Type == TokenType.MathDelim ? '0' : token.Text[0];

                            // e-c space
                            var cat0 = char.GetUnicodeCategory(c0);
                            var cat1 = char.GetUnicodeCategory(c1);

                            if (c1 >= 0x3400 && c1 <= 0x9fff &&
                                (cat0 == UnicodeCategory.LowercaseLetter ||
                                cat0 == UnicodeCategory.UppercaseLetter ||
                                cat0 == UnicodeCategory.DecimalDigitNumber ||
                                cat0 == UnicodeCategory.NonSpacingMark ||
                                ",.!%)]};:?'\"’”".Contains(c0)))
                                html += " ";
                            // c-e space
                            else if (c0 >= 0x3400 && c0 <= 0x9fff &&
                                (cat1 == UnicodeCategory.LowercaseLetter ||
                                cat1 == UnicodeCategory.UppercaseLetter ||
                                cat1 == UnicodeCategory.DecimalDigitNumber ||
                                cat1 == UnicodeCategory.NonSpacingMark ||
                                "%([{'\"‘“".Contains(c1)))
                                html += " ";
                        }

                        lastText = token;
                    }

                    switch (token.Type)
                    {
                        case TokenType.Text:
                            if (token.Text == " ") // from '\ '
                                html += "<span style=\"white-space:pre-wrap\"> </span>";
                            else if (token.Text == "$" || token.Text == "\\")
                                html += $"<span class=\"tex2jax_ignore\">{token.Text}</span>";
                            else
                                html += HttpUtility.HtmlEncode(token.Text);
                            break;
                        case TokenType.Whitespace:
                            // ignore space after cjk punctuation
                            if (last?.Type == TokenType.Text && last.Text.Length == 1 && IsCjkPunctuation(last.Text[0]))
                                break;

                            if (last?.Type != TokenType.Whitespace)
                                html += " ";
                            break;
                        case TokenType.HtmlTag:
                        case TokenType.Placeholder:
                            if (!singleLine)
                                html += token.Text; // raw html
                            break;
                        case TokenType.Tilde:
                            html += "&nbsp;";
                            break;
                        case TokenType.MathDelim:
                            if (singleLine)
                            {
                                switch (token.Text)
                                {
                                    case "\\[": html += "\\("; break;
                                    case "\\]": html += "\\)"; break;
                                    default: html += token.Text; break;
                                }
                            }
                            else
                                html += token.Text;
                            break;
                        case TokenType.Bookmark:
                            if (data == null)
                                break;
                            int id = int.Parse(token.Text);
                            foreach (var bookmark in data.Bookmarks)
                            {
                                if (bookmark.Id == id)
                                    html += $"<a id=\"{bookmark.Label}\" class=\"bookmark\"></a>";
                            }
                            break;
                        case TokenType.BeginGroup:
                        case TokenType.EndGroup:
                            break;
                        case TokenType.Reference:
                            bool flag = false;
                            string _ref = token.Text, text = null;
                            if (_ref.Contains(":TEXT:"))
                            {
                                int index = _ref.IndexOf(":TEXT:");
                                text = HttpUtility.HtmlEncode(_ref.Substring(index + 6));
                                _ref = _ref.Substring(0, index);
                            }
                            foreach (var bookmark in data.Bookmarks)
                                if (bookmark.Label == _ref)
                                {
                                    string content = ToHtmlFinal(bookmark.Content, ParserOptions.SingleLine);
                                    html += $"<a href=\"#{bookmark.Label}\">{text ?? content}</a>";
                                    flag = true;
                                    break;
                                }
                            if (flag) break;

                            if ((_ref.StartsWith("http://") || _ref.StartsWith("https://")) &&
                                Uri.IsWellFormedUriString(_ref, UriKind.Absolute))
                            {
                                html += "<a";
                                if (!(_ref + '/').StartsWith("https://www.bananaspace.net/"))
                                    html += " class=\"external-link\"";
                                html += $" href=\"{_ref}\">{text ?? _ref}</a>";
                                break;
                            }

                            html += $"<:REF:{HttpUtility.HtmlEncode(token.Text)}>";
                            break;
                        case TokenType.CodeSnippet:
                            bool isDisplayCode = token.Text.StartsWith("D/"), noColoring = token.Text.StartsWith("i/");
                            html += isDisplayCode ? "<pre class=\"paragraph code-snippet\">" :
                                noColoring ? "<code>" : "<code class=\"code-snippet\">";
                            html += HttpUtility.HtmlEncode(token.Text.Substring(2));
                            html += isDisplayCode ? "</pre>" : "</code>";
                            break;
                        default:
                            html += "<span style=\"color:red;font-weight:bold\">" + HttpUtility.HtmlEncode(token.ToString()) + "</span>";
                            break;
                    }
                }

                last = token;
            }

            try { html = html.Normalize(); }
            catch { }
            return html;
        }

        private static bool IsCjkPunctuation(char c)
            => (0x3000 <= c && c <= 0x301f) || (0xff00 <= c && c <= 0xff60);
    }

    [Flags]
    public enum ParserOptions
    {
        Default = 0,
        SingleLine = 1
    }
}
