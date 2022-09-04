using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Asm.MooBank.Data.Entities;

namespace Asm.MooBank.Security
{
    public interface IUserDataProvider
    {
        Guid CurrentUserId { get; }

        Task<AccountHolder> GetCurrentUser();

        Task<AccountHolder> GetUser(Guid id);
    }
}
