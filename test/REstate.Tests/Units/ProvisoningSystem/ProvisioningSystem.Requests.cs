using REstate.Engine;
using REstate.Engine.Connectors;
using REstate.Schematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Tests.Units
{
    public partial class ProvisioningSystem
    {
        public interface IProvisionedRequest
        {
        }

        public class ReserveRequest : IProvisionedRequest
        {

        }

        public class ReleaseRequest : IProvisionedRequest
        {

        }

        public class ProvisioningCompleteRequest : IProvisionedRequest
        {
            /// <summary>
            /// The initial reservation that triggered provisioning
            /// </summary>
            public ReserveRequest Reservation { get; set; }
        }

        public class DeprovisionRequest
        {
        }
    }
}
