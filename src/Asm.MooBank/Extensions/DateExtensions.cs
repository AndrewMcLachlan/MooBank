namespace System;

public static class DateExtensions
{
    public static int DifferenceInMonths(this DateOnly date, DateOnly other) => Math.Abs((date.Year - other.Year) * 12 + (date.Month - other.Month));

    public static int DifferenceInMonths(this DateTime date, DateTime other) => Math.Abs((date.Year - other.Year) * 12 + (date.Month - other.Month));
}
