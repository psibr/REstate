using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace REstate.Schematics
{
    public class GuardConnector
        : IGuard
    {
        [Required]
        public string ConnectorKey { get; set; }

        [Required]
        public IDictionary<string, string> Configuration { get; set; }
        
        public string Description { get; set; }

        IReadOnlyDictionary<string, string> IGuard.Settings =>
            new ReadOnlyDictionary<string, string>(Configuration ?? new Dictionary<string, string>(0));
    }
}