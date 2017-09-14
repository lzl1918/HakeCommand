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
}
