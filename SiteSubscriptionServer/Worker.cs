using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;
using SiteSubscription.Data;
using SiteSubscriptionServer.HubConfig;

namespace SiteSubscriptionServer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        //private readonly IHubContext<SiteHub, ISite> _siteHub;
        private readonly IHubContext<SiteHub> _siteHub;

        public Worker(ILogger<Worker> logger, IHubContext<SiteHub> siteHub)
        {
            _logger = logger;
            _siteHub = siteHub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Message sent at: {time}", DateTimeOffset.Now);
                //var timerManager = new TimerManager(() => _siteHub.Clients.All.SendSiteMessage("Message sent to site"));
                //await _siteHub.Clients.All.SendAsync(, "Message sent to site");
                await sendMessagesToClients();
                await Task.Delay(2000, stoppingToken);
            }
        }

        private async Task sendMessagesToClients()
        {     
            var site = getRandomSite();
            _logger.LogInformation("Message sent to: {site}", site.Name);
            await _siteHub.Clients.All.SendAsync(site.EventName, "Update site " + site.Name);
            //foreach (var site in sitesList)
            //{
            //    _logger.LogInformation("Message sent to: {site}", site.Name);
            //    await _siteHub.Clients.All.SendAsync(site.EventName, "Update site " + site.Name);
            //}
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
