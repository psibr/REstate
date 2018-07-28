using REstate.Engine;
using REstate.Engine.Connectors;
using REstate.Schematics;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Natural
{

    public interface IStateDefinition<in TSignal>
        : IStateDefinition
        , IAction<TypeState, TypeState>
        , IPrecondition<TypeState, TypeState>
    {
        Task InvokeAsync(
            ConnectorContext context,
            TSignal signal,
            CancellationToken cancellationToken = default);

        Task<bool> ValidateAsync(
            ConnectorContext context,
            TSignal signal,
            CancellationToken cancellationToken = default);
    }

    public interface IStateDefinition
    {
    }

    public class StateDefinition : StateDefinition<object>
    {

    }

    public abstract class StateDefinition<TSignal>
        : IStateDefinition<TSignal>
    {
        public Task InvokeAsync<TRuntimePayload>(
            ISchematic<TypeState, TypeState> schematic,
            IStateMachine<TypeState, TypeState> machine,
            Status<TypeState> status,
            InputParameters<TypeState, TRuntimePayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            TSignal signal = default;

            if (inputParameters != null)
            {
                if (!(inputParameters.Payload is TSignal castedSignal))
                    throw new ArgumentException(
                        $"Type mismatch for converting to signal: " +
                        $"{typeof(TSignal)} from {typeof(TRuntimePayload)}",
                        nameof(inputParameters.Payload));

                signal = castedSignal;
            }

            return InvokeAsync(
                new ConnectorContext
                {
                    Schematic = new NaturalSchematic(schematic),
                    Machine = new NaturalStateMachine(machine),
                    Status = status,
                    Settings = connectorSettings
                },
                signal,
                cancellationToken);
        }

        public virtual Task InvokeAsync(
            ConnectorContext context,
            TSignal signal,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public virtual Task<bool> ValidateAsync(
            ConnectorContext context,
            TSignal signal,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> ValidateAsync<TRuntimePayload>(
            ISchematic<TypeState, TypeState> schematic,
            IStateMachine<TypeState, TypeState> machine,
            Status<TypeState> status,
            InputParameters<TypeState, TRuntimePayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            TSignal signal = default;

            if (inputParameters != null)
            {
                if (!(inputParameters.Payload is TSignal castedSignal))
                    throw new ArgumentException(
                        $"Type mismatch for converting to signal: " +
                        $"{typeof(TSignal)} from {typeof(TRuntimePayload)}",
                        nameof(inputParameters.Payload));

                signal = castedSignal;
            }

            return ValidateAsync(
                new ConnectorContext
                {
                    Schematic = new NaturalSchematic(schematic),
                    Machine = new NaturalStateMachine(machine),
                    Status = status,
                    Settings = connectorSettings
                },
                signal,
                cancellationToken);
        }
}
}
