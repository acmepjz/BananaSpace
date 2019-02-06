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
        public TextPosition Start { get; set; }
        public TextPosition End { get; set; }
        public Token Ancestor { get; set; }

        public Token(string text, TokenType type, TextPosition start, TextPosition end, Token ancestor = null)
        {
            Text = text;
            Type = type;
            Start = start;
            End = end;
            Ancestor = ancestor;
        }

        public Token(string text, TokenType type, Token ancestor = null)
        {
            Text = text;
            Type = type;
            Start = TextPosition.None;
            End = TextPosition.None;
            Ancestor = ancestor;
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

        public Token AddAncestor(Token ancestor) => 
            new Token(Text, Type, Start, End,
                Ancestor != null && Ancestor.Type == TokenType.Command && (Ancestor.Text == "begin" || Ancestor.Text == "end") ?
                Ancestor.AddAncestor(ancestor) : ancestor);
    }

    public struct TextPosition
    {
        public string FileName { get; set; }
        public int Line { get; set; }
        public int Ch { get; set; }

        public TextPosition(string fileName, int line, int ch)
        {
            FileName = fileName;
            Line = line;
            Ch = ch;
        }

        public static readonly TextPosition None = new TextPosition(null, -1, -1);
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
        Bookmark,
        Reference,
        Placeholder   // e.g. <:FILE:1.png>, to be replaced on request
    }
}
