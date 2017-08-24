using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Services.HistoryProvider
{
    internal sealed class InternalHistoryProvider : IHistoryProvider
    {
        public int Capacity { get; }

        private List<string> commands;
        private int cursorIndex;

        public InternalHistoryProvider(int capacity)
        {
            Capacity = capacity;
            commands = new List<string>();
            cursorIndex = -1;
        }

        public void Add(string command)
        {
            while (commands.Count >= Capacity)
                commands.RemoveAt(commands.Count - 1);

            commands.Insert(0, command);
        }

        public string NextHistory()
        {
            if (cursorIndex >= commands.Count - 1)
                return null;

            cursorIndex++;
            return commands[cursorIndex];
        }
        public string PreviousHistory()
        {
            if (cursorIndex <= 0)
            {
                cursorIndex = -1;
                return null;
            }

            cursorIndex--;
            return commands[cursorIndex];
        }

        public void Reset()
        {
            cursorIndex = -1;
        }


    }
}
