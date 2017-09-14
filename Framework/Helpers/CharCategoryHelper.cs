using HakeCommand.Framework.Input.Internal;

namespace HakeCommand.Framework.Helpers
{
    internal static class CharCategoryHelper
    {
        public const char PIPE_CHAR = '|';
        public const char ESCAPE_CHAR = '`';

        public static bool IsWhitespace(this char ch)
        {
            return " \t\v\n\r".IndexOf(ch) >= 0;
        }
        public static bool IsValidFirstCharacter(this char ch)
        {
            if (ch <= 'Z' && ch >= 'A') return true;
            else if (ch <= '9' && ch >= '0') return true;
            else if (ch <= 'z' && ch >= 'a') return true;
            else if (ch == '/') return true;
            else if (ch == '.') return true;
            else if (ch == '$') return true;
            return false;
        }
        public static bool IsValidCharacter(this char ch)
        {
            if (ch <= 'Z' && ch >= 'A') return true;
            else if (ch <= '9' && ch >= '0') return true;
            else if (ch <= 'z' && ch >= 'a') return true;
            else if ("-_+*^&#@(){}[].:;/".IndexOf(ch) >= 0) return true;
            return false;
        }
        public static bool IsValidInput(this char ch)
        {
            if (ch == '"')
                return true;
            if (ch == PIPE_CHAR)
                return true;
            if (ch == ESCAPE_CHAR)
                return true;
            return IsWhitespace(ch) || IsValidFirstCharacter(ch) || IsValidCharacter(ch);
        }
    }
}
