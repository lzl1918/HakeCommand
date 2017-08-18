using Hake.Extension.DependencyInjection.Abstraction;
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
