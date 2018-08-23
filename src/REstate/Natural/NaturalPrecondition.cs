using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Engine;
using REstate.Engine.Connectors;
using REstate.Schematics;

namespace REstate.Natural
{
    public abstract class NaturalPrecondition<TSignal>
        : INaturalPrecondition<TSignal>
    {
        public abstract Task<bool> ValidateAsync(
            ConnectorContext context,
            TSignal signal,
            CancellationToken cancellationToken = default);

        public Task<bool> ValidateAsync<TPayload>(
            ISchematic<TypeState, TypeState> schematic,
            IStateMachine<TypeState, TypeState> machine,
            Status<TypeState> status,
            InputParameters<TypeState, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            TSignal signal = default;

            if (inputParameters != null)
            {
                if (!(inputParameters.Payload is TSignal castedSignal))
                    throw new ArgumentException(
                        $"Type mismatch for converting to signal: " +
                        $"{typeof(TSignal)} from {typeof(TPayload)}",
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