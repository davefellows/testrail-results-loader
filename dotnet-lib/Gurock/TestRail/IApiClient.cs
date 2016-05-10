using System.Threading.Tasks;

namespace Gurock.TestRail
{
    public interface IApiClient
    {
        string User { get; set; }
        string Password { get; set; }
        Task<object> SendGet(string uri);
        Task<object> SendPost(string uri, object data);
    }
}