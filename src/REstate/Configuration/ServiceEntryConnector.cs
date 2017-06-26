using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace REstate.Configuration
{
    public class ServiceEntryConnector
    {
        [Required]
        public string ConnectorKey { get; set; }

        [Required]
        public IDictionary<string, string> Configuration { get; set; }

        public string Description { get; set; }
    }
}
