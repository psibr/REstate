using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace REstate.Schematics
{
    public class EntryConnector<TInput>
        : IEntryAction<TInput>
    {
        [Required]
        public string ConnectorKey { get; set; }

        [Required]
        public IDictionary<string, string> Configuration { get; set; }
        
        public string Description { get; set; }

        public ExceptionTransition<TInput> FailureTransition { get; set; }

        TInput IEntryAction<TInput>.OnFailureInput =>
            FailureTransition.Input;

        IReadOnlyDictionary<string, string> IEntryAction<TInput>.Settings =>
            new ReadOnlyDictionary<string, string>(Configuration ?? new Dictionary<string, string>(0));
    }
}
