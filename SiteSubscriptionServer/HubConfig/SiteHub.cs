using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Models;
using ServiceInterfaces;
using SiteSubscriptionServer.Interface;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SiteSubscriptionServer.HubConfig
{
    public delegate void ClientConnectionEventHandler(string clientId);
    public delegate void ClientNameChangedEventHandler(string clientId, string newName);
    public delegate void ClientGroupEventHandler(string clientId, string groupName);

    public delegate void MessageReceivedEventHandler(string senderClientId, string message);

    // Allow the server to call all the clients, even if they are not authenticated. Only hub level
    //[Authorize(RequireOutgoing=false)]

    //[Authorize(AuthenticationSchemes = "Bearer")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class SiteHub : Hub
    {
        private static ConcurrentDictionary<string, string> _users = new ConcurrentDictionary<string, string>();
        public static event ClientConnectionEventHandler ClientConnected;
        public static event ClientConnectionEventHandler ClientDisconnected;
        public static event ClientNameChangedEventHandler ClientNameChanged;

        public static event ClientGroupEventHandler ClientJoinedToGroup;
        public static event ClientGroupEventHandler ClientLeftGroup;

        public static event MessageReceivedEventHandler MessageReceived;

        private readonly ISiteConnectionManager _siteConnectionManager;
        //IHubContext<SiteHub> _hubContext;
        //private readonly Site _site;
        public SiteHub(ISiteConnectionManager siteConnectionManager)
        {
            _siteConnectionManager = siteConnectionManager;
            //_hubContext = hubcontext;            
        }
        
        // only available to authenticated users
        //[Authorize]
        public async Task<string> JoinSiteGroup(string groupName)
        {
            //var httpContext = this.Context.GetHttpContext();
            //var userId = httpContext.Request.Query["userId"];
            await AddToGroup(groupName);
            _siteConnectionManager.KeepUserConnection(groupName, Context.ConnectionId);
            
            return Context.ConnectionId;
        }

        //[Authorize]
        public async Task<string> LeaveSiteGroup(string groupName)
        {
            //var httpContext = this.Context.GetHttpContext();
            //var userId = httpContext.Request.Query["userId"];            
            _siteConnectionManager.RemoveUserConnection(Context.ConnectionId, out bool removed);
            if (removed)
                await RemoveFromGroup(groupName);
            return removed.ToString();
        }
        private async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        private async Task RemoveFromGroup(string groupName)
        {

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

       
        public override Task OnConnectedAsync()
        {
            _users.TryAdd(Context.ConnectionId, Context.ConnectionId);
            ClientConnected?.Invoke(Context.ConnectionId);

            return base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            // Here the server sends the client a disconnect message, so remove it from the groups
            string userName;
            _users.TryRemove(Context.ConnectionId, out userName);

            ClientDisconnected?.Invoke(Context.ConnectionId);

            //return base.OnDisconnectedAsync(exception);
            //get the connectionId
            var connectionId = Context.ConnectionId;
            await RemoveFromGroup("");
            _siteConnectionManager.RemoveUserConnection(connectionId, out bool removed);
            await Task.FromResult(0);
        }        

        //public async Task SendMessageToClients(string message)
        //{
        //    await Clients.All.SendSiteMessage(message);
        //}
    }
}
