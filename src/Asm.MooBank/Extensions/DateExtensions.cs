namespace System;

public static class DateExtensions
{
    public static int DifferenceInMonths(this DateOnly date, DateOnly other)
    {
        var months = Math.Abs((date.Year - other.Year) * 12 + (date.Month - other.Month));

        // Round part months up or down
        return date.Day - other.Day < 0 ? months + 1 :
               date.Day - other.Day > 0 ? months - 1 :
               months;
    }

    public static int DifferenceInMonths(this DateTime date, DateTime other)
    {
        var months = Math.Abs((date.Year - other.Year) * 12 + (date.Month - other.Month));

        // Round part months up or down
        return date.Day - other.Day < 0 ? months + 1 :
               date.Day - other.Day > 0 ? months - 1 :
               months;
    }

    public static DateOnly ToStartOfMonth(this DateOnly date)
    {
        return new DateOnly(date.Year, date.Month, 1);
    }

    public static DateOnly ToEndOfMonth(this DateOnly date)
    {
        return new DateOnly(date.Year, date.Month+1, 1).AddDays(-1);
    }

    public static DateTime ToStartOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    public static DateTime ToEndOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month + 1, 1).AddDays(-1);
    }
}
