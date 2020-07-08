using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiteSubscriptionServer.Interface
{
    public interface ISiteConnectionManager
    {
        void KeepUserConnection(string userId, string connectionId);
        void RemoveUserConnection(string connectionId, out bool removed);
        List<string> GetUserConnections(string userId);
    }
}
