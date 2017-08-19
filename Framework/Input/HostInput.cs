using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Input
{
    public interface IHostInput
    {
        string ReadLine();
    }

    internal sealed class HostInput : IHostInput
    {
        public const ConsoleColor COMMAND_NAME_COLOR = ConsoleColor.White;
        public const ConsoleColor PARAMETER_NAME_COLOR = ConsoleColor.DarkGray;
        public const ConsoleColor PARAMETER_COLOR = ConsoleColor.Gray;
        public const ConsoleColor PIPE_SEPERATOR_COLOR = ConsoleColor.DarkCyan;
        public const ConsoleColor ESCAPE_COLOR = ConsoleColor.Yellow;
        public const ConsoleColor STRING_COLOR = ConsoleColor.Cyan;

        private int state;
        private Stack<int> stateStack;
        private StringBuilder textBuffer;

        public HostInput()
        {
            stateStack = new Stack<int>();
            textBuffer = new StringBuilder();
            state = 0;
        }

        public string ReadLine()
        {
            int left, top;
            while (true)
            {
                left = Console.CursorLeft;
                top = Console.CursorTop;
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.KeyChar == '\0')
                    continue;
                if (keyInfo.Key == ConsoleKey.Tab)
                {
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    left--;
                    if (left < 0)
                    {
                        top--;
                        left = Console.BufferWidth - 1;
                    }
                    if (top >= 0 && textBuffer.Length > 0)
                    {
                        textBuffer.Remove(textBuffer.Length - 1, 1);
                        Console.SetCursorPosition(left, top);
                        Console.Write(' ');
                        Console.SetCursorPosition(left, top);
                        state = stateStack.Pop();
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (CharCategoryHelper.IsValidChar(keyInfo.KeyChar))
                    textBuffer.Append(keyInfo.KeyChar);
            }
            string result = textBuffer.ToString();
            textBuffer.Clear();
            return result;
        }

        internal string ReadCommandLine()
        {
            int left, top;
            while (true)
            {
                left = Console.CursorLeft;
                top = Console.CursorTop;
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.KeyChar == '\0')
                    continue;
                if (keyInfo.Key == ConsoleKey.Tab)
                {
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    left--;
                    if (left < 0)
                    {
                        top--;
                        left = Console.BufferWidth - 1;
                    }
                    if (top >= 0 && textBuffer.Length > 0)
                    {
                        textBuffer.Remove(textBuffer.Length - 1, 1);
                        Console.SetCursorPosition(left, top);
                        Console.Write(' ');
                        Console.SetCursorPosition(left, top);
                        state = stateStack.Pop();
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (CharCategoryHelper.IsValidChar(keyInfo.KeyChar))
                {
                    if (OutputConsole(keyInfo.KeyChar))
                        textBuffer.Append(keyInfo.KeyChar);
                }
            }
            string result = textBuffer.ToString();
            textBuffer.Clear();
            state = 0;
            stateStack.Clear();
            return result;
        }

        private bool OutputConsole(char ch)
        {
            int oldstate = state;
            bool output = false;
            ConsoleColor outColor = Console.ForegroundColor;
            ConsoleColor originColor = outColor;
            if (state == 0)
            {
                if (ch.IsWhiteSpace()) { output = true; }
                else if (ch.IsVaildFirstCharacter()) { state = 1; output = true; outColor = COMMAND_NAME_COLOR; }
            }
            else if (state == 1)
            {
                if (ch.IsWhiteSpace()) { state = 2; output = true; }
                else if (ch == '|') { state = 0; output = true; outColor = PIPE_SEPERATOR_COLOR; }
                else if (ch.IsChar()) { output = true; outColor = COMMAND_NAME_COLOR; }
            }
            else if (state == 2)
            {
                if (ch.IsWhiteSpace()) { output = true; }
                else if (ch == '|') { state = 0; output = true; outColor = PIPE_SEPERATOR_COLOR; }
                else if (ch == '-' || ch == '/') { state = 3; output = true; outColor = PARAMETER_NAME_COLOR; }
                else if (ch == '"') { state = 6; output = true; outColor = STRING_COLOR; }
                else if (ch == InternalInput.ESCAPE_CHAR) { state = 5; output = true; outColor = ESCAPE_COLOR; }
                else if (ch.IsVaildFirstCharacter()) { state = 4; output = true; outColor = PARAMETER_COLOR; }
            }
            else if (state == 3)
            {
                if (ch.IsWhiteSpace()) { state = 2; output = true; }
                else if (ch == '|') { state = 0; output = true; outColor = PIPE_SEPERATOR_COLOR; }
                else if (ch.IsVaildFirstCharacter()) { state = 8; output = true; outColor = PARAMETER_NAME_COLOR; }
            }
            else if (state == 4)
            {
                if (ch == ';') { state = 9; output = true; outColor = PARAMETER_COLOR; }
                else if (ch == '|') { state = 0; output = true; outColor = PIPE_SEPERATOR_COLOR; }
                else if (ch == InternalInput.ESCAPE_CHAR) { state = 5; output = true; outColor = ESCAPE_COLOR; }
                else if (ch.IsWhiteSpace()) { state = 2; output = true; }
                else if (ch.IsChar()) { output = true; outColor = PARAMETER_COLOR; }
            }
            else if (state == 5)
            {
                state = 4;
                output = true;
                outColor = ESCAPE_COLOR;
            }
            else if (state == 6)
            {
                if (ch == '"') { state = 10; output = true; outColor = STRING_COLOR; }
                else if (ch == InternalInput.ESCAPE_CHAR) { state = 7; output = true; outColor = ESCAPE_COLOR; }
                else { output = true; outColor = STRING_COLOR; }
            }
            else if (state == 7)
            {
                state = 6;
                output = true;
                outColor = ESCAPE_COLOR;
            }
            else if (state == 8)
            {
                if (ch.IsWhiteSpace()) { state = 11; output = true; }
                else if (ch == '|') { state = 0; output = true; outColor = PIPE_SEPERATOR_COLOR; }
                else if (ch.IsChar()) { output = true; outColor = PARAMETER_NAME_COLOR; }
            }
            else if (state == 9)
            {
                if (ch.IsWhiteSpace()) { state = 2; output = true; }
                else if (ch == '|') { state = 0; output = true; outColor = PIPE_SEPERATOR_COLOR; }
                else if (ch == '"') { state = 15; output = true; outColor = STRING_COLOR; }
                // no duplicate ;
                else if (ch == ';') { output = false; }
                else if (ch == InternalInput.ESCAPE_CHAR) { state = 12; output = true; outColor = ESCAPE_COLOR; }
                else if (ch.IsVaildFirstCharacter()) { state = 13; output = true; outColor = PARAMETER_COLOR; }
            }
            else if (state == 10)
            {
                if (ch == ';') { state = 9; output = true; outColor = PARAMETER_COLOR; }
                else if (ch == '|') { state = 0; output = true; outColor = PIPE_SEPERATOR_COLOR; }
                else if (ch.IsWhiteSpace()) { state = 2; output = true; }
            }
            else if (state == 11)
            {
                if (ch.IsWhiteSpace()) { output = true; }
                else if (ch == '|') { state = 0; output = true; outColor = PIPE_SEPERATOR_COLOR; }
                else if (ch == '"') { state = 19; output = true; outColor = STRING_COLOR; }
                else if (ch == InternalInput.ESCAPE_CHAR) { state = 18; output = true; outColor = ESCAPE_COLOR; }
                else if (ch == '-' || ch == '/') { state = 3; output = true; outColor = PARAMETER_NAME_COLOR; }
                else if (ch.IsVaildFirstCharacter()) { state = 17; output = true; outColor = PARAMETER_COLOR; }
            }
            else if (state == 12)
            {
                state = 13;
                output = true;
                outColor = ESCAPE_COLOR;
            }
            else if (state == 13)
            {
                if (ch.IsWhiteSpace()) { state = 2; output = true; }
                else if (ch == '|') { state = 0; output = true; outColor = PIPE_SEPERATOR_COLOR; }
                else if (ch == ';') { state = 9; output = true; outColor = PARAMETER_COLOR; }
                else if (ch == InternalInput.ESCAPE_CHAR) { state = 12; output = true; outColor = ESCAPE_COLOR; }
                else if (ch.IsChar()) { output = true; outColor = PARAMETER_COLOR; }
            }
            else if (state == 14)
            {
                if (ch == ';') { state = 9; output = true; outColor = PARAMETER_COLOR; }
                else if (ch == '|') { state = 0; output = true; outColor = PIPE_SEPERATOR_COLOR; }
                else if (ch.IsWhiteSpace()) { state = 2; output = true; }
            }
            else if (state == 15)
            {
                if (ch == '"') { state = 14; output = true; outColor = STRING_COLOR; }
                else if (ch == InternalInput.ESCAPE_CHAR) { state = 16; output = true; outColor = ESCAPE_COLOR; }
                else { output = true; outColor = STRING_COLOR; }
            }
            else if (state == 16)
            {
                state = 15;
                output = true;
                outColor = ESCAPE_COLOR;
            }
            else if (state == 17)
            {
                if (ch.IsWhiteSpace()) { state = 2; output = true; }
                else if (ch == '|') { state = 0; output = true; outColor = PIPE_SEPERATOR_COLOR; }
                else if (ch == InternalInput.ESCAPE_CHAR) { state = 18; output = true; outColor = ESCAPE_COLOR; }
                else if (ch == ';') { state = 21; output = true; outColor = PARAMETER_COLOR; }
                else if (ch.IsChar()) { output = true; outColor = PARAMETER_COLOR; }
            }
            else if (state == 18)
            {
                state = 17;
                output = true;
                outColor = ESCAPE_COLOR;
            }
            else if (state == 19)
            {
                if (ch == InternalInput.ESCAPE_CHAR) { state = 20; output = true; outColor = ESCAPE_COLOR; }
                else if (ch == '"') { state = 22; output = true; outColor = STRING_COLOR; }
                else { output = true; outColor = STRING_COLOR; }
            }
            else if (state == 20)
            {
                state = 19;
                output = true;
                outColor = ESCAPE_COLOR;
            }
            else if (state == 21)
            {
                if (ch.IsWhiteSpace()) { state = 2; output = true; }
                else if (ch == '|') { state = 0; output = true; outColor = PIPE_SEPERATOR_COLOR; }
                else if (ch == '"') { state = 26; output = true; outColor = STRING_COLOR; }
                else if (ch == ';') { output = false; }
                else if (ch == InternalInput.ESCAPE_CHAR) { state = 23; output = true; outColor = ESCAPE_COLOR; }
                else if (ch.IsVaildFirstCharacter()) { state = 24; output = true; outColor = PARAMETER_COLOR; }
            }
            else if (state == 22)
            {
                if (ch == ';') { state = 21; output = true; outColor = PARAMETER_COLOR; }
                else if (ch == '|') { state = 0; output = true; outColor = PIPE_SEPERATOR_COLOR; }
                else if (ch.IsWhiteSpace()) { state = 2; output = true; }
            }
            else if (state == 23)
            {
                state = 24;
                output = true;
                outColor = ESCAPE_COLOR;
            }
            else if (state == 24)
            {
                if (ch.IsWhiteSpace()) { state = 2; output = true; }
                else if (ch == '|') { state = 0; output = true; outColor = PIPE_SEPERATOR_COLOR; }
                else if (ch == ';') { state = 21; output = true; outColor = PARAMETER_COLOR; }
                else if (ch == InternalInput.ESCAPE_CHAR) { state = 23; output = true; outColor = ESCAPE_COLOR; }
                else if (ch.IsChar()) { output = true; outColor = PARAMETER_COLOR; }
            }
            else if (state == 25)
            {
                if (ch == ';') { state = 21; output = true; outColor = PARAMETER_COLOR; }
                else if (ch == '|') { state = 0; output = true; outColor = PIPE_SEPERATOR_COLOR; }
                else if (ch.IsWhiteSpace()) { state = 2; output = true; }
            }
            else if (state == 26)
            {
                if (ch == '"') { state = 25; output = true; outColor = STRING_COLOR; }
                else if (ch == InternalInput.ESCAPE_CHAR) { state = 27; output = true; outColor = ESCAPE_COLOR; }
                else { output = true; outColor = STRING_COLOR; }
            }
            else if (state == 27)
            {
                state = 26;
                output = true;
                outColor = ESCAPE_COLOR;
            }

            if (output)
            {
                stateStack.Push(oldstate);
                Console.ForegroundColor = outColor;
                Console.Write(ch);
                Console.ForegroundColor = originColor;
            }
            return output;
        }
    }
}
