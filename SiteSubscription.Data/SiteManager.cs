
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiteSubscription.Data
{
    public class SiteManager
    {
        public static int counter = 0;
        public static List<Site> GetSiteListData()
        {
            return new List<Site>
            {
                new Site { Id = 1, Name = "Koala Beach", EventName = "koalabeachevent" },
                new Site { Id = 2, Name = "Orange", EventName = "orangeevent" },
                new Site { Id = 3, Name = "Kangaroo", EventName = "kangarooevent" },
                new Site { Id = 4, Name = "Cornerstone", EventName = "cornerstoneevent" }
            };
        }
    }
}
