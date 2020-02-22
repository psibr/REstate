using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using REstate.Tests.Features.Context;

// ReSharper disable InconsistentNaming

namespace REstate.Tests.Features
{
    [FeatureDescription(@"
In order to utilize REstate to track state
As a developer
I want to send input to machines to change state")]
    [ScenarioCategory("Machine Transitions")]
    public abstract class MachineTransitionScenarios<TContext>
        : REstateFeature<TContext>
        where TContext : REstateContext<string, string>, new()
    {
        [Scenario]
        public async Task A_Machine_can_transition_between_states()
        {
            var uniqueId = Guid.NewGuid().ToString();

            var schematicName = uniqueId;

            await Runner.WithContext(Context).RunScenarioAsync(
                _ =>   Given_host_configuration_is_applied(),
                _ => _.Given_a_Schematic(agent => agent
                    .CreateSchematic<string, string>(schematicName)
                        .WithState("1", state => state.AsInitialState())
                        .WithState("2", state => state
                            .WithTransitionFrom("1", "++", null))
                        .Build()),
                _ => _.Given_a_Machine_exists_with_MachineId_MACHINEID(_.CurrentSchematic, uniqueId),
                _ => _.When_input_is_sent("++"),
                _ => _.Then_no_exception_was_thrown(),
                _ => _.Then_the_Machines_state_is_STATE("2"));
        }

        [Scenario]
        public async Task A_Machine_can_transition_between_states_with_a_payload()
        {
            var uniqueId = Guid.NewGuid().ToString();

            var schematicName = uniqueId;

            var payload = "payload";

            await Runner.WithContext(Context).RunScenarioAsync(
                _ => Given_host_configuration_is_applied(),
                _ => _.Given_a_Schematic(agent => agent
                    .CreateSchematic<string, string>(schematicName)
                    .WithState("1", state => state.AsInitialState())
                    .WithState("2", state => state
                        .WithTransitionFrom("1", "++", null))
                    .Build()),
                _ => _.Given_a_Machine_exists_with_MachineId_MACHINEID(_.CurrentSchematic, uniqueId),
                _ => _.When_input_and_a_payload_is_sent("++", payload),
                _ => _.Then_no_exception_was_thrown(),
                _ => _.Then_the_Machines_state_is_STATE("2"));
        }

        [Scenario]
        public async Task A_Machine_can_track_a_StateBag()
        {
            var uniqueId = Guid.NewGuid().ToString();

            var schematicName = uniqueId;

            var stateBag = new Dictionary<string, string>
            {
                ["SomeKey"] = "SomeValue"
            };

            await Runner.WithContext(Context).RunScenarioAsync(
                _ => Given_host_configuration_is_applied(),
                _ => _.Given_a_Schematic(agent => agent
                    .CreateSchematic<string, string>(schematicName)
                    .WithState("1", state => state.AsInitialState())
                    .WithState("2", state => state
                        .WithTransitionFrom("1", "++", null))
                    .Build()),
                _ => _.Given_a_Machine_exists_with_MachineId_MACHINEID(_.CurrentSchematic, uniqueId),
                _ => _.When_input_is_sent_with_a_CommitNumber_and_StateBag("++", 0, stateBag),
                _ => _.Then_no_exception_was_thrown(),
                _ => _.Then_the_Machines_state_is_STATE("2"),
                _ => _.Then_the_StateBag_has_a_matching_entry(new KeyValuePair<string, string>("SomeKey", "SomeValue")));
        }

        [Scenario]
        public async Task A_Machine_will_throw_a_StateConflictException_if_LastCommitNumber_is_provided_but_does_not_match()
        {
            var uniqueId = Guid.NewGuid().ToString();

            var schematicName = uniqueId;

            var stateBag = new Dictionary<string, string>
            {
                ["SomeKey"] = "SomeValue"
            };

            await Runner.WithContext(Context).RunScenarioAsync(
                _ => Given_host_configuration_is_applied(),
                _ => _.Given_a_Schematic(agent => agent
                    .CreateSchematic<string, string>(schematicName)
                    .WithState("1", state => state.AsInitialState())
                    .WithState("2", state => state
                        .WithTransitionFrom("1", "++", null))
                    .Build()),
                _ => _.Given_a_Machine_exists_with_MachineId_MACHINEID(_.CurrentSchematic, uniqueId),
                _ => _.When_input_is_sent_with_a_CommitNumber_and_StateBag("++", 1, stateBag),
                _ => _.Then_a_StateConflictException_was_thrown());
        }

        [Scenario]
        public async Task A_Machine_can_update_a_StateBag()
        {
            var uniqueId = Guid.NewGuid().ToString();

            var schematicName = uniqueId;

            var stateBagAtCommitNumber0 = new Dictionary<string, string>
            {
                ["i"] = "0"
            };

            var stateBagAtCommitNumber1 = new Dictionary<string, string>
            {
                ["i"] = "1"
            };

            await Runner.WithContext(Context).RunScenarioAsync(
                _ => Given_host_configuration_is_applied(),
                _ => _.Given_a_Schematic(agent => agent
                    .CreateSchematic<string, string>(schematicName)
                    .WithState("1", state => state.AsInitialState())
                    .WithState("2", state => state
                        .WithTransitionFrom("1", "++", null))
                    .WithState("3", state => state
                        .WithTransitionFrom("2", "++", null))
                    .Build()),
                _ => _.Given_a_Machine_exists_with_MachineId_MACHINEID(_.CurrentSchematic, uniqueId),
                _ => _.When_input_is_sent_with_a_CommitNumber_and_StateBag("++", 0, stateBagAtCommitNumber0),
                _ => _.When_input_is_sent_with_a_CommitNumber_and_StateBag("++", 1, stateBagAtCommitNumber1),
                _ => _.Then_no_exception_was_thrown(),
                _ => _.Then_the_Machines_state_is_STATE("3"),
                _ => _.Then_the_StateBag_has_a_matching_entry(new KeyValuePair<string, string>("i", "1")));
        }

        [Scenario]
        public async Task A_Machine_will_not_replace_a_StateBag_with_null()
        {
            var uniqueId = Guid.NewGuid().ToString();

            var schematicName = uniqueId;

            var stateBagAtCommitNumber0 = new Dictionary<string, string>
            {
                ["i"] = "0"
            };

            IDictionary<string, string> stateBagAtCommitNumber1 = null;

            await Runner.WithContext(Context).RunScenarioAsync(
                _ => Given_host_configuration_is_applied(),
                _ => _.Given_a_Schematic(agent => agent
                    .CreateSchematic<string, string>(schematicName)
                    .WithState("1", state => state.AsInitialState())
                    .WithState("2", state => state
                        .WithTransitionFrom("1", "++", null))
                    .WithState("3", state => state
                        .WithTransitionFrom("2", "++", null))
                    .Build()),
                _ => _.Given_a_Machine_exists_with_MachineId_MACHINEID(_.CurrentSchematic, uniqueId),
                _ => _.When_input_is_sent_with_a_CommitNumber_and_StateBag("++", 0, stateBagAtCommitNumber0),
                _ => _.When_input_is_sent_with_a_CommitNumber_and_StateBag("++", 1, stateBagAtCommitNumber1),
                _ => _.Then_no_exception_was_thrown(),
                _ => _.Then_the_Machines_state_is_STATE("3"),
                _ => _.Then_the_StateBag_has_a_matching_entry(new KeyValuePair<string, string>("i", "0")));
        }
    }

    public class MachineTransition
        : MachineTransitionScenarios<REstateContext<string, string>>
    {

    }
}

