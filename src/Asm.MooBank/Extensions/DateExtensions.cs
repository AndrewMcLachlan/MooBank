namespace System;

public static class DateExtensions
{
    public static DateOnly ToStartOfMonth(this DateOnly date)
    {
        return new DateOnly(date.Year, date.Month, 1);
    }

    public static DateOnly ToEndOfMonth(this DateOnly date)
    {
        return new DateOnly(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
    }

    public static DateTime ToStartOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    public static DateTime ToEndOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
    }
}
