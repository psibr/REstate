using System;
using System.Threading.Tasks;
using REstate.Schematics;
using Xunit;

namespace REstate.Tests.Features.Context
{
    public partial class REstateContext<TState, TInput> 
        : REstateContext
    {
        public ISchematic<TState, TInput> CurrentSchematic { get; set; }

        #region GIVEN
        public Task Given_a_Schematic_with_an_initial_state_INITIALSTATE(string schematicName, TState initialState)
        {
            CurrentSchematic = CurrentHost.Agent()
                .CreateSchematic<TState, TInput>(schematicName)
                .WithState(initialState, state => state
                    .AsInitialState())
                .Build();

            return Task.CompletedTask;
        }

        public async Task Given_a_Schematic_is_stored(ISchematic<TState, TInput> schematic)
        {
            await CurrentHost.Agent()
                .GetStateEngine<TState, TInput>()
                .StoreSchematicAsync(schematic);
        }

        public async Task Given_a_Schematic_with_an_action_is_stored(string schematicName, TState state, ConnectorKey connectorKey)
        {
            var schematic = CurrentHost.Agent()
                .CreateSchematic<TState, TInput>(schematicName)
                .WithState(state, _ => _
                    .AsInitialState()
                    .WithAction(connectorKey))
                .Build();

            await CurrentHost.Agent()
                .GetStateEngine<TState, TInput>()
                .StoreSchematicAsync(schematic);
        }

        public Task Given_a_Schematic(Func<IAgent, ISchematic<TState, TInput>> schematic)
        {
            CurrentSchematic = schematic.Invoke(CurrentHost.Agent());

            return Task.CompletedTask;
        }
        #endregion

        #region WHEN
        public async Task When_a_Schematic_is_retrieved(string schematicName)
        {
            CurrentSchematic = await CurrentHost.Agent()
                .GetStateEngine<TState, TInput>()
                .GetSchematicAsync(schematicName);
        }
        #endregion

        #region THEN
        public Task Then_the_connector_key_should_have_a_valid_identifier(TState state, ConnectorKey connectorKey)
        {
            Assert.NotNull(CurrentSchematic);
            Assert.NotNull(CurrentSchematic.States[state].Action.ConnectorKey.Identifier);
            Assert.Equal(connectorKey.Identifier, CurrentSchematic.States[state].Action.ConnectorKey.Identifier);

            return Task.CompletedTask;
        }
        #endregion
    }
}