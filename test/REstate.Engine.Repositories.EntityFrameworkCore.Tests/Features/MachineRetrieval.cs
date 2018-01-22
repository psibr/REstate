﻿using System.Threading.Tasks;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios.Contextual;
using LightBDD.Framework.Scenarios.Extended;
using REstate.Engine.Repositories.EntityFrameworkCore.Tests.Features.Context;
using REstate.Tests.Features.Templates;

// ReSharper disable InconsistentNaming

namespace REstate.Engine.Repositories.EntityFrameworkCore.Tests.Features
{
    [FeatureDescription(@"
In order to use familiar storage
As a developer
I want to retrieve previously created Machines stored using Entity Framework Core")]
    [ScenarioCategory("Machine Retrieval")]
    [ScenarioCategory("EntityFrameworkCore")]
    public class MachineRetrieval
        : MachineRetrievalScenarios<REstateEntityFrameworkCoreContext<string, string>>
    {        
        protected override Task<CompositeStep> Given_host_configuration_is_applied()
        {
            return Task.FromResult(
                CompositeStep
                    .DefineNew()
                    .WithContext(Context)
                    .AddAsyncSteps(
                        _ => _.Given_EntityFrameworkCore_is_the_registered_repository())
                    .Build());
        }
    }
}
