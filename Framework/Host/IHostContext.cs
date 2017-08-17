using Hake.Extension.Pipeline.Abstraction;
using HakeCommand.Framework.Command;

namespace HakeCommand.Framework.Host
{
    public interface IHostContext : IContext
    {
        ICommand Command { get; }
        object Result { get; }
        void SetResult(object result);
    }
}
