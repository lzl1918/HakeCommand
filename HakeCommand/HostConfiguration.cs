using Hake.Extension.DependencyInjection.Abstraction;
using HakeCommand.Framework.Command;
using HakeCommand.Framework.Components.CommandLoader;
using HakeCommand.Framework.Host;
using HakeCommand.Framework.Services.Environment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HakeCommand
{
    public class TestComponent
    {
        public Task Invoke(IHostContext context, IEnvironment env, string path, Func<Task> next)
        {
            ICommand command = context.Command;
            if (command.Command != "ls")
                return next();

            if (path == null)
                path = env.WorkingDirectory.FullName;
            else
                path = Path.Combine(env.WorkingDirectory.FullName, path);
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
                context.SetResult($"can not find directory: {directoryInfo.FullName}");
            else
            {
                var files = directoryInfo.EnumerateFiles();
                var directories = directoryInfo.EnumerateDirectories();
                StringBuilder builder = new StringBuilder();
                foreach (DirectoryInfo dir in directories)
                    builder.AppendLine(dir.Name);
                foreach (FileInfo file in files)
                    builder.AppendLine(file.Name);
                context.SetResult(builder.ToString());
            }
            return Task.CompletedTask;
        } 
    }
    public sealed class HostConfiguration
    {
        public HostConfiguration()
        {

        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCommandProvider();
        }
        public void ConfigureComponents(IAppBuilder app)
        {
            app.UseCommands();
        }
    }
}
