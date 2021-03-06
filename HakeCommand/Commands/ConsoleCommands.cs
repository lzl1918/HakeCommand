﻿using HakeCommand.Framework;
using HakeCommand.Framework.Services.Environment;
using HakeCommand.Framework.Services.OutputEngine;
using System;
using System.IO;

namespace HakeCommand.Commands
{
    public sealed class ConsoleCommands : CommandSet
    {
        [Description("Clear console contents")]
        [Command("cls")]
        [Command("clear")]
        public void ClearConsole()
        {
            OutputEngine.Clear();
        }

        [Description("Change the current working directory")]
        [Command("cd")]
        public void SetWorkingDirectory([Path]DirectoryInfo path)
        {
            if (!path.Exists)
                SetExceptionAndThrow(new Exception($"directory does not exist: {path.FullName}"));
            Environment.SetDirectory(path.FullName);
        }

        [Description("Don't write the returned object to console")]
        [Command("slient")]
        public void NoOutput()
        {
            Context.WriteResult = false;
            Context.SetResult(Context.InputObject);
        }
    }
}
