using REstate.Engine;
using REstate.Engine.Connectors;
using REstate.Schematics;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace REstate
{
    public interface IActionWithPayload<TState, TInput, TPayload>
    {
        Task InvokeAsync(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default);
    }

    public abstract class ActionWithPayload<TState, TInput, TPayload>
        : IAction<TState, TInput>
        , IActionWithPayload<TState, TInput, TPayload>
        , IPrecondition<TState, TInput>
    {
        public Task InvokeAsync<TRuntimePayload>(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            InputParameters<TInput, TRuntimePayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            var payloadParameter = Expression.Parameter(typeof(TRuntimePayload), "payload");

            var converter = Expression.Lambda<Func<TRuntimePayload, TPayload>>(
                body: Expression.Convert(
                    expression: payloadParameter, 
                    type: typeof(TPayload)), 
                parameters: payloadParameter).Compile();

            var hoistedPayload = converter.Invoke(inputParameters.Payload);

            return InvokeAsync(
                schematic,
                machine,
                status,
                new InputParameters<TInput, TPayload>(inputParameters.Input, hoistedPayload),
                connectorSettings,
                cancellationToken);
        }

        public abstract Task InvokeAsync(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default);

        public Task<bool> ValidateAsync<TRuntimePayload>(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            InputParameters<TInput, TRuntimePayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(inputParameters.Payload is TPayload);
        }
    }
}
