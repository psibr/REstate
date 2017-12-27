using System;
using System.Diagnostics;
using System.Reflection;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios.Contextual;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.XUnit2;
using REstate.Remote.Tests.Features.Context;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming

namespace REstate.Remote.Tests.Features
{
    [FeatureDescription(@"
In order to support cloud scaling
As a developer
I want to retrieve Machines from a remote server")]
    [ScenarioCategory("Machine Retrieval")]
    [ScenarioCategory("Remote")]
    [ScenarioCategory("gRPC")]
    //[Collection("gRPC")]
    public class MachineRetrieval
        : FeatureFixture
    {
        [Scenario]
        public void A_Machine_can_be_retrieved()
        {
            var uniqueId = MethodBase.GetCurrentMethod().Name;

            var schematicName = uniqueId;
            var machineId = uniqueId;

            Runner.WithContext<REstateRemoteContext<string, string>>().RunScenario(
                _ => _.Given_a_new_host(),
                _ => _.Given_a_REstate_gRPC_Server_running_on_the_default_endpoint(),
                _ => _.Given_the_default_agent_is_gRPC_remote_on_default_endpoint(),
                _ => _.Given_a_Schematic_with_an_initial_state_INITIALSTATE(schematicName, "Initial"),
                _ => _.Given_a_Machine_exists_with_MachineId_MACHINEID(_.CurrentSchematic, machineId),
                _ => _.When_a_Machine_is_retrieved_with_MachineId_MACHINEID(machineId),
                _ => _.Then_no_exception_was_thrown(),
                _ => _.Then_the_Machine_is_valid(_.CurrentMachine));
        }

        [Scenario]
        public void A_NonExistant_Machine_cannot_be_retrieved()
        {
            var uniqueId = MethodBase.GetCurrentMethod().Name;

            var machineId = uniqueId;

            Runner.WithContext<REstateRemoteContext<string, string>>().RunScenario(
                _ => _.Given_a_new_host(),
                _ => _.Given_a_REstate_gRPC_Server_running_on_the_default_endpoint(),
                _ => _.Given_the_default_agent_is_gRPC_remote_on_default_endpoint(),
                _ => _.When_a_Machine_is_retrieved_with_MachineId_MACHINEID(machineId),
                _ => _.Then_MachineDoesNotExistException_is_thrown());
        }

        #region Constructor
        public MachineRetrieval(ITestOutputHelper output)
            : base(output)
        {
        }
        #endregion
    }
}

