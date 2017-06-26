using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Services
{
    public interface IConnectorFactory
    {
        string ConnectorKey { get; }

        Task<IConnector> BuildConnectorAsync(CancellationToken cancellationToken);

        bool IsActionConnector { get; }

        bool IsGuardConnector { get; }
    }
}
