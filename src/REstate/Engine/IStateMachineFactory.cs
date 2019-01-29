using REstate.Schematics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine
{
    public interface IStateMachineFactory<TState, TInput>
    {
        IStateMachine<TState, TInput> Construct(string machineId);

        IStateMachine<TState, TInput> ConstructPreloaded(string machineId, Schematic<TState, TInput> schematic, ReadOnlyDictionary<string, string> metadata);
    }
}
