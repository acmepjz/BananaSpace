using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Banana.Text
{
    public class Token
    {
        public string Text { get; set; }
        public TokenType Type { get; set; }
        public bool VerbatimOutput { get; set; }

        public Token(string text, TokenType type)
        {
            Text = text;
            Type = type;
        }

        public override bool Equals(object obj)
        {
            return obj is Token && Text == ((Token)obj).Text && Type == ((Token)obj).Type;
        }

        public override int GetHashCode()
        {
            return Text == null ? Type.GetHashCode() : Text.GetHashCode() + Type.GetHashCode();
        }

        public static bool operator ==(Token t1, Token t2)
        {
            if (ReferenceEquals(t1, t2))
                return true;
            if (t1 is null)
                return false;
            return t1.Equals(t2);
        }

        public static bool operator !=(Token t1, Token t2)
        {
            return !(t1 == t2);
        }

        public override string ToString()
        {
            return
                Type == TokenType.BeginGroup ? "{" :
                Type == TokenType.EndGroup ? "}" :
                Type == TokenType.LineBreak ? "\\\\" :
                Type == TokenType.Subscript ? "_" :
                Type == TokenType.Superscript ? "^" :
                Type == TokenType.Tab ? "&" :
                Type == TokenType.Command ? "\\" + Text :
                Type == TokenType.Tilde ? "~" :
                Text ?? Type.ToString();
        }
    }

    public enum TokenType
    {
        Whitespace,
        BeginGroup,
        EndGroup,
        Superscript,
        Subscript,
        Tab,
        LineBreak,
        Command,
        Text,
        Argument,
        Tilde,
        MathDelim,
        HtmlTag,
        Reference,
        Bookmark
    }
}
