using HakeCommand.Framework.Helpers;
using HakeCommand.Framework.Services.HistoryProvider;
using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Input.Internal
{
    internal sealed class HostInput : IHostInput
    {
        private Stack<char> postStack;
        private Stack<int> stateStack;
        private StringBuilder textBuffer;
        private IHistoryProvider historyProvider;
        private string lastInputCache;
        private int position;
        private int lastPosition;
        private OutputStateMachine outputMachine;

        public HostInput(IHistoryProvider historyProvider)
        {
            stateStack = new Stack<int>();
            textBuffer = new StringBuilder();
            postStack = new Stack<char>();
            position = 0;
            this.historyProvider = historyProvider;
            outputMachine = new OutputStateMachine();
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
                        outputMachine.SetState(stateStack.Pop());
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (CharCategoryHelper.IsValidInput(keyInfo.KeyChar))
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
            outputMachine.ClearState();
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
                        outputMachine.ClearState();
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
                        outputMachine.ClearState();
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
                            outputMachine.ClearState();
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
                else if (CharCategoryHelper.IsValidInput(keyInfo.KeyChar))
                {
                    int stackSize = stateStack.Count;
                    int current = stackSize;
                    while (current > position)
                    {
                        outputMachine.SetState(stateStack.Pop());
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
                    outputMachine.SetState(stateStack.Pop());
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
                outputMachine.SetState(stateStack.Pop());
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
            int oldstate = outputMachine.State;
            bool output = false;
            ConsoleColor outColor = Console.ForegroundColor;
            ConsoleColor originColor = outColor;
            outputMachine.InovkeOneShot(ch, originColor, out output, out outColor);
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
