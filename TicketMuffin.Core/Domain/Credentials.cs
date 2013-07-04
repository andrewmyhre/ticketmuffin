namespace TicketMuffin.Core.Domain
{
    public class Credentials
    {
        public string Username { get; set; }

        public byte[] SaltedHashedPassword { get; set; }
    }
}