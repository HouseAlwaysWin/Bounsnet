namespace Bounsweet.Models
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public string ClientId { get; set; }
        public DateTime IssuedTime { get; set; }
        public DateTime ExpiredTime { get; set; }
        public string ProtectedTicket { get; set; }

        public RefreshTokenInfo TokenInfo { get; set; }
    }
}