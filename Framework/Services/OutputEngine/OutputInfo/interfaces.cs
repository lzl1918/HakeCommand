using System.Collections.Generic;

namespace HakeCommand.Framework.Services.OutputEngine
{
    public interface IOutputHeader
    {
        string Content { get; }
    }
    public interface IOutputBody
    {
        IReadOnlyList<string> Contents { get; }
    }
    public interface IOutputFooter
    {
        string Content { get; }
    }
    public interface IOutputInfo
    {
        IOutputHeader Header { get; }
        IReadOnlyList<IOutputBody> Body { get; }
        IOutputFooter Footer { get; }
    }
}
