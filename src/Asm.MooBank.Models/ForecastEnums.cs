namespace Asm.MooBank.Models;

public enum AccountScopeMode : byte
{
    AllAccounts = 0,
    SelectedAccounts = 1
}

public enum StartingBalanceMode : byte
{
    CalculatedCurrent = 0,
    ManualAmount = 1
}

public enum PlannedItemType : byte
{
    Expense = 0,
    Income = 1
}

public enum PlannedItemDateMode : byte
{
    FixedDate = 0,
    Schedule = 1,
    FlexibleWindow = 2
}

public enum AllocationMode : byte
{
    EvenlySpread = 0,
    AllAtEnd = 1
}
