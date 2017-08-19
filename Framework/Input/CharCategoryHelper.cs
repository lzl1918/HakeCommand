namespace HakeCommand.Framework
{
    internal static class CharCategoryHelper
    {
        public static bool IsWhiteSpace(this char ch)
        {
            return " \t\v\n\r".IndexOf(ch) >= 0;
        }
        public static bool IsVaildFirstCharacter(this char ch)
        {
            if (ch <= 'Z' && ch >= 'A') return true;
            else if (ch <= '9' && ch >= '0') return true;
            else if (ch <= 'z' && ch >= 'a') return true;
            else if (ch == '/') return true;
            else if (ch == '.') return true;
            else if (ch == '$') return true;
            return false;
        }
        public static bool IsChar(this char ch)
        {
            if (ch <= 'Z' && ch >= 'A') return true;
            else if (ch <= '9' && ch >= '0') return true;
            else if (ch <= 'z' && ch >= 'a') return true;
            else if ("-_+*^&#@(){}[].:;/".IndexOf(ch) >= 0) return true;
            return false;
        }
        public static bool IsValidChar(this char ch)
        {
            return ch == InternalInput.ESCAPE_CHAR || ch == '|' || IsWhiteSpace(ch) || IsVaildFirstCharacter(ch) || IsChar(ch);
        }
    }
}
