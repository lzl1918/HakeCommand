using HakeCommand.Framework.Host;
using System;

namespace HakeCommand
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            IHost host = new HostBuilder()
                .UseConfiguration<HostConfiguration>()
                .Build();

            host.Run();
        }
    }
}