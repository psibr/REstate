using REstate.Engine;
using REstate.Engine.Connectors;
using REstate.Schematics;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Natural
{
    public interface IStateDefinition
        : IAction<TypeState, TypeState>
        , IPrecondition<TypeState, TypeState>
    {
    }

    public abstract class StateDefinition
        : IStateDefinition
    {
        public Task InvokeAsync<TRuntimePayload>(
            ISchematic<TypeState, TypeState> schematic,
            IStateMachine<TypeState, TypeState> machine,
            Status<TypeState> status,
            InputParameters<TypeState, TRuntimePayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            if(inputParameters == null) return Task.CompletedTask;

            if (this is IAcceptSignal<TRuntimePayload> action)
                return action.InvokeAsync(
                    new ConnectorContext
                    {
                        Schematic = new NaturalSchematic(schematic),
                        Machine = new NaturalStateMachine(machine),
                        Status = status,
                        Settings = connectorSettings
                    },
                    inputParameters.Payload,
                    cancellationToken);
            else
                throw new ArgumentException($"State: {GetType()} does not implement {typeof(IAcceptSignal<TRuntimePayload>)}!");
        }

        public Task<bool> ValidateAsync<TRuntimePayload>(
            ISchematic<TypeState, TypeState> schematic,
            IStateMachine<TypeState, TypeState> machine,
            Status<TypeState> status,
            InputParameters<TypeState, TRuntimePayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            if (inputParameters == null) return Task.FromResult(true);

            if (this is IAcceptSignal<TRuntimePayload> precondition)
                return precondition.ValidateAsync(
                    new ConnectorContext
                    {
                        Schematic = new NaturalSchematic(schematic),
                        Machine = new NaturalStateMachine(machine),
                        Status = status,
                        Settings = connectorSettings
                    },
                    inputParameters.Payload,
                    cancellationToken);
            else
                throw new ArgumentException($"State: {GetType()} does not implement {typeof(IAcceptSignal<TRuntimePayload>)}!");
        }
    }
}
