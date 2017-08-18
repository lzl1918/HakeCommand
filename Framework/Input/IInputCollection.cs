using System.Collections.Generic;

namespace HakeCommand.Framework
{
    public interface IInputCollection
    {
        string Raw { get; }
        IReadOnlyList<IInput> Inputs { get; }
        bool ContainsError { get; }
        string ErrorMessage { get; }
    }

    internal sealed class InternalInputCollection : IInputCollection
    {
        public string Raw { get; }
        public IReadOnlyList<IInput> Inputs { get; }
        public bool ContainsError { get; }
        public string ErrorMessage { get; }
        
        public InternalInputCollection(string raw, List<IInput> inputs)
        {
            Raw = raw;
            Inputs = inputs;
            ContainsError = false;
            ErrorMessage = null;
        }
        public InternalInputCollection(string raw, string error)
        {
            Raw = raw;
            Inputs = null;
            ContainsError = true;
            ErrorMessage = error;
        }
    }
}
