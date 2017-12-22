using System;
using REstate.Schematics;

namespace REstate.Tests.Features.Context
{
    public partial class REstateContext<TState, TInput> 
        : REstateContext
    {
        public Schematic<TState, TInput> CurrentSchematic { get; set; }

        public void Given_a_simple_schematic_with_an_initial_state_INITIALSTATE(TState initialState)
        {
            CurrentSchematic = CurrentHost.Agent()
                .CreateSchematic<TState, TInput>("simple")
                .WithState(initialState, state => state
                    .AsInitialState())
                .Build();
        }
    }
}