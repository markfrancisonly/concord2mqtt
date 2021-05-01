using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace Automation.Concord2Mqtt
{
    public class Host : IHostedService, IDisposable
    {
        readonly ILogger<Host> logger;
        readonly IConfiguration configuration;
        readonly IHostEnvironment env;
        Concord2Mqtt server;
        ManualResetEvent stopSignal = new ManualResetEvent(false);

        public Host(ILogger<Host> logger, IConfiguration configuration, IHostEnvironment env)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.env = env;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
 
                System.Threading.Thread hostThread = new System.Threading.Thread
                (
                    delegate ()
                    {
                       

                        while (true)
                        {
                            try
                            {
                                logger.LogInformation("Concord2Mqtt started at {0}", DateTimeOffset.Now);

                                Concord.ConcordConfiguration concordConfig = configuration.GetSection("Concord").Get<Concord.ConcordConfiguration>();
                                Concord2MqttConfiguration concord2mqttConfig = configuration.GetSection("Concord2Mqtt").Get<Concord2MqttConfiguration>();

                                if (concordConfig == null || concord2mqttConfig == null)
                                {
                                    logger.LogError("Configuration cannot be read. Exiting.");
                                    return;
                                }

                                server = new Concord2Mqtt(logger, concord2mqttConfig, concordConfig);

                                try
                                {
                                    stopSignal.Reset();
                                    server.Start();
                                    stopSignal.WaitOne();
                                    server.Stop();
                                    server = null;
                                }
                                catch
                                {
                                    throw;
                                }
                                break;
                            }
                            catch (Exception ex)
                            {

                                try
                                {
                                    logger.LogError(ex, "Host failed to start home automation server");
                                    logger.LogInformation("Failed to start home automation server. Restarting in 5 seconds");

                                    Thread.Sleep(5000);

                                }
                                catch (ThreadAbortException)
                                {
                                    return;
                                }
                                catch (ThreadInterruptedException)
                                {
                                    return;
                                }
                                catch { }
                                continue;
                            }
                            finally
                            {
                                // close server down

                                if (server != null)
                                    server.Stop();
                             
                            }
                        }
                    }
                );

                hostThread.IsBackground = false;
                hostThread.Name = "HomeControlServerHost";
                hostThread.Start();
                
          
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            try
            {
                logger.LogInformation("Concord2Mqtt is stopping.");
                stopSignal.Set();
 
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to gracefully stop home host");
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }




    }
}