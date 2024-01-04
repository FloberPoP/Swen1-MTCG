using System.Net.Sockets;


namespace MTCG.Controller
{
    public class ClientResponse
    {
        public NetworkStream? NetworkStream { get; set; }
        public string? ResponseString { get; set; }
        public int StatusCode { get; set; }
        public Dictionary<string, string> ResponseHeaders { get; set; } = new Dictionary<string, string>();
    }
}
