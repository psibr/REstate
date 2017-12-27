﻿using System;
using System.Collections.Generic;
using System.Text;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios.Contextual;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.XUnit2;
using REstate.Remote.Tests.Features.Context;
using Xunit;
using Xunit.Abstractions;

namespace REstate.Remote.Tests.Features
{
    [FeatureDescription(@"
In order to support cloud scaling
As a developer
I want to connect using gRPC to a remote REstate server")]
    [ScenarioCategory("REstate gRPC Server")]
    [ScenarioCategory("Remote")]
    [ScenarioCategory("gRPC")]
    [Collection("gRPC")]
    public class GrpcServer
        : FeatureFixture
    {
        [Scenario]
        public void REstate_gRPC_Server_Sets_BoundPorts_on_start()
        {
            Runner.WithContext<REstateRemoteContext<string, string>>().RunScenario(
                _ => _.Given_a_new_host(),
                _ => _.When_a_REstate_gRPC_Server_is_created_and_started(),
                _ => _.Then_REstate_gRPC_Server_has_bound_ports());
        }

        #region Constructor
        public GrpcServer(ITestOutputHelper output)
            : base(output)
        {
        }
        #endregion
    }
}
