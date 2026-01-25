namespace Asm.MooBank.Core.Tests.Support;

internal class TestModels
{
    // Common IDs used across tests
    public static readonly Guid UserId = new("35462a0c-d902-41cb-bbee-de7acb943739");
    public static readonly Guid FamilyId = new("cb0a08af-2054-4873-9add-c259916d2d43");
    public static readonly Guid OtherFamilyId = new("1e658c80-3c5c-4cd0-95dd-fa09f6edb9e1");
    public static readonly Guid AccountId = new("f1b1b1b1-1b1b-1b1b-1b1b-1b1b1b1b1b1b");
    public static readonly Guid GroupId = new("243e7609-8868-4e0e-996d-31d16ddbe220");
    public static readonly Guid InstitutionId = new("841abac9-db4e-4a7a-81d1-561b04c2f5c4");
    public static readonly Guid TransactionId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    public static readonly Guid SplitId = new("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ForecastPlanId = new("22222222-2222-2222-2222-222222222222");
    public static readonly Guid VirtualInstrumentId = new("33333333-3333-3333-3333-333333333333");

    // User that owns the account
    public static readonly MooBank.Models.User AccountHolder = new()
    {
        Id = UserId,
        EmailAddress = "test@mclachlan.family",
        FamilyId = FamilyId,
        Currency = "AUD",
        Accounts = [AccountId],
        SharedAccounts = [],
    };

    // User that has shared access to the account
    public static readonly MooBank.Models.User FamilyMember = new()
    {
        Id = new Guid("5a0cda81-3ab6-43d3-85e9-fa0e323881ff"),
        EmailAddress = "family@mclachlan.family",
        FamilyId = FamilyId,
        Currency = "AUD",
        Accounts = [],
        SharedAccounts = [AccountId],
    };

    // User from different family
    public static readonly MooBank.Models.User OtherUser = new()
    {
        Id = new Guid("77777777-7777-7777-7777-777777777777"),
        EmailAddress = "other@example.com",
        FamilyId = OtherFamilyId,
        Currency = "USD",
        Accounts = [],
        SharedAccounts = [],
    };
}
