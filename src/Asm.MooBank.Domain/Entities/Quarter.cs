using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities;

[Owned]
public class QuarterEntity(int year, int quarterNumber) : IComparable<QuarterEntity>
{
    public int Year { get; set; } = year;

    public int QuarterNumber { get; set; } = quarterNumber;

    public int CompareTo(QuarterEntity? other)
    {
        if (other == null) return 1;
        return ((Quarter)this).CompareTo(other);
    }

    public static implicit operator Quarter(QuarterEntity value)
        => new(value.Year, value.QuarterNumber);

    //public static explicit operator Quarter(QuarterEntity value)
    //=> new(value.Year, value.QuarterNumber);

    public static implicit operator QuarterEntity(Quarter value)
        => new(value.Year, value.QuarterNumber);

    public static implicit operator QuarterEntity(string quarter) => Quarter.Parse(quarter);

    public static implicit operator string(QuarterEntity quarter) => ((Quarter)quarter).ToString();
}
