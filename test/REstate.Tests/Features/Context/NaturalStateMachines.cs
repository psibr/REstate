using System;
using System.Threading.Tasks;
using REstate.Natural;

namespace REstate.Tests.Features.Context
{
    public partial class REstateContext<TState, TInput>
        : REstateContext
    {
        public INaturalStateMachine CurrentNaturalStateMachine { get; set; }

        #region GIVEN
        public async Task Given_a_NaturalStateMachine(INaturalSchematic naturalSchematic)
        {
            try
            {
                CurrentNaturalStateMachine = await CurrentHost.Agent()
                    .GetNaturalStateEngine()
                    .CreateMachineAsync(naturalSchematic);
            }
            catch (Exception ex)
            {
                CurrentException = ex;
            }
        }
        #endregion

        #region WHEN
        public async Task When_a_NaturalStateMachine_is_created(INaturalSchematic naturalSchematic)
        {
            try
            {
                CurrentNaturalStateMachine = await CurrentHost.Agent()
                    .GetNaturalStateEngine()
                    .CreateMachineAsync(naturalSchematic);
            }
            catch(Exception ex)
            {
                CurrentException = ex;
            }
        }
        #endregion

        #region THEN
        #endregion
    }
}