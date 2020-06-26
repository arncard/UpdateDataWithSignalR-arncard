using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;
using ServiceInterfaces;
using SiteSubscription.Data;
using SiteSubscriptionClient.ConnectionRetryPolicy;
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

            //WithAutomaticReconnect() configures the client to wait 0, 2, 10, and 30 seconds respectively 
            //before trying each reconnect attempt, stopping after four failed attempts.
            _connection = new HubConnectionBuilder()
                .WithUrl(HubUrl)
                .WithAutomaticReconnect(new RandomRetryPolicy())
                .Build();

            _connection.Reconnecting += _connection_Reconnecting;
            _connection.Reconnected += _connection_Reconnected;
            _connection.Closed += _connection_Closed;            
            // subscribe to a random site
            var site = getRandomSite();
            // connection.on runs when server-side code calls it using the Send method
            //_connection.On<string>("SendSiteMessage", SendSiteMessage);
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

        #region Connection handling
        private async Task _connection_Closed(Exception arg)
        {
            if (_connection.State == HubConnectionState.Disconnected)
            {                
                // In this state the users should be notified the connection has been closed                
                _logger.LogWarning("The connection has been closed");

                // Try to restart manually ??????
                // waiting for some random delay to prevent overloading the server
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await _connection.StartAsync();
            }

            //return Task.CompletedTask;
        }        

        private Task _connection_Reconnected(string arg)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                // In this state the users should be notified the connection was reestablished
                // Start dequeuing messages queued while reconnecting if any
                _logger.LogWarning("The connection was restablished");
            }

            return Task.CompletedTask;
        }

        private Task _connection_Reconnecting(Exception arg)
        {
            if(_connection.State == HubConnectionState.Reconnecting)
            {
                // In this state the users should be notified the connection was lost
                // Start queuing or dropping messages
                _logger.LogWarning("Starting a reconnection event");
            }            

            return Task.CompletedTask;
        }
        #endregion Connection handling

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Loop is here to wait until the server is running
            while (true)
            {
                try
                {
                    await _connection.StartAsync(cancellationToken);
                    _logger.LogInformation("Connection started");
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

        // To be passed to WithAutomaticReconnect
        //// WithAutomaticReconnect() won't configure the HubConnection to retry initial start failures, so start failures need to be handled manually
        //public static async Task<bool> ConnectWithRetryAsync(HubConnection connection, CancellationToken token)
        //{
        //    // Keep trying to until we can start or the token is canceled.
        //    while (true)
        //    {
        //        try
        //        {
        //            await connection.StartAsync(token);
        //            if(connection.State == HubConnectionState.Connected)
        //                return true;
        //        }
        //        catch when (token.IsCancellationRequested)
        //        {
        //            return false;
        //        }
        //        catch
        //        {
        //            // Failed to connect, trying again in 5000 ms.
        //            if(connection.State == HubConnectionState.Disconnected)
        //                await Task.Delay(5000);
        //        }
        //    }
        //}
    }
}
