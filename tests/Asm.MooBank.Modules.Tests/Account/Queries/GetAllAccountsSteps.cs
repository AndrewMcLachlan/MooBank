using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Modules.Accounts.Queries;

namespace Asm.MooBank.Modules.Tests.Account.Queries
{
    [Binding]
    internal class GetAllAccountsSteps : StepDefinitionBase
    {
        private IEnumerable<InstitutionAccount> _result;

        [Given(@"I have the set of generated accounts")]
        public void GivenIHaveTheSetOfGeneratedAccounts()
        {
        }

        [When(@"I call GetAllHandler\.Handle")]
        public async Task WhenICallGetAllHandler_Handle()
        {
            GetAllHandler getAllHandler = new(Entities.InstitutionAccounts, Models.AccountHolder, Mocks.CurrencyConverterMock.Object);
            _result = await getAllHandler.Handle(new GetAll(), new System.Threading.CancellationToken());
        }

        [Then(@"the accounts with the following Ids are returned")]
        public void ThenTheAccountsWithTheFollowingIdsAreReturned(Table table)
        {
            var ids = table.Rows.Select(r => Guid.Parse(r[0]));

            var inspectors = ids.Select<Guid, Action<InstitutionAccount>>(i => ia => Assert.Equal(i, ia.Id)).ToArray();

            Assert.Collection(_result, inspectors);
        }
    }
}
