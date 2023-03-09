namespace Bounsnet.Models
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public string ClientId { get; set; }
        public DateTime IssuedTime { get; set; }
        public DateTime ExpiredTime { get; set; }
        public string ProtectedTicket { get; set; }

        public long ClientKeyId { get; set; }
        public string ClientSecret { get; set; }
        public string ClientName { get; set; }
        public bool Active { get; set; }
        public int LifeTime { get; set; }
        public string AllowedOrigin { get; set; }

    }
}