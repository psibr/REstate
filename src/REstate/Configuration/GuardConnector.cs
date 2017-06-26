using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace REstate.Configuration
{
    public class GuardConnector
    {
        [Required]
        public string ConnectorKey { get; set; }

        [Required]
        public IDictionary<string, string> Configuration { get; set; }
        
        public string Description { get; set; }
    }
}