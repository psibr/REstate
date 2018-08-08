using System;
using System.Threading.Tasks;
using NaturalSchematicExamples;
using REstate.Natural;
using Xunit;

namespace REstate.Tests.Features.Context
{
    public partial class REstateContext<TState, TInput>
        : REstateContext
    {
        public INaturalSchematic CurrentNaturalSchematic { get; set; }

        #region GIVEN
        public Task Given_a_NaturalSchematic()
        {
            CurrentNaturalSchematic = CurrentHost.Agent().ConstructSchematic<ProvisioningSystem>();

            return Task.CompletedTask;
        }

        public async Task Given_a_NaturalSchematic_is_stored(INaturalSchematic naturalSchematic)
        {
            await CurrentHost.Agent()
                .GetStateEngine<TypeState, TypeState>()
                .StoreSchematicAsync(naturalSchematic);
        }
        #endregion

        #region WHEN
        public async Task When_a_NaturalSchematic_is_retrieved(string schematicName)
        {
            try
            {
                CurrentNaturalSchematic = new NaturalSchematic(
                    await CurrentHost.Agent()
                        .GetStateEngine<TypeState, TypeState>()
                        .GetSchematicAsync(schematicName));
            }
            catch(Exception ex)
            {
                CurrentException = ex;
            }
        }
        #endregion

        #region THEN
        public Task Then_no_exception_is_thrown()
        {
            Assert.Null(CurrentException);

            return Task.CompletedTask;
        }
        #endregion
    }
}