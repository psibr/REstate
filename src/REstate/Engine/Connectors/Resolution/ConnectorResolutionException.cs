using System;

namespace REstate.Engine.Connectors.Resolution
{
    public class ConnectorResolutionException
        : Exception
    {
        public ConnectorResolutionException()
        {
        }

        public ConnectorResolutionException(string message) : base(message)
        {
        }

        public ConnectorResolutionException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
