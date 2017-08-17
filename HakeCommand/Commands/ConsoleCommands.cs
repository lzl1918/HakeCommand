using HakeCommand.Framework;
using HakeCommand.Framework.Services.Environment;
using HakeCommand.Framework.Services.Output;
using System.IO;

namespace HakeCommand.Commands
{
    public sealed class ConsoleCommands : CommandSet
    {
        [Command("cls")]
        [Command("clear")]
        public void ClearConsole(IOutput output)
        {
            output.Clear();
        }

        [Command("cd")]
        public void SetWorkingDirectory(IEnvironment env, IOutput output, string path)
        {
            if (path == null)
                path = env.WorkingDirectory.FullName;
            else
                path = Path.Combine(env.WorkingDirectory.FullName, path);
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
                output.OutputObject($"directory not exists: {directoryInfo.FullName}");
            else
                env.SetDirectory(path);
        }
    }
}
