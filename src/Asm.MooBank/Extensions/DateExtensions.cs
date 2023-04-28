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
}
