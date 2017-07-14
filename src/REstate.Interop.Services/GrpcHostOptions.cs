using Grpc.Core;

namespace REstate.Remote
{
    public class GrpcHostOptions
    {
        public Channel Channel { get; set; }
        public bool UseAsDefaultEngine { get; set; }
    }
}