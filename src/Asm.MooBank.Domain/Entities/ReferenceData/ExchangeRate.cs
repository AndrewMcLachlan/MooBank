﻿using System.Diagnostics.CodeAnalysis;

namespace Asm.MooBank.Domain.Entities.ReferenceData;
public class ExchangeRate([DisallowNull] int id) : KeyedEntity<int>(id)
{
    public ExchangeRate() : this(default) { }

    public string From { get; set; } = null!;

    public string To { get; set; } = null!;

    public decimal Rate { get; set; }

    public decimal ReverseRate { get; private set; }

    public DateTimeOffset LastUpdated { get; set; }
}