using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Host
{
    public interface IHostBuilder
    {
        IHost Build();
        IHostBuilder ConfigureServices(Action<IServiceProvider> configureServices);
    }
}
