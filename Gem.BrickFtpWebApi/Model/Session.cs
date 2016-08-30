using System.Net;

namespace Gem.BrickFtpWebApi.Model
{
    public class Session
    {
        public string Domain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public SessionId SessionId { get; set; }
        public Cookie Cookie { get; private set; }

        public Session(string domain, string username, string password, SessionId sessionId)
        {
            Domain = domain;
            Username = username;
            Password = password;
            SessionId = sessionId;
            Cookie = new Cookie(BrickFtp.SessionCookieName, sessionId.id, "", domain);
        }
    }
}