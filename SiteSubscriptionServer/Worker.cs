using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Models;
using SiteSubscription.Data;
using SiteSubscriptionServer.HubConfig;

namespace SiteSubscriptionServer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        //private readonly IHubContext<SiteHub, ISite> _siteHub;
        private readonly IHubContext<SiteHub> _siteHubContext;
        private readonly SiteHub _siteHub;

        public Worker(ILogger<Worker> logger, IHubContext<SiteHub> siteHubContext)
        {
            _logger = logger;
            _siteHubContext = siteHubContext;
            //_siteHub = siteHub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Executing at: {time}", DateTimeOffset.Now);                
                await sendMessagesToClients();
                await Task.Delay(5000, stoppingToken);
            }
        }

        private async Task sendMessagesToClients()
        {
            var site = getRandomSite();
            SiteManager.counter++;
            _logger.LogInformation("Message sent to: {site} with ID: {counter}", site.Name, SiteManager.counter);
            //await _siteHubContext.Clients.All.SendAsync(site.EventName, "Update site " + site.Name + " with ID: " + SiteManager.counter);
            await _siteHubContext.Clients.Group(site.EventName).SendAsync(site.EventName, "Update site " + site.Name + " with ID: " + SiteManager.counter);
            //foreach (var site in sitesList)
            //{
            //    _logger.LogInformation("Message sent to: {site}", site.Name);
            //    await _siteHub.Clients.All.SendAsync(site.EventName, "Update site " + site.Name);
            //}
        }

        public async Task JoinToGroup(string groupName)
        {            
            //await _siteHub.AddToGroup(groupName);


            //await _siteHub.Clients.Group(groupName).SendAsync("Send", $"{_siteHub.ConnectionId} has joined the group {groupName}.");
        }

        //public async Task RemoveFromGroup(string groupName)
        //{
        //    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        //    await _siteHub.Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupName}.");
        //}

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting signalr process at: {time}", DateTimeOffset.Now);
            // DO YOUR STUFF HERE
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // DO YOUR STUFF HERE
            await base.StopAsync(cancellationToken);
        }        

        private Site getRandomSite()
        {
            var sitesList = SiteManager.GetSiteListData();
            Random random = new Random();
            int index = random.Next(sitesList.Count);
            var site = sitesList.ElementAt(index);
            return site;
        }
    }
}
