using System.Collections.Generic;

namespace HakeCommand.Framework.Input.Internal
{
    internal sealed class InputCollection : IInputCollection
    {
        public string Raw { get; }
        public IReadOnlyList<IInput> Inputs { get; }
        public bool ContainsError { get; }
        public string ErrorMessage { get; }
        
        public InputCollection(string raw, List<IInput> inputs)
        {
            Raw = raw;
            Inputs = inputs;
            ContainsError = false;
            ErrorMessage = null;
        }
        public InputCollection(string raw, string error)
        {
            Raw = raw;
            Inputs = null;
            ContainsError = true;
            ErrorMessage = error;
        }
    }
}
