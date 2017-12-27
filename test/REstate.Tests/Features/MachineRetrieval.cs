using System;
using System.Diagnostics;
using System.Reflection;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios.Contextual;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.XUnit2;
using REstate.Tests.Features.Context;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming

namespace REstate.Tests.Features
{
    [FeatureDescription(@"
In order to utilize REstate's functionality
As a developer
I want to retrieve previously created machines")]
    [ScenarioCategory("Machine Retrieval")]
    public class MachineRetrieval
        : FeatureFixture
    {
        [Scenario]
        public void A_Machine_can_be_retrieved()
        {
            var uniqueId = MethodBase.GetCurrentMethod().Name;

            var schematicName = uniqueId;
            var machineId = uniqueId;

            Runner.WithContext<REstateContext<string, string>>().RunScenario(
                _ => _.Given_a_new_host(),
                _ => _.Given_a_Schematic_with_an_initial_state_INITIALSTATE(schematicName, "Initial"),
                _ => _.Given_a_Machine_exists_with_MachineId_MACHINEID(_.CurrentSchematic, machineId),
                _ => _.When_a_Machine_is_retrieved_with_MachineId_MACHINEID(machineId),
                _ => _.Then_the_Machine_is_valid(_.CurrentMachine));
        }

        #region Constructor
        public MachineRetrieval(ITestOutputHelper output)
            : base(output)
        {
        }
        #endregion
    }
}

