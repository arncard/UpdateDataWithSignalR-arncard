using Microsoft.AspNetCore.SignalR;
using ServiceInterfaces;
using System.Threading.Tasks;

namespace SiteSubscriptionServer.HubConfig
{
    public class SiteHub : Hub<ISite>
    {
        public async Task SendMessageToClients(string message)
        {
            await Clients.All.SendSiteMessage(message);
        }
    }
}
