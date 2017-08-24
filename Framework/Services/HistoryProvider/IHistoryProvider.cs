using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Services.HistoryProvider
{
    public interface IHistoryProvider
    {
        void Add(string command);
        string NextHistory();
        string PreviousHistory();
        void Reset();
    }
}
