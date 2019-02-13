using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Banana.Text
{
    public class TextException : Exception
    {
        public Token Token { get; set; }

        private TextException(string message, Token token) : base(message)
        {
            Token = token;
        }

        private static string Flatten(string s)
        {
            if (s == null) return null;
            s = s.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", " \u21b2 ");
            if (s.Length > 80) s = s.Substring(0, 65) + " ... " + s.Substring(s.Length - 10);
            return s;
        }

        public static TextException CustomMessage(Token token, string message) => new TextException(Flatten(message), token);
        public static TextException EmptyCsName(Token token) => new TextException("命令名称不能为空。", token);
        public static TextException InvalidCharCode(Token token, string parameter) => new TextException($"'{Flatten(parameter)}' 不是有效的字符编码。", token);
        public static TextException InvalidInMathMode(Token token) => new TextException("此命令无法在数学公式中使用。", token);
        public static TextException InvalidLabelName(Token token, string parameter) => new TextException($"'{Flatten(parameter)}' 不是合法的书签名。书签名只能由字母、数字和 '-' 构成。", token);
        public static TextException InvalidMacroName(Token token, string parameter) => new TextException($"'{Flatten(parameter)}' 不是合法的宏名称。", token);
        public static TextException InvalidVariableName(Token token, string parameter) => new TextException($"'{Flatten(parameter)}' 不是合法的变量名。变量名只能由字母、数字和 '-' 构成。", token);
        public static TextException MaxBufferExceeded(Token token, int tokens) => new TextException($"宏展开使得代码长度超过了最大限制 ({tokens})。代码中可能存在死循环。", token);
        public static TextException MaxExpansionsExceeded(Token token, int expansions) => new TextException($"宏展开的次数超过了最大限制 ({expansions})。代码中可能存在死循环。", token);
        public static TextException NoMatchingDefinition(Token token) => new TextException($"'{Flatten(token.ToString())}' 的用法和它的定义不匹配。", token);
        public static TextException PlainStringExpected(Token token) => new TextException("参数只能由纯文字组成。", token);
        public static TextException TextTooLong(int length) => new TextException($"代码过长。一个页面最多只能包含 {length} 个字符。代码没有保存！", null);
        public static TextException TokenExpected(Token token) => new TextException("在读取参数时遇到了 '}' 或文档结尾。", token);
        public static TextException TokenExpected(Token token, string expected) => new TextException($"需要一个 '{expected}'，但是它没有出现。", token);
        public static TextException UndefinedCommand(Token token) => new TextException($"命令 '{token}' 没有定义。", token);
        public static TextException UnexpectedToken(Token token) => new TextException($"'{token}' 不应该出现在这里。", token);
        public static TextException UnmatchedBracket(Token token, string bracket) => new TextException($"'{bracket}' 没有配对。", token);

        public string GetMessage()
        {
            string s = "";
            var token = Token;

            while (token != null)
            {
                s += $"at '{token.ToString()}'";
                if (token.Start.FileName == "user")
                    s += $" (行 {token.Start.Line + 1}, 字符 {token.Start.Ch})";
                else if (token.Start.FileName == "preamble")
                    s += $" (内置: 行 {token.Start.Line + 1}, 字符 {token.Start.Ch})";
                s += "\r\n";
                token = token.Ancestor;
            }

            s = s == "" ? Message : Message + "\r\n\r\n" + s;
            return s;
        }
    }
}
