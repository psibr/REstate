﻿using System.Threading.Tasks;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using REstate.Engine.Repositories.Redis.Tests.Features.Context;
using REstate.Tests.Features;

// ReSharper disable InconsistentNaming

namespace REstate.Engine.Repositories.Redis.Tests.Features
{
    [FeatureDescription(@"
In order to use familiar storage
As a developer
I want to delete previously created Machines stored using Redis")]
    [ScenarioCategory("Machine Deletion")]
    [ScenarioCategory("Redis")]
    public class MachineDeletion
        : MachineDeletionScenarios<REstateRedisContext<string, string>>
    {
        protected override Task<CompositeStep> Given_host_configuration_is_applied()
        {
            return Task.FromResult(
                CompositeStep
                    .DefineNew()
                    .WithContext(Context)
                    .AddAsyncSteps(
                        _ => _.Given_a_new_host(),
                        _ => _.Given_Redis_is_the_registered_repository())
                    .Build());
        }
    }
}
