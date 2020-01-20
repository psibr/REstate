namespace Server.Configuration.REstate
{
    public class ServerConfiguration
    {
        public ServerBinding Binding { get; set; }

        public string RepositoryConnectionString { get; set; }
    }

    public class ServerBinding
    {
        public string Address { get; set; }

        public int Port { get; set; }
    }
}
