using System;
using MagicOnion;
using MagicOnion.Server;
using REstate.Interop.Models;
#pragma warning disable 1998

namespace REstate.Interop.Services
{
    public interface IStateMachineService
        : IService<IStateMachineService>
    {
        UnaryResult<SendResponse> SendAsync(SendRequest t);
    }

    public class StateMachineService
        : ServiceBase<IStateMachineService>
    {
        public async UnaryResult<SendResponse> SendAsync(SendRequest t)
        {
            return new SendResponse
            {
                MachineId = t.MachineId,
                CommitTag = Guid.NewGuid(),
                StateBytes = new byte[1]
            };
        }
    }
}