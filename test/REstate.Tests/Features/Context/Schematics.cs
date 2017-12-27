using System;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate.Tests.Features.Context
{
    public partial class REstateContext<TState, TInput> 
        : REstateContext
    {
        public Schematic<TState, TInput> CurrentSchematic { get; set; }

        public Task Given_a_Schematic_with_an_initial_state_INITIALSTATE(string schematicName, TState initialState)
        {
            CurrentSchematic = CurrentHost.Agent()
                .CreateSchematic<TState, TInput>(schematicName)
                .WithState(initialState, state => state
                    .AsInitialState())
                .Build();

            return Task.CompletedTask;
        }

        public async Task Given_a_Schematic_is_stored(Schematic<TState, TInput> schematic)
        {
            await CurrentHost.Agent()
                .GetStateEngine<TState, TInput>()
                .StoreSchematicAsync(schematic);
        }
    }
}