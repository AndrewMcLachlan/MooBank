using System;

namespace Asm.MooBank.Data.Entities
{
    public partial class VirtualAccount
    {
        public static implicit operator Models.VirtualAccount(VirtualAccount account)
        {
            return new Models.VirtualAccount
            {
                Id = account.VirtualAccountId,
                Name = account.Name,
                Description = account.Description,
                Balance = account.Balance,
            };
        }

        public static explicit operator VirtualAccount(Models.VirtualAccount account)
        {
            return new VirtualAccount
            {
                VirtualAccountId = account.Id == Guid.Empty ? Guid.NewGuid() : account.Id,
                Name = account.Name,
                Description = account.Description,
                Balance = account.Balance,
            };
        }
    }
}
