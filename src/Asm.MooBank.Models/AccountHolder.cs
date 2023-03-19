using System;

namespace Asm.MooBank.Models;

public partial record AccountHolder
{
    public Guid Id { get; set; }

    public string EmailAddress { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }
}
