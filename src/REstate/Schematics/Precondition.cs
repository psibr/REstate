using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace REstate.Schematics
{
    public class Precondition
        : IPrecondition
    {
        private IDictionary<string, string> _configuration;

        [Required]
        public ConnectorKey ConnectorKey { get; set; }

        [Required]
        public IDictionary<string, string> Configuration
        {
            get => _configuration;
            set => _configuration = new Dictionary<string, string>(value, StringComparer.OrdinalIgnoreCase);
        }

        public string Description { get; set; }

        IReadOnlyDictionary<string, string> IPrecondition.Settings =>
            new ReadOnlyDictionary<string, string>(Configuration 
                ?? new Dictionary<string, string>(0));
    }
}
