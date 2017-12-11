using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace REstate.Schematics
{
    public class ExceptionInput<TInput> 
        : IExceptionInput<TInput>
    {
        public ExceptionInput(TInput input)
        {
            Input = input;
        }

        [Required]
        public TInput Input { get; set; }
    }
}
