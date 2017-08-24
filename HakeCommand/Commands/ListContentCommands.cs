using HakeCommand.Framework;
using HakeCommand.Framework.Input;
using HakeCommand.Framework.Services.Environment;
using HakeCommand.Framework.Services.OutputEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HakeCommand.Commands
{
    public sealed class ListContentCommands : CommandSet
    {
        [Command("ls")]
        public IList<FileSystemInfo> ListContent([Path]DirectoryInfo path)
        {
            if (!path.Exists)
                SetExceptionAndThrow(new Exception($"directory does not exist: {path.FullName}"));

            if (!Context.InPipe)
            {
                OutputEngine.WriteObject($"{path}");
                OutputEngine.WriteObject("");
            }
            var files = path.EnumerateFiles();
            var directories = path.EnumerateDirectories();
            List<FileSystemInfo> result = new List<FileSystemInfo>();
            result.AddRange(directories);
            result.AddRange(files);
            return result;
        }

        [Command("cat")]
        public string ShowFileContent([Path]FileInfo file)
        {
            if (file == null)
            {
                string input = ReadLine("file path");
                file = new FileInfo(Path.Combine(Environment.WorkingDirectory.FullName, input));
            }
            if (!file.Exists)
                SetExceptionAndThrow(new Exception($"file does not exist: {file.FullName}"));

            Stream stream = file.OpenRead();
            StreamReader reader = new StreamReader(stream);
            string content = reader.ReadToEnd();
            reader.Dispose();
            stream.Dispose();
            return content;
        }

        [Command("echo")]
        public object ShowContent(object content)
        {
            content = content ?? InputObject;
            return content;
        }
    }
}
