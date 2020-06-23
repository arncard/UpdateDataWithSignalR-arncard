using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;
using ServiceInterfaces;
using SiteSubscription.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace SiteSubscriptionClient
{
    public class SiteHubClient : ISite, IHostedService
    {
        public string HubUrl => "https://localhost:5001/hubs/site";
        private readonly ILogger<SiteHubClient> _logger;
        private HubConnection _connection;

        public SiteHubClient(ILogger<SiteHubClient> logger)
        {
            _logger = logger;

            _connection = new HubConnectionBuilder()
                .WithUrl(HubUrl)
                .Build();
            // connection.on runs when server-side code calls it using the Send method
            //_connection.On<string>("SendSiteMessage", SendSiteMessage);

            // subscribe to a random site
            var site = getRandomSite();
            _connection.On<string>(site.EventName, SendSiteMessage);
            _logger.LogInformation("You are subscribed to the site : " + site.Name);
        }

        public Task SendSiteMessage(string message)
        {
            _logger.LogInformation("Message received from server: {message}", message);

            return Task.CompletedTask;
        }

        private Site getRandomSite()
        {
            var sitesList = SiteManager.GetSiteListData();
            Random random = new Random();
            int index = random.Next(sitesList.Count);
            var site = sitesList.ElementAt(index);
            return site;
        }        

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Loop is here to wait until the server is running
            while (true)
            {
                try
                {
                    await _connection.StartAsync(cancellationToken);

                    break;
                }
                catch
                {
                    await Task.Delay(1000);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _connection.DisposeAsync();
        }
    }
}
