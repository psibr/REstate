using REstate.Schematics;

namespace REstate.Engine.Connectors.Resolution
{
    /// <summary>
    /// Implementations resolve connectors from a given<see cref="ConnectorKey"/>.
    /// </summary>
    public interface IConnectorResolver<TState, TInput>
    {
        /// <summary>
        /// Resolves an Action Connector, given a <see cref="ConnectorKey"/>.
        /// </summary>
        IAction<TState, TInput> ResolveAction(ConnectorKey connectorKey);

        /// <summary>
        /// Resolves a BlukAction Connector, given a <see cref="ConnectorKey"/>.
        /// </summary>
        IBulkAction<TState, TInput> ResolveBulkAction(ConnectorKey connectorKey);

        /// <summary>
        /// Resolves a Precondition Connector, given a <see cref="ConnectorKey"/>.
        /// </summary>
        IPrecondition<TState, TInput> ResolvePrecondition(ConnectorKey connectorKey);
    }
}
