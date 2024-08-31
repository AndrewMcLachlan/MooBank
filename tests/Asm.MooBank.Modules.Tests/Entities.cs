using Asm.Domain;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Modules.Tests;
public class Entities
{
    public readonly Faker<InstitutionAccount> InstitutionAccountsFaker = new();

    public readonly IQueryable<InstitutionAccount> InstitutionAccounts;

    public readonly InstitutionAccount Account = new(Models.AccountId)
    {
        Controller = MooBank.Models.Controller.Manual,
        Currency = "AUD",
        Balance = 1000,
        Name = "Test Account",
        AccountType = MooBank.Models.AccountType.Transaction,
        LastTransaction = DateOnly.FromDateTime(DateTime.Today),
        Description = "Test Account Description",
        Owners =
        [
            new()
            {
                UserId = Models.UserId,
                User = new Domain.Entities.User.User(Models.UserId)
                {
                    EmailAddress = "test@mclachlan.family",
                    FamilyId = Models.FamilyId,
                }
            }
        ]
    };

    public readonly Domain.Entities.User.User User = new Domain.Entities.User.User(Models.UserId)
    {
        EmailAddress = "test@mclachlan.family",
        FamilyId = Models.FamilyId,
    };

    public readonly Domain.Entities.User.User FamilyUser = new(new("5a0cda81-3ab6-43d3-85e9-fa0e323881ff"))
    {
        EmailAddress = "test2@mclachlan.family",
        FamilyId = Models.FamilyId,
    };

    public readonly Domain.Entities.User.User OtherUser = new(new("5a0cda81-3ab6-43d3-85e9-fa0e323881ff"))
    {
        EmailAddress = "test2@mclachlan.family",
        FamilyId = new("1e658c80-3c5c-4cd0-95dd-fa09f6edb9e1"),
    };


    public Entities()
    {
        InstitutionAccountsFaker
            .CustomInstantiator(f => new InstitutionAccount(f.Random.Guid()) { Name = ""})
            .RuleFor(a => a.Owners, f => f.IndexFaker < 5 ? [new() { UserId = Models.UserId, User = User }] : [new() { UserId = FamilyUser.Id, User = FamilyUser }])
            .RuleFor(a => a.ShareWithFamily, f => f.IndexFaker == 5)
            ;

        var accounts = InstitutionAccountsFaker.UseSeed(1).Generate(10);
        accounts.Insert(0, Account);
        InstitutionAccounts = CreateDbSetMock(accounts).Object;
    }

    private static Mock<DbSet<T>> CreateDbSetMock<T>(IEnumerable<T> data) where T : Entity => CreateDbSetMock(data.AsQueryable());

    private static Mock<DbSet<T>> CreateDbSetMock<T>(IQueryable<T> data) where T : Entity
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(data.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());
        mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new AsyncEnumerator<T>(data.GetEnumerator()));


        return mockSet;
    }

    /*   public IQueryable<InstitutionAccount> GetInstitutionAccounts()
       {
           return new List<InstitutionAccount>
           {
               new InstitutionAccount(new Guid("35462a0c-d902-41cb-bbee-de7acb943739"))
               {
                   Controller = Controller.Manual,
                   Currency = "AUD",
                   Balance = 1000,
                   Name = "Test Account",
                   AccountType = AccountType.Transaction,
                   LastTransaction = DateOnly.FromDateTime(DateTime.Today),
                   Description = "Test Account Description",
               },
               new InstitutionAccount(new Guid("35462a0c-d902-41cb-bbee-de7acb943739"))
               {
                   Controller = Controller.Manual,
                   Currency = "AUD",
                   Balance = 1000,
                   Name = "Test Account",
                   AccountType = AccountType.Transaction,
                   LastTransaction = DateOnly.FromDateTime(DateTime.Today),
                   Description = "Test Account Description",
               },
               new InstitutionAccount(new Guid("35462a0c-d902-41cb-bbee-de7acb943739"))
               {
                   Controller = Controller.Manual,
                   Currency = "AUD",
                   Balance = 1000,
                   Name = "Test Account",
                   AccountType = AccountType.Transaction,
                   LastTransaction = DateOnly.FromDateTime(DateTime.Today),
                   Description = "Test Account Description",
               },
               new InstitutionAccount(new Guid("35462a0c-d902-41cb-bbee-de7acb943739"))
               {
                   Controller = Controller.Manual,
                   Currency = "AUD",
                   Balance = 1000,
                   Name = "Test Account",
                   AccountType = AccountType.Transaction,
                   LastTransaction = DateOnly.FromDateTime(DateTime.Today),
                   Description = "Test Account Description",
               },
               new InstitutionAccount(new Guid("35462a0c-d902-41cb-bbee-de7acb943739"))
               {
                   Controller = Controller.Manual,
                   Currency = "AUD",
                   Balance = 1000,
                   Name = "Test Account",
                   AccountType = AccountType.Transaction,
                   LastTransaction = DateOnly.FromDateTime(DateTime.Today),
                   Description = "Test Account Description",
               },
               new InstitutionAccount(new Guid("35462a0c-d902-41cb-bbee-de7acb943739"))
               {
                   Controller = Controller.Manual,
                   Currency = "AUD",
                   Balance = 1000,
                   Name = "Test Account",
                   AccountType =

   */




}
