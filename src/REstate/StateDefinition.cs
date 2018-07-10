using REstate.Engine;
using REstate.Engine.Connectors;
using REstate.Schematics;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate
{
    public interface IStateDefinition<TInput, in TPayload>
        : IStateDefinition<TInput>
    {
        Task InvokeAsync(
            ISchematic<TypeState, TInput> schematic,
            IStateMachine<TypeState, TInput> machine,
            Status<TypeState> status,
            IInputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default);
    }

    public interface IStateDefinition<TInput>
        : IStateDefinition
        , IAction<TypeState, TInput>
        , IPrecondition<TypeState, TInput>
    {
    }

    public interface IStateDefinition
    {
    }

    public class StateDefinition : IStateDefinition
    {

    }

    public abstract class StateDefinition<TInput>
        : StateDefinition
        , IStateDefinition<TInput>
    {
        public virtual Task InvokeAsync<TPayload>(
            ISchematic<TypeState, TInput> schematic,
            IStateMachine<TypeState, TInput> machine,
            Status<TypeState> status,
            InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public virtual Task<bool> ValidateAsync<TPayload>(
            ISchematic<TypeState, TInput> schematic,
            IStateMachine<TypeState, TInput> machine,
            Status<TypeState> status,
            InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }

    public abstract class StateDefinition<TInput, TPayload>
        : StateDefinition<TInput>
        , IStateDefinition<TInput, TPayload>
    {
        public override Task InvokeAsync<TRuntimePayload>(
            ISchematic<TypeState, TInput> schematic,
            IStateMachine<TypeState, TInput> machine,
            Status<TypeState> status,
            InputParameters<TInput, TRuntimePayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            if (!(inputParameters.Payload is TPayload payload))
                throw new ArgumentException("Payload type mismatch", nameof(inputParameters.Payload));

            return InvokeAsync(
                schematic,
                machine,
                status,
                new InputParameters<TInput, TPayload>(inputParameters.Input, payload),
                connectorSettings,
                cancellationToken);
        }

        public virtual Task InvokeAsync(
            ISchematic<TypeState, TInput> schematic,
            IStateMachine<TypeState, TInput> machine,
            Status<TypeState> status,
            IInputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override Task<bool> ValidateAsync<TRuntimePayload>(
            ISchematic<TypeState, TInput> schematic,
            IStateMachine<TypeState, TInput> machine,
            Status<TypeState> status,
            InputParameters<TInput, TRuntimePayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(inputParameters.Payload is TPayload);
        }
    }
}
