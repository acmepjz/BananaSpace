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
        public static string ToHtml(string s, UserPage page, ParserOptions options)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";

            var tokens = Expression.Parse(s);
            var data = Preamble.GetInitialExpansionData();

            // add \@secnum
            var secNum = new List<Token>();
            if (!string.IsNullOrWhiteSpace(page.SectionNumber))
            {
                foreach (char c in page.SectionNumber)
                    secNum.Add(new Token(c.ToString(), TokenType.Text));
            }
            var cd = new CommandDefinition("@secnum")
            {
                Patterns = { new List<Token>() },
                Definitions = { secNum }
            };
            data.Commands[cd.Name] = cd;

            Expression.ExpandSpecials(tokens, data);
            tokens = Expression.ExpandFinal(tokens, data);

            return ToHtml(tokens, options, data);
        }

        public static string ToHtml(List<Token> tokens, ParserOptions options, ExpansionData data = null)
        {
            string html = "";
            bool singleLine = options.HasFlag(ParserOptions.SingleLine);

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
                    // disable html tags in verbatim output (i.e. math mode)
                    else if (token.Type != TokenType.HtmlTag && token.Type != TokenType.Bookmark)
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
                            else
                                html += HttpUtility.HtmlEncode(token.Text);
                            break;
                        case TokenType.Whitespace:
                            if (last?.Type != TokenType.Whitespace)
                                html += " ";
                            break;
                        case TokenType.HtmlTag:
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
                                    html += $"<a id=\"{bookmark.Label}\"></a>";
                            }
                            break;
                        case TokenType.Reference:
                            bool flag = false;
                            foreach (var bookmark in data.Bookmarks)
                                if (bookmark.Label == token.Text)
                                {
                                    string content = ToHtml(bookmark.Content, ParserOptions.SingleLine);
                                    html += $"<a href=\"#{bookmark.Label}\">{content}</a>";
                                    flag = true;
                                    break;
                                }
                            if (flag) break;

                            html += "<span style=\"color:red;font-weight:bold\">??</span>";

                            break;
                        case TokenType.BeginGroup:
                        case TokenType.EndGroup:
                            break;
                        default:
                            html += "<span style=\"color:red;font-weight:bold\">" + HttpUtility.HtmlEncode(token.ToString()) + "</span>";
                            break;
                    }
                }

                last = token;
            }

            return html;
        }
    }

    [Flags]
    public enum ParserOptions
    {
        Default = 0,
        SingleLine = 1
    }
}
