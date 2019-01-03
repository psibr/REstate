using System;
using System.Threading.Tasks;
using NaturalSchematicExamples;
using REstate.Natural;

namespace REstate.Tests.Features.Context
{
    public partial class REstateNaturalContext : REstateContext<TypeState, TypeState>
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

        public async Task When_a_signal_is_sent<TSignal>(INaturalStateMachine naturalMachine, TSignal signal)
        {
            try
            {
                CurrentStatus = await naturalMachine.SignalAsync(new ProvisioningSystem.ReserveSignal())
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                CurrentException = ex;
            }
        }
        #endregion

        #region THEN
        #endregion
    }
}