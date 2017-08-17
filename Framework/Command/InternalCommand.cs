#if _DEBUG
#define TEST
#endif

using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Command
{
    internal static class CharCateHelper
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
            return false;
        }
        public static bool IsChar(this char ch)
        {
            if (ch <= 'Z' && ch >= 'A') return true;
            else if (ch <= '9' && ch >= '0') return true;
            else if (ch <= 'z' && ch >= 'a') return true;
            else if ("-_+*^&#@(){}[].|;/".IndexOf(ch) >= 0) return true;
            return false;
        }
    }


#if _DEBUG
    public
#else
    internal
#endif
    sealed class InternalCommand : ICommand
    {
        public string Raw { get; }
        public string Command { get; }
        public object[] Arguments { get; }
        public IReadOnlyDictionary<string, object> Options { get; }

        public bool ContainsError { get; }
        public string ErrorMessage { get; }

        private InternalCommand(string raw, string command, object[] arguments, IReadOnlyDictionary<string, object> options)
        {
            Raw = raw;
            Command = command;
            Arguments = arguments;
            Options = options;
            ContainsError = false;
            ErrorMessage = null;
        }
        private InternalCommand(string raw, string error)
        {
            Raw = raw;
            Command = "";
            Arguments = new object[0];
            Options = new Dictionary<string, object>();
            ErrorMessage = error;
            ContainsError = true;
        }

#if TEST
        public
#else
        internal
#endif
        static ICommand Parse(string input)
        {
            string raw = input;
            string command = "";
            string errorMessage = null;
            List<object> arguments = new List<object>();
            Dictionary<string, object> namedArgs = new Dictionary<string, object>();

            input = input.ToLower().Trim();
            int len = input.Length;
            StringBuilder valueBuilder = new StringBuilder(len);
            List<string> valueArray = new List<string>();
            string key = "";
            char ch;
            int index = 0;
            int state = 0;
            while (true)
            {
                if (index < len)
                    ch = input[index];
                else if (index == len)
                {
                    if (state == 0) break;
                    else if (state == 1) ch = ' ';
                    else if (state == 2) break;
                    else if (state == 3) break;
                    else if (state == 4) ch = ' ';
                    else if (state == 5) { ch = '\\'; index--; }
                    else if (state == 6) { ch = '"'; index--; }
                    else if (state == 7) { ch = '\\'; index--; }
                    else if (state == 8) { namedArgs[valueBuilder.ToString()] = true; valueBuilder.Clear(); break; }
                    else if (state == 9) ch = ' ';
                    else if (state == 10) ch = ' ';
                    else if (state == 11) { namedArgs[key] = true; break; }
                    else if (state == 12) { ch = '\\'; index--; }
                    else if (state == 13) ch = ' ';
                    else if (state == 14) ch = ' ';
                    else if (state == 15) { ch = '"'; index--; }
                    else if (state == 16) { ch = '\\'; index--; }
                    else if (state == 17) ch = ' ';
                    else if (state == 18) { ch = '\\'; index--; }
                    else if (state == 19) { ch = '"'; index--; }
                    else if (state == 20) { ch = '\\'; index--; }
                    else if (state == 21) ch = ' ';
                    else if (state == 22) ch = ' ';
                    else if (state == 23) { ch = '\\'; index--; }
                    else if (state == 24) ch = ' ';
                    else if (state == 25) ch = ' ';
                    else if (state == 26) { ch = '"'; index--; }
                    else if (state == 27) { ch = '\\'; index--; }
                    else
                        break;
                }
                else
                    break;

                if (state == 0)
                {
                    if (ch.IsWhiteSpace()) { }
                    else if (ch.IsVaildFirstCharacter()) { valueBuilder.Append(ch); state = 1; }
                    else { ProcessSyntaxError(); break; }
                }
                else if (state == 1)
                {
                    if (ch.IsWhiteSpace()) { command = valueBuilder.ToString(); valueBuilder.Clear(); state = 2; }
                    else if (ch.IsChar()) { valueBuilder.Append(ch); }
                    else { ProcessSyntaxError(); break; }
                }
                else if (state == 2)
                {
                    if (ch.IsWhiteSpace()) { }
                    else if (ch == '-' || ch == '/') { state = 3; }
                    else if (ch == '"') { state = 6; }
                    else if (ch == '\\') { state = 5; }
                    else if (ch.IsVaildFirstCharacter()) { valueBuilder.Append(ch); state = 4; }
                    else { ProcessSyntaxError(); break; }
                }
                else if (state == 3)
                {
                    if (ch.IsWhiteSpace()) { state = 2; }
                    else if (ch.IsVaildFirstCharacter()) { valueBuilder.Append(ch); state = 8; }
                    else { ProcessSyntaxError(); break; }
                }
                else if (state == 4)
                {
                    if (ch == ';') { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 9; }
                    else if (ch == '\\') { state = 5; }
                    else if (ch.IsWhiteSpace()) { arguments.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 2; }
                    else if (ch.IsChar()) { valueBuilder.Append(ch); }
                    else { ProcessSyntaxError(); break; }
                }
                else if (state == 5)
                {
                    valueBuilder.Append(ch);
                    state = 4;
                }
                else if (state == 6)
                {
                    if (ch == '"') { state = 10; }
                    else if (ch == '\\') { state = 7; }
                    else { valueBuilder.Append(ch); }
                }
                else if (state == 7)
                {
                    valueBuilder.Append(ch);
                    state = 6;
                }
                else if (state == 8)
                {
                    if (ch.IsWhiteSpace()) { key = valueBuilder.ToString(); valueBuilder.Clear(); state = 11; }
                    else if (ch.IsChar()) { valueBuilder.Append(ch); }
                    else { ProcessSyntaxError(); break; }
                }
                else if (state == 9)
                {
                    if (ch.IsWhiteSpace())
                    {
                        if (valueArray.Count == 1) { arguments.Add(valueArray[0]); }
                        else { arguments.Add(valueArray.CreateCopy()); }
                        valueArray.Clear();
                        state = 2;
                    }
                    else if (ch == '"') { state = 15; }
                    else if (ch == ';') { }
                    else if (ch == '\\') { state = 12; }
                    else if (ch.IsVaildFirstCharacter()) { valueBuilder.Append(ch); state = 13; }
                    else { ProcessSyntaxError(); break; }
                }
                else if (state == 10)
                {
                    if (ch == ';') { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 9; }
                    else if (ch.IsWhiteSpace()) { arguments.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 2; }
                    else { ProcessSyntaxError(); break; }
                }
                else if (state == 11)
                {
                    if (ch.IsWhiteSpace()) { }
                    else if (ch == '"') { state = 19; }
                    else if (ch == '\\') { state = 18; }
                    else if (ch == '-' || ch == '/') { namedArgs[key] = true; state = 3; }
                    else if (ch.IsVaildFirstCharacter()) { valueBuilder.Append(ch); state = 17; }
                    else { ProcessSyntaxError(); break; }
                }
                else if (state == 12)
                {
                    valueBuilder.Append(ch);
                    state = 13;
                }
                else if (state == 13)
                {
                    if (ch.IsWhiteSpace()) { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); arguments.Add(valueArray.CreateCopy()); valueArray.Clear(); state = 2; }
                    else if (ch == ';') { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 9; }
                    else if (ch == '\\') { state = 12; }
                    else if (ch.IsChar()) { valueBuilder.Append(ch); }
                    else { ProcessSyntaxError(); break; }
                }
                else if (state == 14)
                {
                    if (ch == ';') { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 9; }
                    else if (ch.IsWhiteSpace()) { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); arguments.Add(valueArray.CreateCopy()); valueArray.Clear(); state = 2; }
                    else { ProcessSyntaxError(); break; }
                }
                else if (state == 15)
                {
                    if (ch == '"') { state = 14; }
                    else if (ch == '\\') { state = 16; }
                    else { valueBuilder.Append(ch); }
                }
                else if (state == 16)
                {
                    valueBuilder.Append(ch);
                    state = 15;
                }
                else if (state == 17)
                {
                    if (ch.IsWhiteSpace()) { namedArgs[key] = valueBuilder.ToString(); valueBuilder.Clear(); state = 2; }
                    else if (ch == '\\') { state = 18; }
                    else if (ch == ';') { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 21; }
                    else if (ch.IsChar()) { valueBuilder.Append(ch); }
                    else { ProcessSyntaxError(); break; }
                }
                else if (state == 18)
                {
                    valueBuilder.Append(ch);
                    state = 17;
                }
                else if (state == 19)
                {
                    if (ch == '\\') { state = 20; }
                    else if (ch == '"') { state = 22; }
                    else { valueBuilder.Append(ch); }
                }
                else if (state == 20)
                {
                    valueBuilder.Append(ch);
                    state = 19;
                }
                else if (state == 21)
                {
                    if (ch.IsWhiteSpace())
                    {
                        if (valueArray.Count == 1) { namedArgs[key] = valueArray[0]; }
                        else { namedArgs[key] = valueArray.CreateCopy(); }
                        valueArray.Clear();
                        state = 2;
                    }
                    else if (ch == '"') { state = 26; }
                    else if (ch == ';') { }
                    else if (ch == '\\') { state = 23; }
                    else if (ch.IsVaildFirstCharacter()) { valueBuilder.Append(ch); state = 24; }
                    else { ProcessSyntaxError(); break; }
                }
                else if (state == 22)
                {
                    if (ch == ';') { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 21; }
                    else if (ch.IsWhiteSpace()) { namedArgs[key] = valueBuilder.ToString(); valueBuilder.Clear(); state = 2; }
                    else { ProcessSyntaxError(); break; }
                }
                else if (state == 23)
                {
                    valueBuilder.Append(ch);
                    state = 24;
                }
                else if (state == 24)
                {
                    if (ch.IsWhiteSpace()) { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); namedArgs[key] = valueArray.CreateCopy(); valueArray.Clear(); state = 2; }
                    else if (ch == ';') { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 21; }
                    else if (ch == '\\') { state = 23; }
                    else if (ch.IsChar()) { valueBuilder.Append(ch); }
                    else { ProcessSyntaxError(); break; }
                }
                else if (state == 25)
                {
                    if (ch == ';') { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 21; }
                    else if (ch.IsWhiteSpace()) { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); namedArgs[key] = valueArray.CreateCopy(); valueArray.Clear(); state = 2; }
                    else { ProcessSyntaxError(); break; }
                }
                else if (state == 26)
                {
                    if (ch == '"') { state = 25; }
                    else if (ch == '\\') { state = 27; }
                    else { valueBuilder.Append(ch); }
                }
                else if (state == 27)
                {
                    valueBuilder.Append(ch);
                    state = 26;
                }
                else
                    throw new Exception($"unknow state {state}");

                index++;
            }
            InternalCommand cmd = null;
            if (errorMessage == null)
                cmd = new InternalCommand(raw, command, arguments.ToArray(), namedArgs);
            else
                cmd = new InternalCommand(raw, errorMessage);
            return cmd;

            void ProcessSyntaxError()
            {
                string prefix = raw.Substring(0, index);
                string post = raw.Substring(index);
                StringBuilder builder = new StringBuilder(raw.Length * 2);
                builder.AppendLine($"syntax error at {index}:");
                builder.AppendLine(raw);
                for (int i = 0; i < index; i++)
                    builder.Append(' ');
                builder.Append('^');
                builder.AppendLine();
                errorMessage = builder.ToString();

            }
        }
        
    }

    internal static class ListHelpers
    {
        public static List<string> CreateCopy(this List<string> array)
        {
            return new List<string>(array);
        }
    }
}
