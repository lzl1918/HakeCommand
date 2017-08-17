using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace HakeCommand.Framework.Services.Environment
{
    public interface IEnvironment
    {
        DirectoryInfo WorkingDirectory { get; }

        void SetDirectory(string path);
    }

    internal sealed class Environment : IEnvironment
    {
        public DirectoryInfo WorkingDirectory { get; private set; }

        public Environment(string path)
        {
            SetDirectory(path);
        }
        public void SetDirectory(string path)
        {
            WorkingDirectory = new DirectoryInfo(path);
        }
    }
}
