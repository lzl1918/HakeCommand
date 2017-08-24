using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Services.PathCompletion
{
    public sealed class PathCompletionChange
    {
        public string Input { get; }
        public int RemoveCount { get; }
        public string AppendText { get; }
        internal PathCompletionChange(string input, int removeCount, string appendText)
        {
            Input = input;
            RemoveCount = removeCount;
            AppendText = appendText;
        }
    }
    public interface IPathCompletionService
    {
        PathCompletionChange Complete(string inputPath);
        void Reset();
    }
}
