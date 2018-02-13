using System.Threading.Tasks;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios.Contextual;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.XUnit2;
using REstate.Tests.Features.Context;

// ReSharper disable InconsistentNaming

namespace REstate.Tests.Features.Templates
{
    public class REstateFeature<TContext>
        : FeatureFixture
        where TContext : REstateContext, new()
    {
        protected REstateFeature()
        {
            Context = new TContext();
        }

        protected virtual Task<CompositeStep> Given_host_configuration_is_applied()
        {
            return Task.FromResult(
                CompositeStep
                    .DefineNew()
                    .WithContext(Context)
                    .AddAsyncSteps(context => context.Given_a_new_host())
                    .Build());
        }

        protected TContext Context { get; }
    }
}