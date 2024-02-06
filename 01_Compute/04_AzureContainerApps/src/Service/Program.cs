using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DotNetEnv;
using System.Text.Json;
using Azure.Messaging.ServiceBus;

IHost consoleHost = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddTransient<Main>(); 
    })
    .Build();
Main main = consoleHost.Services.GetRequiredService<Main>(); 
await main.ExecuteAsync(args); 

class Main
{
    string _configurationFile = "../../CreateEnv/Configuration.env"; 
    
    public Main()
    {
        Env.Load(_configurationFile);
    }

    public async Task<bool> ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {

        string connectionString = Env.GetString("LI4DEVS.CONTAINERAPPS.SB.CONNECTIONSTRING")
            ??
            "Endpoint=sb://lil-azure4devs.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=oK8MARl5hJtblDnruqpwx5OylNUzyVGXM+ASbIqhtRI=";
        string queueName = Env.GetString("LI4DEVS.CONTAINERAPPS.SB.QUEUE")
            ??
            "lil-azure4devs-queue";

        await using ServiceBusClient serviceBusClient = new ServiceBusClient(connectionString);
        ServiceBusReceiver serviceBusReceiver = serviceBusClient.CreateReceiver(queueName);

        ServiceBusReceivedMessage serviceBusReceivedMessage; 

        while (true) {
            Console.WriteLine($"Checking for messages on {queueName} - {DateTime.UtcNow}...");
            serviceBusReceivedMessage = await serviceBusReceiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));
            if (serviceBusReceivedMessage != null) {
                string messageBody = serviceBusReceivedMessage.Body.ToString();
                Console.WriteLine($"Message received: {messageBody}");
                await serviceBusReceiver.CompleteMessageAsync(serviceBusReceivedMessage); 
            }
            Thread.Sleep(1000);
            if (cancellationToken.IsCancellationRequested) {
                break; 
            }
        }
        return true; 
    }
}