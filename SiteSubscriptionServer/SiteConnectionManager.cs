using Microsoft.AspNetCore.SignalR;
using SiteSubscriptionServer.HubConfig;
using SiteSubscriptionServer.Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace SiteSubscriptionServer
{
    public class SiteConnectionManager : ISiteConnectionManager
    {
        //private IHubContext<SiteHub> _siteHubContext { get; set; }
        //public SiteConnectionManager(IHubContext<SiteHub> siteHubContext)
        //{
        //    _siteHubContext = siteHubContext;
        //}

        //private void BroadcastSiteUpdates()
        //{
        //    _siteHubContext.Clients.Group("event");
        //}
        private static Dictionary<string, List<string>> userConnectionMap = new Dictionary<string, List<string>>();
        private static string userConnectionMapLocker = string.Empty;
        public void KeepUserConnection(string groupName, string connectionId)
        {
            lock (userConnectionMapLocker)
            {
                if (!userConnectionMap.ContainsKey(groupName))
                {
                    userConnectionMap[groupName] = new List<string>();
                }
                userConnectionMap[groupName].Add(connectionId);
            }
        }
        public List<string> GetUserConnections(string groupName)
        {
            var conn = new List<string>();
            lock (userConnectionMapLocker)
            {
                conn = userConnectionMap[groupName];
            }
            return conn;
        }
        public void RemoveUserConnection(string connectionId)
        {
            //This method will remove the connectionId of user
            lock (userConnectionMapLocker)
            {
                foreach (var groupName in userConnectionMap.Keys)
                {
                    if (userConnectionMap.ContainsKey(groupName))
                    {
                        if (userConnectionMap[groupName].Contains(connectionId))
                        {
                            userConnectionMap[groupName].Remove(connectionId);
                            break;
                        }
                    }
                }
            }
        }

    }
}
