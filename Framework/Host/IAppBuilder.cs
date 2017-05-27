using Hake.Extension.Pipeline.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HakeCommand.Framework.Host
{
    public delegate Task AppDelegate(IHostContext context);
    public interface IAppBuilder : IPipeline<AppDelegate, IAppBuilder, IHostContext>
    {
        IServiceProvider Services { get; }
    }
}
