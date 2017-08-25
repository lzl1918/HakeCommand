using HakeCommand.Framework.Services.HistoryProvider;
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
        private Stack<char> postStack;
        private Stack<int> stateStack;
        private StringBuilder textBuffer;
        private IHistoryProvider historyProvider;
        private string lastInputCache;
        private int position;
        private int lastPosition;

        public HostInput(IHistoryProvider historyProvider)
        {
            stateStack = new Stack<int>();
            textBuffer = new StringBuilder();
            postStack = new Stack<char>();
            state = 0;
            position = 0;
            this.historyProvider = historyProvider;
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
                {
                    Console.Write(keyInfo.KeyChar);
                    textBuffer.Append(keyInfo.KeyChar);
                }
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
                if (keyInfo.Key == ConsoleKey.Tab)
                {
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    RemoveChar(ref left, ref top, ref position);
                    lastInputCache = null;
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    string command = historyProvider.NextHistory();
                    if (command != null)
                    {
                        if (lastInputCache == null)
                        {
                            lastInputCache = textBuffer.ToString();
                            lastPosition = position;
                        }
                        int length = textBuffer.Length;
                        MoveToEnd(ref left, ref top);
                        while (length > 0)
                        {
                            RemoveChar(ref left, ref top, ref position);
                            length--;
                        }
                        state = 0;
                        position = 0;
                        foreach (char commandChar in command)
                        {
                            if (OutputConsole(commandChar))
                            {
                                textBuffer.Append(commandChar);
                                position++;
                            }
                        }
                    }
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    string command = historyProvider.PreviousHistory();
                    if (command != null)
                    {
                        if (lastInputCache == null)
                        {
                            lastInputCache = textBuffer.ToString();
                            lastPosition = position;
                        }
                        int length = textBuffer.Length;
                        MoveToEnd(ref left, ref top);
                        while (length > 0)
                        {
                            RemoveChar(ref left, ref top, ref position);
                            length--;
                        }
                        state = 0;
                        position = 0;
                        foreach (char commandChar in command)
                        {
                            if (OutputConsole(commandChar))
                            {
                                textBuffer.Append(commandChar);
                                position++;
                            }
                        }
                    }
                    else
                    {
                        command = lastInputCache;
                        if (command != null)
                        {
                            int length = textBuffer.Length;
                            MoveToEnd(ref left, ref top);
                            while (length > 0)
                            {
                                RemoveChar(ref left, ref top, ref position);
                                length--;
                            }
                            state = 0;
                            position = 0;
                            int setleft = 0;
                            int settop = 0;
                            foreach (char commandChar in command)
                            {
                                if (OutputConsole(commandChar))
                                {
                                    textBuffer.Append(commandChar);
                                    if (position == lastPosition)
                                    {
                                        setleft = Console.CursorLeft;
                                        settop = Console.CursorTop;
                                    }
                                    position++;
                                }
                            }
                            if (position == lastPosition)
                            {
                                setleft = Console.CursorLeft;
                                settop = Console.CursorTop;
                            }
                            Console.SetCursorPosition(setleft, settop);
                        }
                    }
                }
                else if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    if (position <= 0)
                        continue;
                    left--;
                    if (left < 0)
                    {
                        top--;
                        left = Console.BufferWidth - 1;
                    }
                    Console.SetCursorPosition(left, top);
                    position--;
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    if (position >= textBuffer.Length)
                        continue;
                    left++;
                    if (left >= Console.BufferWidth)
                    {
                        top++;
                        left = 0;
                    }
                    Console.SetCursorPosition(left, top);
                    position++;
                }
                else if (keyInfo.Key == ConsoleKey.Delete)
                {
                    RemoveNextChar(ref left, ref top, ref position);
                    lastInputCache = null;
                }
                else if (keyInfo.Key == ConsoleKey.Home)
                {
                    MoveToHome(ref left, ref top);
                }
                else if (keyInfo.Key == ConsoleKey.End)
                {
                    MoveToEnd(ref left, ref top);
                }
                else if (keyInfo.KeyChar == '\0')
                    continue;
                else if (CharCategoryHelper.IsValidChar(keyInfo.KeyChar))
                {
                    int stackSize = stateStack.Count;
                    int current = stackSize;
                    while (current > position)
                    {
                        state = stateStack.Pop();
                        current--;
                    }

                    if (OutputConsole(keyInfo.KeyChar))
                    {
                        current = stackSize - 1;
                        while (current >= position)
                        {
                            postStack.Push(textBuffer[current]);
                            current--;
                        }
                        if (postStack.Count > 0)
                            textBuffer.Remove(position, stackSize - position);
                        int currentLeft = Console.CursorLeft;
                        int currentTop = Console.CursorTop;
                        textBuffer.Append(keyInfo.KeyChar);
                        position++;
                        lastInputCache = null;

                        int len = postStack.Count;
                        char ch;
                        int emptyCount = 0;
                        while (len > 0)
                        {
                            ch = postStack.Pop();
                            if (OutputConsole(ch))
                                textBuffer.Append(ch);
                            else
                                emptyCount++;
                            len--;
                        }
                        while (emptyCount > 0)
                        {
                            Console.Write(' ');
                            emptyCount--;
                        }
                        Console.SetCursorPosition(currentLeft, currentTop);
                    }
                }
            }
            string result = textBuffer.ToString();
            textBuffer.Clear();
            state = 0;
            position = 0;
            lastPosition = 0;
            lastInputCache = null;
            stateStack.Clear();
            return result;
        }
        private void RemoveChar(ref int left, ref int top, ref int position)
        {
            if (position <= 0)
                return;

            left--;
            if (left < 0)
            {
                top--;
                left = Console.BufferWidth - 1;
            }
            if (top >= 0 && textBuffer.Length > 0)
            {
                position--;
                int stackSize = stateStack.Count;
                int current = stackSize - 1;
                while (current >= position)
                {
                    state = stateStack.Pop();
                    postStack.Push(textBuffer[current]);
                    current--;
                }
                postStack.Pop();
                textBuffer.Remove(position, stackSize - position);
                Console.SetCursorPosition(left, top);
                int len = postStack.Count;
                char ch;
                int emptyCount = 0;
                while (len > 0)
                {
                    ch = postStack.Pop();
                    if (OutputConsole(ch))
                        textBuffer.Append(ch);
                    else
                        emptyCount++;
                    len--;
                }
                while (emptyCount > 0)
                {
                    Console.Write(' ');
                    emptyCount--;
                }
                Console.Write(' ');
                Console.SetCursorPosition(left, top);
            }
        }
        private void RemoveNextChar(ref int left, ref int top, ref int position)
        {
            if (position >= textBuffer.Length || textBuffer.Length <= 0)
                return;

            int stackSize = stateStack.Count;
            int current = stackSize - 1;
            while (current >= position)
            {
                state = stateStack.Pop();
                postStack.Push(textBuffer[current]);
                current--;
            }
            postStack.Pop();
            textBuffer.Remove(position, stackSize - position);
            int len = postStack.Count;
            char ch;
            int emptyCount = 0;
            while (len > 0)
            {
                ch = postStack.Pop();
                if (OutputConsole(ch))
                    textBuffer.Append(ch);
                else
                    emptyCount++;
                len--;
            }
            while (emptyCount > 0)
            {
                Console.Write(' ');
                emptyCount--;
            }
            Console.Write(' ');
            Console.SetCursorPosition(left, top);
        }
        private void MoveToEnd(ref int left, ref int top)
        {
            int len = textBuffer.Length;
            while (position < len)
            {
                left++;
                if (left >= Console.BufferWidth)
                {
                    top++;
                    left = 0;
                }
                Console.SetCursorPosition(left, top);
                position++;
            }
        }
        private void MoveToHome(ref int left, ref int top)
        {
            while (position > 0)
            {
                left--;
                if (left < 0)
                {
                    top--;
                    left = Console.BufferWidth - 1;
                }
                Console.SetCursorPosition(left, top);
                position--;
            }
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
