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
        /// <summary>
        /// Categorization interface for signals that are
        /// accepted in the <see cref="Provisioned"/> state.
        /// </summary>
        public interface IProvisionedSignal
        {
        }

        public class ReserveSignal : IProvisionedSignal
        {

        }

        public class ReleaseSignal : IProvisionedSignal
        {

        }

        /// <summary>
        /// Informs the provisioning system that provisioning has completed.
        /// </summary>
        public class ProvisioningCompleteSignal : IProvisionedSignal
        {
            /// <summary>
            /// The initial reservation that triggered provisioning
            /// </summary>
            public ReserveSignal Reservation { get; set; }
        }

        public class DeprovisionSignal
        {
        }
    }
}
