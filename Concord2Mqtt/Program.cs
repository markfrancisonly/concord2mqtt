using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Automation.Concord2Mqtt
{
    public class Program
    {

        public static void Main(string[] args)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalUnhandledExceptionHandler);

            using (IHost host = CreateHostBuilder(args).Build())
            {
                host.Run();
            }
        }


        static void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            try
            {
                Exception ex = args.ExceptionObject as Exception;
                if (ex == null)
                {
                    if (args.IsTerminating)
                        Debug.WriteLine(string.Format("Fatal unhandled exception caught by global handler: {0}", args.ExceptionObject));
                    else
                        Debug.WriteLine(string.Format("Unhandled exception caught by global handler: {0}", args.ExceptionObject));
                }
                else
                {
                    if (args.IsTerminating)
                        Debug.WriteLine(string.Format("Fatal unhandled exception caught by global handler: {0}", ex.ToString()));
                    else
                        Debug.WriteLine(string.Format("Unhandled exception caught by global handler: {0}", ex.ToString()));
                }
            }
            catch
            { }

        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<Host>();
                });

    }
}
