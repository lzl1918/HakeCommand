using HakeCommand.Framework.Host;
using System;

namespace HakeCommand
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            IHost host = new HostBuilder()
                .UseConfiguration<HostConfiguration>()
                .Build();

            host.Run();
        }
    }
}