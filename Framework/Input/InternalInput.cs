using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework
{
#if _DEBUG
    public
#else
    internal
#endif
    sealed class InternalInput : IInput
    {
        internal const char ESCAPE_CHAR = '`';

        public string Name { get; }
        public object[] Arguments { get; }
        public IReadOnlyDictionary<string, object> Options { get; }

        public bool ContainsError { get; }
        public string ErrorMessage { get; }

        private InternalInput(string name, object[] arguments, IReadOnlyDictionary<string, object> options)
        {
            Name = name;
            Arguments = arguments;
            Options = options;
            ContainsError = false;
            ErrorMessage = null;
        }
        private InternalInput(string error)
        {
            Name = "";
            Arguments = new object[0];
            Options = new Dictionary<string, object>();
            ErrorMessage = error;
            ContainsError = true;
        }

#if _DEBUG
        public
#else
        internal
#endif
        static IInputCollection Parse(string input)
        {
            string raw = input;
            string name = "";
            string errorMessage = null;
            List<object> arguments = new List<object>();
            Dictionary<string, object> options = new Dictionary<string, object>();

            IInput iinput;
            List<IInput> inputs = new List<IInput>();
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
                    if (state == 0)
                    { break; }
                    else if (state == 1)
                    { ch = ' '; }
                    else if (state == 2)
                    { break; }
                    else if (state == 3)
                    { break; }
                    else if (state == 4)
                    { ch = ' '; }
                    else if (state == 5)
                    { ch = '\\'; index--; }
                    else if (state == 6)
                    { ch = '"'; index--; }
                    else if (state == 7)
                    { ch = '\\'; index--; }
                    else if (state == 8)
                    { options[valueBuilder.ToString()] = true; valueBuilder.Clear(); break; }
                    else if (state == 9)
                    { ch = ' '; }
                    else if (state == 10)
                    { ch = ' '; }
                    else if (state == 11)
                    { options[key] = true; break; }
                    else if (state == 12)
                    { ch = '\\'; index--; }
                    else if (state == 13)
                    { ch = ' '; }
                    else if (state == 14)
                    { ch = ' '; }
                    else if (state == 15)
                    { ch = '"'; index--; }
                    else if (state == 16)
                    { ch = '\\'; index--; }
                    else if (state == 17)
                    { ch = ' '; }
                    else if (state == 18)
                    { ch = '\\'; index--; }
                    else if (state == 19)
                    { ch = '"'; index--; }
                    else if (state == 20)
                    { ch = '\\'; index--; }
                    else if (state == 21)
                    { ch = ' '; }
                    else if (state == 22)
                    { ch = ' '; }
                    else if (state == 23)
                    { ch = '\\'; index--; }
                    else if (state == 24)
                    { ch = ' '; }
                    else if (state == 25)
                    { ch = ' '; }
                    else if (state == 26)
                    { ch = '"'; index--; }
                    else if (state == 27)
                    { ch = '\\'; index--; }
                    else
                        break;
                }
                else
                    break;

                if (state == 0)
                {
                    if (ch.IsWhiteSpace())
                    { }
                    else if (ch.IsVaildFirstCharacter())
                    { valueBuilder.Append(ch); state = 1; }
                    else
                    { ProcessSyntaxError(); break; }
                }
                else if (state == 1)
                {
                    if (ch.IsWhiteSpace())
                    { name = valueBuilder.ToString(); valueBuilder.Clear(); state = 2; }
                    else if (ch == '|')
                    { name = valueBuilder.ToString(); valueBuilder.Clear(); iinput = new InternalInput(name, arguments.ToArray(), new Dictionary<string, object>(options)); arguments.Clear(); options.Clear(); inputs.Add(iinput); name = ""; state = 0; }
                    else if (ch.IsChar())
                    { valueBuilder.Append(ch); }
                    else
                    { ProcessSyntaxError(); break; }
                }
                else if (state == 2)
                {
                    if (ch.IsWhiteSpace())
                    { }
                    else if (ch == '|')
                    { iinput = new InternalInput(name, arguments.ToArray(), new Dictionary<string, object>(options)); arguments.Clear(); options.Clear(); inputs.Add(iinput); name = ""; state = 0; }
                    else if (ch == '-' || ch == '/')
                    { state = 3; }
                    else if (ch == '"')
                    { state = 6; }
                    else if (ch == ESCAPE_CHAR)
                    { state = 5; }
                    else if (ch.IsVaildFirstCharacter())
                    { valueBuilder.Append(ch); state = 4; }
                    else
                    { ProcessSyntaxError(); break; }
                }
                else if (state == 3)
                {
                    if (ch.IsWhiteSpace())
                    { state = 2; }
                    else if (ch == '|')
                    { iinput = new InternalInput(name, arguments.ToArray(), new Dictionary<string, object>(options)); arguments.Clear(); options.Clear(); inputs.Add(iinput); name = ""; state = 0; }
                    else if (ch.IsVaildFirstCharacter())
                    { valueBuilder.Append(ch); state = 8; }
                    else
                    { ProcessSyntaxError(); break; }
                }
                else if (state == 4)
                {
                    if (ch == ';')
                    { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 9; }
                    else if (ch == '|')
                    { arguments.Add(valueBuilder.ToString()); valueBuilder.Clear(); iinput = new InternalInput(name, arguments.ToArray(), new Dictionary<string, object>(options)); arguments.Clear(); options.Clear(); inputs.Add(iinput); name = ""; state = 0; }
                    else if (ch == ESCAPE_CHAR)
                    { state = 5; }
                    else if (ch.IsWhiteSpace())
                    { arguments.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 2; }
                    else if (ch.IsChar())
                    { valueBuilder.Append(ch); }
                    else
                    { ProcessSyntaxError(); break; }
                }
                else if (state == 5)
                {
                    valueBuilder.Append(ch);
                    state = 4;
                }
                else if (state == 6)
                {
                    if (ch == '"')
                    { state = 10; }
                    else if (ch == ESCAPE_CHAR)
                    { state = 7; }
                    else
                    { valueBuilder.Append(ch); }
                }
                else if (state == 7)
                {
                    valueBuilder.Append(ch);
                    state = 6;
                }
                else if (state == 8)
                {
                    if (ch.IsWhiteSpace())
                    { key = valueBuilder.ToString(); valueBuilder.Clear(); state = 11; }
                    else if (ch == '|')
                    { key = valueBuilder.ToString(); valueBuilder.Clear(); options.Add(key, true); iinput = new InternalInput(name, arguments.ToArray(), new Dictionary<string, object>(options)); arguments.Clear(); options.Clear(); inputs.Add(iinput); name = ""; state = 0; }
                    else if (ch.IsChar())
                    { valueBuilder.Append(ch); }
                    else
                    { ProcessSyntaxError(); break; }
                }
                else if (state == 9)
                {
                    if (ch.IsWhiteSpace())
                    {
                        if (valueArray.Count == 1) { arguments.Add(valueArray[0]); }
                        else { arguments.Add(valueArray.ToArray()); }
                        valueArray.Clear();
                        state = 2;
                    }
                    else if (ch == '|')
                    {
                        if (valueArray.Count == 1) { arguments.Add(valueArray[0]); }
                        else { arguments.Add(valueArray.ToArray()); }
                        valueArray.Clear();
                        iinput = new InternalInput(name, arguments.ToArray(), new Dictionary<string, object>(options)); arguments.Clear(); options.Clear(); inputs.Add(iinput); name = ""; state = 0;
                    }
                    else if (ch == '"')
                    { state = 15; }
                    else if (ch == ';')
                    { }
                    else if (ch == ESCAPE_CHAR)
                    { state = 12; }
                    else if (ch.IsVaildFirstCharacter())
                    { valueBuilder.Append(ch); state = 13; }
                    else
                    { ProcessSyntaxError(); break; }
                }
                else if (state == 10)
                {
                    if (ch == ';')
                    { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 9; }
                    else if (ch == '|')
                    { arguments.Add(valueBuilder.ToString()); valueBuilder.Clear(); iinput = new InternalInput(name, arguments.ToArray(), new Dictionary<string, object>(options)); arguments.Clear(); options.Clear(); inputs.Add(iinput); name = ""; state = 0; }
                    else if (ch.IsWhiteSpace())
                    { arguments.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 2; }
                    else
                    { ProcessSyntaxError(); break; }
                }
                else if (state == 11)
                {
                    if (ch.IsWhiteSpace())
                    { }
                    else if (ch == '|')
                    { options.Add(key, true); iinput = new InternalInput(name, arguments.ToArray(), new Dictionary<string, object>(options)); arguments.Clear(); options.Clear(); inputs.Add(iinput); name = ""; state = 0; }
                    else if (ch == '"')
                    { state = 19; }
                    else if (ch == ESCAPE_CHAR)
                    { state = 18; }
                    else if (ch == '-' || ch == '/')
                    { options[key] = true; state = 3; }
                    else if (ch.IsVaildFirstCharacter())
                    { valueBuilder.Append(ch); state = 17; }
                    else
                    { ProcessSyntaxError(); break; }
                }
                else if (state == 12)
                {
                    valueBuilder.Append(ch);
                    state = 13;
                }
                else if (state == 13)
                {
                    if (ch.IsWhiteSpace())
                    { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); arguments.Add(valueArray.ToArray()); valueArray.Clear(); state = 2; }
                    else if (ch == '|')
                    { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); arguments.Add(valueArray.ToArray()); valueArray.Clear(); iinput = new InternalInput(name, arguments.ToArray(), new Dictionary<string, object>(options)); arguments.Clear(); options.Clear(); inputs.Add(iinput); name = ""; state = 0; }
                    else if (ch == ';')
                    { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 9; }
                    else if (ch == ESCAPE_CHAR)
                    { state = 12; }
                    else if (ch.IsChar())
                    { valueBuilder.Append(ch); }
                    else
                    { ProcessSyntaxError(); break; }
                }
                else if (state == 14)
                {
                    if (ch == ';')
                    { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 9; }
                    else if (ch == '|')
                    { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); arguments.Add(valueArray.ToArray()); valueArray.Clear(); iinput = new InternalInput(name, arguments.ToArray(), new Dictionary<string, object>(options)); arguments.Clear(); options.Clear(); inputs.Add(iinput); name = ""; state = 0; }
                    else if (ch.IsWhiteSpace())
                    { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); arguments.Add(valueArray.ToArray()); valueArray.Clear(); state = 2; }
                    else
                    { ProcessSyntaxError(); break; }
                }
                else if (state == 15)
                {
                    if (ch == '"')
                    { state = 14; }
                    else if (ch == ESCAPE_CHAR)
                    { state = 16; }
                    else
                    { valueBuilder.Append(ch); }
                }
                else if (state == 16)
                {
                    valueBuilder.Append(ch);
                    state = 15;
                }
                else if (state == 17)
                {
                    if (ch.IsWhiteSpace())
                    { options[key] = valueBuilder.ToString(); valueBuilder.Clear(); state = 2; }
                    else if (ch == '|')
                    { options[key] = valueBuilder.ToString(); valueBuilder.Clear(); iinput = new InternalInput(name, arguments.ToArray(), new Dictionary<string, object>(options)); arguments.Clear(); options.Clear(); inputs.Add(iinput); name = ""; state = 0; }
                    else if (ch == ESCAPE_CHAR)
                    { state = 18; }
                    else if (ch == ';')
                    { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 21; }
                    else if (ch.IsChar())
                    { valueBuilder.Append(ch); }
                    else
                    { ProcessSyntaxError(); break; }
                }
                else if (state == 18)
                {
                    valueBuilder.Append(ch);
                    state = 17;
                }
                else if (state == 19)
                {
                    if (ch == ESCAPE_CHAR)
                    { state = 20; }
                    else if (ch == '"')
                    { state = 22; }
                    else
                    { valueBuilder.Append(ch); }
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
                        if (valueArray.Count == 1) { options[key] = valueArray[0]; }
                        else { options[key] = valueArray.ToArray(); }
                        valueArray.Clear();
                        state = 2;
                    }
                    else if (ch == '|')
                    {
                        if (valueArray.Count == 1) { options[key] = valueArray[0]; }
                        else { options[key] = valueArray.ToArray(); }
                        valueArray.Clear();
                        iinput = new InternalInput(name, arguments.ToArray(), new Dictionary<string, object>(options)); arguments.Clear(); options.Clear(); inputs.Add(iinput); name = ""; state = 0;
                    }
                    else if (ch == '"')
                    { state = 26; }
                    else if (ch == ';')
                    { }
                    else if (ch == ESCAPE_CHAR)
                    { state = 23; }
                    else if (ch.IsVaildFirstCharacter())
                    { valueBuilder.Append(ch); state = 24; }
                    else
                    { ProcessSyntaxError(); break; }
                }
                else if (state == 22)
                {
                    if (ch == ';')
                    { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 21; }
                    else if (ch == '|')
                    { options[key] = valueBuilder.ToString(); valueBuilder.Clear(); iinput = new InternalInput(name, arguments.ToArray(), new Dictionary<string, object>(options)); arguments.Clear(); options.Clear(); inputs.Add(iinput); name = ""; state = 0; }
                    else if (ch.IsWhiteSpace())
                    { options[key] = valueBuilder.ToString(); valueBuilder.Clear(); state = 2; }
                    else
                    { ProcessSyntaxError(); break; }
                }
                else if (state == 23)
                {
                    valueBuilder.Append(ch);
                    state = 24;
                }
                else if (state == 24)
                {
                    if (ch.IsWhiteSpace())
                    { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); options[key] = valueArray.ToArray(); valueArray.Clear(); state = 2; }
                    else if (ch == '|')
                    { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); options[key] = valueArray.ToArray(); valueArray.Clear(); iinput = new InternalInput(name, arguments.ToArray(), new Dictionary<string, object>(options)); arguments.Clear(); options.Clear(); inputs.Add(iinput); name = ""; state = 0; }
                    else if (ch == ';')
                    { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 21; }
                    else if (ch == ESCAPE_CHAR)
                    { state = 23; }
                    else if (ch.IsChar())
                    { valueBuilder.Append(ch); }
                    else
                    { ProcessSyntaxError(); break; }
                }
                else if (state == 25)
                {
                    if (ch == ';')
                    { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); state = 21; }
                    else if (ch == '|')
                    { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); options[key] = valueArray.ToArray(); valueArray.Clear(); iinput = new InternalInput(name, arguments.ToArray(), new Dictionary<string, object>(options)); arguments.Clear(); options.Clear(); inputs.Add(iinput); name = ""; state = 0; }
                    else if (ch.IsWhiteSpace())
                    { valueArray.Add(valueBuilder.ToString()); valueBuilder.Clear(); options[key] = valueArray.ToArray(); valueArray.Clear(); state = 2; }
                    else
                    { ProcessSyntaxError(); break; }
                }
                else if (state == 26)
                {
                    if (ch == '"')
                    { state = 25; }
                    else if (ch == ESCAPE_CHAR)
                    { state = 27; }
                    else
                    { valueBuilder.Append(ch); }
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

            InternalInputCollection inputResult = null;
            if (errorMessage == null)
            {
                if (name.Length > 0)
                {
                    iinput = new InternalInput(name, arguments.ToArray(), options);
                    inputs.Add(iinput);
                }
                inputResult = new InternalInputCollection(raw, inputs);
            }

            else
                inputResult = new InternalInputCollection(raw, errorMessage);
            return inputResult;

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
}
