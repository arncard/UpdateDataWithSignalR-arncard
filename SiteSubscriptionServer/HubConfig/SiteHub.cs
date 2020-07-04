using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Models;
using ServiceInterfaces;
using SiteSubscriptionServer.Interface;
using System;
using System.Threading.Tasks;

namespace SiteSubscriptionServer.HubConfig
{
    //[Authorize(AuthenticationSchemes = "Bearer")]
    public class SiteHub : Hub
    {
        private readonly ISiteConnectionManager _siteConnectionManager;
        //IHubContext<SiteHub> _hubContext;
        //private readonly Site _site;
        public SiteHub(ISiteConnectionManager siteConnectionManager)
        {
            _siteConnectionManager = siteConnectionManager;
            //_hubContext = hubcontext;            
        }

        public async Task<string> GetConnectionId(string groupName)
        {
            //var httpContext = this.Context.GetHttpContext();
            //var userId = httpContext.Request.Query["userId"];
            await AddToGroup(groupName);
            _siteConnectionManager.KeepUserConnection(groupName, Context.ConnectionId);
            
            return Context.ConnectionId;
        }
        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            //return base.OnDisconnectedAsync(exception);
            //get the connectionId
            var connectionId = Context.ConnectionId;
            _siteConnectionManager.RemoveUserConnection(connectionId);
            await Task.FromResult(0);
        }        

        //public async Task SendMessageToClients(string message)
        //{
        //    await Clients.All.SendSiteMessage(message);
        //}
    }
}
