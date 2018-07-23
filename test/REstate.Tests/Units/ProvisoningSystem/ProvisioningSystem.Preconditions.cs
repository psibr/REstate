using REstate.Engine;
using REstate.Engine.Connectors;
using REstate.Schematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Tests.Units
{
    public partial class ProvisioningSystem
    {
        public class SomethingIsValid
            : IPrecondition<TypeState, TypeState>
        {
            public Task<bool> ValidateAsync<TPayload>(
                ISchematic<TypeState, TypeState> schematic,
                IStateMachine<TypeState, TypeState> machine,
                Status<TypeState> status,
                InputParameters<TypeState, TPayload> inputParameters,
                IReadOnlyDictionary<string, string> connectorSettings,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(true);
            }
        }
    }
}
