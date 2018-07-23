using REstate.Engine;
using REstate.Engine.Connectors;
using REstate.Schematics;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate
{
    public interface IStateDefinition<in TPayload>
        : IAction<TypeState, TypeState>
        , IPrecondition<TypeState, TypeState>
    {
        Task InvokeAsync(
            ISchematic<TypeState, TypeState> schematic,
            IStateMachine<TypeState, TypeState> machine,
            Status<TypeState> status,
            IInputParameters<TypeState, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default);

        Task<bool> ValidateAsync(
            ISchematic<TypeState, TypeState> schematic,
            IStateMachine<TypeState, TypeState> machine,
            Status<TypeState> status,
            IInputParameters<TypeState, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default);
    }

    public interface IStateDefinition
    {
    }

    public class StateDefinition : IStateDefinition
    {

    }

    public abstract class StateDefinition<TPayload>
        : StateDefinition
        , IStateDefinition<TPayload>
    {
        public Task InvokeAsync<TRuntimePayload>(
            ISchematic<TypeState, TypeState> schematic,
            IStateMachine<TypeState, TypeState> machine,
            Status<TypeState> status,
            InputParameters<TypeState, TRuntimePayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            if (!(inputParameters.Payload is TPayload payload))
                throw new ArgumentException("Payload type mismatch", nameof(inputParameters.Payload));

            return InvokeAsync(
                schematic,
                machine,
                status,
                (IInputParameters<TypeState, TPayload>)new InputParameters<TypeState, TPayload>(inputParameters.Input, payload),
                connectorSettings,
                cancellationToken);
        }

        public virtual Task InvokeAsync(
            ISchematic<TypeState, TypeState> schematic,
            IStateMachine<TypeState, TypeState> machine,
            Status<TypeState> status,
            IInputParameters<TypeState, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public virtual Task<bool> ValidateAsync(
            ISchematic<TypeState, TypeState> schematic,
            IStateMachine<TypeState, TypeState> machine,
            Status<TypeState> status,
            IInputParameters<TypeState, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
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
            if(inputParameters.Payload is TPayload payload)
            {
                return ValidateAsync(
                    schematic,
                    machine,
                    status,
                    (IInputParameters<TypeState, TPayload>)new InputParameters<TypeState, TPayload>(inputParameters.Input, payload),
                    connectorSettings,
                    cancellationToken);
            }

            return Task.FromResult(false);
        }
    }
}
