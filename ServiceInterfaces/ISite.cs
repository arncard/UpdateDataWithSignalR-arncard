using System.Threading.Tasks;

namespace ServiceInterfaces
{
    public interface ISite
    {
        Task SendSiteMessage(string message);
    }
}
