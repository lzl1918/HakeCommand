using HakeCommand.Framework;
using HakeCommand.Framework.Services.Environment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HakeCommand.Commands
{
    public sealed class ListContentCommands : CommandSet
    {
        [Command("ls")]
        public string ListContent(IEnvironment env, string path)
        {
            if (path == null)
                path = env.WorkingDirectory.FullName;
            else
                path = Path.Combine(env.WorkingDirectory.FullName, path);
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
                return $"can not find directory: {directoryInfo.FullName}";
            else
            {
                var files = directoryInfo.EnumerateFiles();
                var directories = directoryInfo.EnumerateDirectories();
                StringBuilder builder = new StringBuilder();
                foreach (DirectoryInfo dir in directories)
                    builder.AppendLine(dir.Name);
                foreach (FileInfo file in files)
                    builder.AppendLine(file.Name);
                return builder.ToString();
            }
        }
    }
}
