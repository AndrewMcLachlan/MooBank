namespace Asm.MooBank.Models;

public readonly struct Quarter : IEquatable<Quarter>, IComparable<Quarter>, IComparable
{
    public int Year { get; }
    public int QuarterNumber { get; }

    public Quarter()
    {
        var q = FromDate(DateTime.Now);
        Year = q.Year;
        QuarterNumber = q.QuarterNumber;
    }

    public Quarter(int startMonth)
    {
        var q = FromDate(DateTime.Now, startMonth);
        Year = q.Year;
        QuarterNumber = q.QuarterNumber;
    }

    public Quarter(int year, int quarterNumber)
    {
        if (quarterNumber < 1 || quarterNumber > 4)
            throw new ArgumentOutOfRangeException(nameof(quarterNumber), "Quarter number must be between 1 and 4.");
        Year = year;
        QuarterNumber = quarterNumber;
    }

    public static bool TryParse(string? quarter, out Quarter result)
    {
        if (quarter == null)
        {
            result = default;
            return false;
        }

        try
        {
            result = Parse(quarter);
            return true;
        }
        catch (FormatException)
        {
            result = default;
            return false;
        }
    }

    public static Quarter Parse(string quarter)
    {
        ArgumentNullException.ThrowIfNull(quarter);

        string[] parts = quarter.Split('-');
        if (parts.Length != 2 || !Int32.TryParse(parts[0], out int year) || !Int32.TryParse(parts[1].AsSpan(1), out int quarterNumber))
        {
            throw new FormatException($"Invalid quarter format: {quarter}");
        }
        return new Quarter(year, quarterNumber);
    }

    public static Quarter FromDate(DateTime date, int startMonth = 1)
    {
        // Adjust the year if the date is before the start month
        int adjustedYear = date.Year;
        if (date.Month < startMonth)
            adjustedYear--;

        // Calculate the zero-based month offset from the start month
        int monthOffset = (date.Month - startMonth + 12) % 12;
        int quarterNumber = (monthOffset / 3) + 1;

        return new Quarter(adjustedYear, quarterNumber);
    }

    public static Quarter FromDate(DateOnly date, int startMonth = 1) =>
        FromDate(date.ToDateTime(TimeOnly.MinValue), startMonth);

    public override string ToString()
    {
        return $"{Year}-Q{QuarterNumber}";
    }

    public override bool Equals(object? obj) =>
    Equals((Quarter?)obj);

    public override int GetHashCode() =>
        HashCode.Combine(Year, QuarterNumber);

    public bool Equals(Quarter other)
    {
        return Year == other.Year && QuarterNumber == other.QuarterNumber;
    }

    int IComparable.CompareTo(object? obj)
    {
        if (obj is null) return 1; // null is considered less than any instance of Quarter
        if (obj is not Quarter other)
            throw new ArgumentException($"Object must be of type {nameof(Quarter)}.", nameof(obj));

        return CompareTo(other);
    }

    public int CompareTo(Quarter other)
    {
        if (Year != other.Year) return Year.CompareTo(other.Year);
        return QuarterNumber.CompareTo(other.QuarterNumber);
    }

    public static implicit operator string(Quarter quarter) => quarter.ToString();

    public static implicit operator Quarter(string quarter) => Parse(quarter);


    public static bool operator ==(Quarter a, Quarter b) => a.Equals(b);

    public static bool operator !=(Quarter a, Quarter b) => !a.Equals(b);

    public static bool operator <(Quarter a, Quarter b) =>
        a.Year < b.Year || (a.Year == b.Year && a.QuarterNumber < b.QuarterNumber);

    public static bool operator >(Quarter a, Quarter b) =>
        a.Year > b.Year || (a.Year == b.Year && a.QuarterNumber > b.QuarterNumber);

    public static bool operator <=(Quarter a, Quarter b) => a < b || a == b;
    public static bool operator >=(Quarter a, Quarter b) => a > b || a == b;
}
