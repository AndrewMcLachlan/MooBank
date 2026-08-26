using Asm.Domain;
using Asm.MooBank.DemoData;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Utility;
using Asm.MooBank.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;
using GeneratedTransaction = Asm.MooBank.DemoData.Transaction;
using TagEntity = Asm.MooBank.Domain.Entities.Tag.Tag;
using UserEntity = Asm.MooBank.Domain.Entities.User.User;
using UtilityAccount = Asm.MooBank.Domain.Entities.Utility.Account;

namespace Asm.MooBank.Services.DemoData;

internal interface IDemoDataWriter
{
    /// <summary>
    /// Writes the given month into the configured demo instruments.
    /// </summary>
    Task Extend(DateOnly month, CancellationToken cancellationToken = default);
}

/// <summary>
/// Writes a month of demo data.
/// </summary>
/// <remarks>
/// Resolved only from a scope that already has the demo family's identity set, because several of
/// its dependencies take the current user in their own constructors and would throw before this
/// ever ran. <see cref="DemoDataService"/> owns that sequencing.
/// </remarks>
internal class DemoDataWriter(
    IOptions<DemoDataOptions> options,
    IQueryable<DomainTransaction> transactions,
    IQueryable<TagEntity> tags,
    IQueryable<LogicalAccount> logicalAccounts,
    ITransactionRepository transactionRepository,
    IAccountRepository utilityAccounts,
    IRunRulesService runRulesService,
    IUnitOfWork unitOfWork,
    ILogger<DemoDataWriter> logger) : IDemoDataWriter
{
    private const string Source = "Demo Data";

    // Terms of the two loans the backfill created. The job continues them; it does not re-derive
    // them, so these must match the figures in DemoMortgage.sql and DemoCarLoan.sql.
    private const decimal MortgageRate = 0.055m;
    private const decimal CarLoanRate = 0.075m;
    private const decimal CarLoanRepayment = 701.35m;

    // Utility bills are quarterly. The tariff is not held here at all -- it is read back off the
    // account's last bill, so it stays whatever the data says.
    private const int MinimumBillingDays = 75;

    // The gross-up from the net salary on checking, as used by DemoSuper.sql.
    private const decimal SalaryGrossUp = 1.309524m;
    private const decimal SuperNominalReturn = 0.07m;

    public async Task Extend(DateOnly month, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var checkingId = settings.CheckingAccountId!.Value;
        var monthEnd = month.AddMonths(1).AddDays(-1);

        // Checking first: everything else reads the rows it writes.
        await Step("checking account", () => ExtendChecking(checkingId, month, monthEnd, cancellationToken));

        await Step("savings account", () => ExtendSavings(settings, month, monthEnd, cancellationToken));
        await Step("mortgage", () => ExtendLoan(settings.MortgageAccountId, checkingId, "Mortgage", MortgageRate, month, monthEnd, cancellationToken));
        await Step("car loan", () => ExtendLoan(settings.LoanAccountId, checkingId, "Car Loan", CarLoanRate, month, monthEnd, cancellationToken, CarLoanRepayment));
        await Step("superannuation", () => ExtendSuper(settings, checkingId, month, monthEnd, cancellationToken));
        await Step("utility bills", () => ExtendBills(settings, checkingId, monthEnd, cancellationToken));
    }

    /// <summary>
    /// Runs one piece, letting the rest of the run continue if it fails.
    /// </summary>
    /// <remarks>
    /// The pieces are independent, and a month missing from the mortgage is not a reason to leave
    /// the bills unwritten as well. Failures are logged at error because nothing retries them: the
    /// job fills the previous month only, so a hole stays a hole.
    /// </remarks>
    private async Task Step(string what, Func<Task> work)
    {
        try
        {
            await work();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to extend the demo {What}. This month will stay missing; nothing retries it.", what);
        }
    }


    private async Task ExtendChecking(Guid accountId, DateOnly month, DateOnly monthEnd, CancellationToken cancellationToken)
    {
        if (await AlreadyFilled(accountId, month, monthEnd, "checking account", cancellationToken)) return;

        var balance = await BalanceOf(accountId, cancellationToken);

        var generated = new TransactionAccountGenerator(balance, month.ToDateTime(TimeOnly.MinValue), monthEnd.ToDateTime(TimeOnly.MinValue))
            .Generate();

        foreach (var generatedTransaction in generated)
        {
            transactionRepository.Add(ToDomain(accountId, generatedTransaction));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Added {Count} transactions to the demo checking account.", generated.Count);

        // The account's rules are what gave its history its tags, so the new rows are tagged the
        // same way rather than by anything this job decides.
        await runRulesService.RunRules(accountId, cancellationToken);
    }

    private async Task ExtendSavings(DemoDataOptions settings, DateOnly month, DateOnly monthEnd, CancellationToken cancellationToken)
    {
        if (settings.SavingsAccountId is not Guid accountId) return;
        if (await AlreadyFilled(accountId, month, monthEnd, "savings account", cancellationToken)) return;

        var balance = await BalanceOf(accountId, cancellationToken);

        // The transfers out of checking this month are the transfers into savings. Matched on the
        // payee the generator writes into the description, which is the only Osko payee it uses.
        var transfersIn = await transactions
            .Where(t => t.AccountId == settings.CheckingAccountId!.Value && t.TransactionType == TransactionType.Debit)
            .Where(t => t.TransactionTime >= month.ToDateTime(TimeOnly.MinValue) && t.TransactionTime < monthEnd.AddDays(1).ToDateTime(TimeOnly.MinValue))
            .Where(t => t.Description != null && t.Description.Contains("Osko Payment to SAVINGS ACCOUNT"))
            .Select(t => new { t.TransactionTime, t.Amount })
            .ToListAsync(cancellationToken);

        var generated = new SavingsAccountGenerator(
                balance,
                month.ToDateTime(TimeOnly.MinValue),
                monthEnd.ToDateTime(TimeOnly.MinValue),
                [.. transfersIn.Select(t => (t.TransactionTime.Date, Math.Abs(t.Amount)))])
            .Generate();

        foreach (var generatedTransaction in generated)
        {
            transactionRepository.Add(ToDomain(accountId, generatedTransaction));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Added {Count} transactions to the demo savings account.", generated.Count);
    }

    /// <summary>
    /// Extends an amortising loan from the repayments the checking account shows for the month.
    /// </summary>
    /// <remarks>
    /// The ledger runs positive-owing: the repayment is a debit, split into a tagged interest
    /// portion and principal, and the interest is credited back so the balance amortises rather
    /// than falling by the whole repayment. See DemoMortgage.sql for why that direction is forced.
    /// </remarks>
    private async Task ExtendLoan(Guid? loanAccountId, Guid checkingId, string tagName, decimal annualRate, DateOnly month, DateOnly monthEnd, CancellationToken cancellationToken, decimal? instalment = null)
    {
        if (loanAccountId is not Guid accountId) return;
        if (await AlreadyFilled(accountId, month, monthEnd, tagName, cancellationToken)) return;

        var interestTagId = await logicalAccounts
            .Where(a => a.Id == accountId)
            .SelectMany(a => a.TagPurposes)
            .Where(p => p.Purpose == TagPurpose.MortgageInterest)
            .Select(p => (int?)p.TagId)
            .FirstOrDefaultAsync(cancellationToken);

        if (interestTagId is null)
        {
            logger.LogWarning("The demo {TagName} account has no MortgageInterest tag purpose, so its interest cannot be tagged. Skipping.", tagName);
            return;
        }

        var interestTag = await tags.FirstOrDefaultAsync(t => t.Id == interestTagId.Value, cancellationToken);

        if (interestTag is null)
        {
            logger.LogWarning("The interest tag for the demo {TagName} account could not be loaded. Skipping.", tagName);
            return;
        }

        var repayments = await TaggedAmounts(checkingId, tagName, month, monthEnd, cancellationToken);
        var owing = await BalanceOf(accountId, cancellationToken);

        if (repayments.Count == 0 && instalment is not null)
        {
            repayments = await OriginateRepayment(checkingId, tagName, instalment.Value, annualRate, owing, month, cancellationToken);
        }

        if (repayments.Count == 0)
        {
            logger.LogInformation("No {TagName} repayments on the demo checking account for {Month:yyyy-MM}.", tagName, month);
            return;
        }

        foreach (var (when, amount) in repayments)
        {
            var (interest, principal) = LoanSchedule.Split(owing, amount, annualRate);

            transactionRepository.Add(DomainTransaction.Create(accountId, null, interest, "Interest Charged", when, null, Source, null));

            var repayment = DomainTransaction.Create(accountId, null, -amount, "Loan Repayment", when, TransactionSubType.Recurring, Source, null);
            SplitIntoInterestAndPrincipal(repayment, interest, principal, interestTag);
            transactionRepository.Add(repayment);

            owing += interest - amount;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Added {Count} repayments to the demo {TagName} account. Balance owing {Owing:N2}.", repayments.Count, tagName, owing);
    }

    private async Task ExtendSuper(DemoDataOptions settings, Guid checkingId, DateOnly month, DateOnly monthEnd, CancellationToken cancellationToken)
    {
        if (settings.SuperAccountId is not Guid accountId) return;
        if (await AlreadyFilled(accountId, month, monthEnd, "super account", cancellationToken)) return;

        var employerTagId = await logicalAccounts
            .Where(a => a.Id == accountId)
            .SelectMany(a => a.TagPurposes)
            .Where(p => p.Purpose == TagPurpose.EmployerContribution)
            .Select(p => (int?)p.TagId)
            .FirstOrDefaultAsync(cancellationToken);

        var employerTag = employerTagId is null
            ? null
            : await tags.FirstOrDefaultAsync(t => t.Id == employerTagId.Value, cancellationToken);

        if (employerTag is null)
        {
            logger.LogWarning("The demo super account has no EmployerContribution tag purpose, so contributions cannot be tagged. Skipping.");
            return;
        }

        var salaries = await TaggedAmounts(checkingId, "Salary", month, monthEnd, cancellationToken);

        var balance = await BalanceOf(accountId, cancellationToken);
        var contributed = 0m;

        foreach (var (when, amount) in salaries)
        {
            // The guarantee is levied on gross pay; the account shows net.
            var contribution = Math.Round(amount * SalaryGrossUp * SuperannuationGuarantee.RateFor(DateOnly.FromDateTime(when)), 2, MidpointRounding.AwayFromZero);

            var transaction = DomainTransaction.Create(accountId, null, contribution, "Employer Contribution", when, null, Source, null);
            transaction.AddOrUpdateSplit(employerTag);
            transactionRepository.Add(transaction);

            contributed += contribution;
        }

        // Earnings land at the end of a quarter, so most months add nothing.
        if (monthEnd.Month % 3 == 0)
        {
            var earnings = Math.Round((balance + contributed / 2m) * SuperNominalReturn / 4m, 2, MidpointRounding.AwayFromZero);

            if (earnings > 0)
            {
                transactionRepository.Add(DomainTransaction.Create(accountId, null, earnings, "Investment Earnings", monthEnd.ToDateTime(TimeOnly.MinValue), null, Source, null));
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Added {Count} contributions totalling {Total:N2} to the demo super account.", salaries.Count, contributed);
    }

    private async Task ExtendBills(DemoDataOptions settings, Guid checkingId, DateOnly monthEnd, CancellationToken cancellationToken)
    {
        await ExtendBills(settings.ElectricityAccountId, checkingId, "Electricity", monthEnd, cancellationToken);
        await ExtendBills(settings.WaterAccountId, checkingId, "Water", monthEnd, cancellationToken);
    }

    /// <summary>
    /// Adds a bill once a billing cycle's worth of payments has accumulated.
    /// </summary>
    /// <remarks>
    /// The tariff is taken from the account's most recent bill rather than held here. A demo
    /// household changes electricity retailer every few years and prices rise in between, so a rate
    /// written into this class would be wrong within the year and wrong in a way nobody would
    /// notice. Reading it back means the job inherits whatever the data already says -- including a
    /// rate corrected by hand through the bill editor.
    /// </remarks>
    private async Task ExtendBills(Guid? utilityAccountId, Guid checkingId, string tagName, DateOnly monthEnd, CancellationToken cancellationToken)
    {
        if (utilityAccountId is not Guid accountId) return;

        var summary = await utilityAccounts.Get(accountId, cancellationToken);

        if (summary is null)
        {
            logger.LogWarning("The configured demo {TagName} account {InstrumentId} does not exist.", tagName, accountId);
            return;
        }

        var lastBillId = summary.Bills.OrderByDescending(b => b.IssueDate).ThenByDescending(b => b.Id).FirstOrDefault()?.Id;

        if (lastBillId is null)
        {
            logger.LogWarning("The demo {TagName} account has no bills to take a tariff from. Run DemoUtilitiesRebuild.sql first.", tagName);
            return;
        }

        // Reloaded through the filtered include so the last bill arrives with the periods, charges
        // and usages this one is modelled on.
        var account = await utilityAccounts.GetWithBill(accountId, lastBillId.Value, cancellationToken);
        var last = account?.Bills.SingleOrDefault();
        var lastPeriod = last?.Periods.OrderByDescending(p => p.PeriodEnd).FirstOrDefault();
        var lastUsage = lastPeriod?.Usages.FirstOrDefault(u => u.UsageType == UsageType.Consumption);

        if (account is null || last is null || lastPeriod is null || lastUsage is null)
        {
            logger.LogWarning("The last demo {TagName} bill has no priced consumption to copy. Skipping.", tagName);
            return;
        }

        var periodStart = last.IssueDate.AddDays(1);

        var payments = await TaggedAmounts(checkingId, tagName, periodStart, monthEnd, cancellationToken);

        if (payments.Count == 0) return;

        var issued = DateOnly.FromDateTime(payments.Max(p => p.When));
        var days = issued.DayNumber - periodStart.DayNumber + 1;

        // Bills are quarterly, so most months add nothing and the payments simply accumulate until
        // a cycle's worth has passed. Without this the job would issue a bill a month, each one
        // covering a few weeks, and the series would step from quarterly to monthly overnight.
        if (days < MinimumBillingDays)
        {
            logger.LogInformation("Only {Days} days since the last demo {TagName} bill. Waiting for a full cycle.", days, tagName);
            return;
        }

        var cost = payments.Sum(p => p.Amount);
        var serviceTotal = lastPeriod.ServiceCharges.Sum(sc => sc.ChargePerDay) * days;
        var usage = Math.Round((cost - serviceTotal) / lastUsage.PricePerUnit, 3, MidpointRounding.AwayFromZero);

        if (usage <= 0)
        {
            logger.LogWarning("A demo {TagName} bill for {Issued} would need non-positive consumption. Skipping it.", tagName, issued);
            return;
        }

        var period = new Period
        {
            PeriodStart = periodStart.ToDateTime(TimeOnly.MinValue),
            PeriodEnd = issued.ToDateTime(TimeOnly.MinValue),
            // Consumption only: the demo's electricity account has no solar, so it never exports. A
            // period can carry several usages, and the type has to be stated -- the column is
            // nullable until the follow-up tightens it, and a null read back through the
            // non-nullable UsageType would throw.
            Usages = [new Usage { PricePerUnit = lastUsage.PricePerUnit, TotalUsage = usage, UsageType = UsageType.Consumption }],
        };

        foreach (var charge in lastPeriod.ServiceCharges)
        {
            period.ServiceCharges.Add(new ServiceCharge { ChargeTypeId = charge.ChargeTypeId, ChargePerDay = charge.ChargePerDay });
        }

        account.Bills.Add(new Bill
        {
            AccountId = accountId,
            InvoiceNumber = $"{tagName[0]}{issued:yyyyMMdd}",
            IssueDate = issued,
            PreviousReading = last.CurrentReading,
            CurrentReading = (last.CurrentReading ?? 0) + (int)Math.Round(usage, 0, MidpointRounding.AwayFromZero),
            CostsIncludeGST = true,
            Periods = [period],
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Added {Count} demo {TagName} bills.", payments.Count, tagName);
    }

    /// <summary>
    /// Writes a loan repayment to the checking account, for a loan nothing else pays.
    /// </summary>
    /// <remarks>
    /// The mortgage is derived: the generator writes a home loan payment every month and the
    /// account's rules tag it. The car loan has no such template and no rule, because the backfill
    /// put both sides of it there directly -- so without this the repayments simply stop and the
    /// demo shows a loan nobody is paying.
    ///
    /// Written only when the month has no such payment already, so a run that got this far last
    /// time and then failed does not pay the loan twice.
    /// </remarks>
    private async Task<List<(DateTime When, decimal Amount)>> OriginateRepayment(
        Guid checkingId, string tagName, decimal instalment, decimal annualRate, decimal owing, DateOnly month, CancellationToken cancellationToken)
    {
        if (owing <= 0.01m)
        {
            logger.LogInformation("The demo {TagName} is repaid. Nothing further to pay.", tagName);
            return [];
        }

        var tag = await tags.FirstOrDefaultAsync(t => t.Name == tagName, cancellationToken);

        if (tag is null)
        {
            logger.LogWarning("No '{TagName}' tag exists, so a repayment cannot be tagged. Skipping.", tagName);
            return [];
        }

        // The final instalment clears what is left rather than overpaying.
        var payoff = owing + Math.Round(owing * annualRate / 12m, 2, MidpointRounding.AwayFromZero);
        var amount = Math.Min(instalment, payoff);
        var due = month.ToDateTime(TimeOnly.MinValue);

        var payment = DomainTransaction.Create(checkingId, null, -amount, $"Direct Debit - {tagName.ToUpperInvariant()} REPAYMENT", due, TransactionSubType.DirectDebit, Source, null);
        payment.AddOrUpdateSplit(tag);
        transactionRepository.Add(payment);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return [(due, amount)];
    }

    /// <summary>
    /// Splits a loan repayment in two, tagging the interest portion.
    /// </summary>
    /// <remarks>
    /// The split the transaction was created with is reused rather than replaced. UpdateSplits
    /// matches on id, and a split it is asked to remove while it is the only one is emptied rather
    /// than dropped -- so handing it two brand new splits would leave the original behind holding
    /// the full amount, and the sum of the splits would exceed the transaction.
    ///
    /// Amounts are positive magnitudes on a negative transaction, which is how the domain writes
    /// them: the constraint that keeps splits within their transaction compares absolute values.
    /// </remarks>
    private static void SplitIntoInterestAndPrincipal(DomainTransaction repayment, decimal interest, decimal principal, TagEntity interestTag)
    {
        var existing = repayment.Splits.First();

        repayment.UpdateSplits([
            new TransactionSplit(existing.Id) { Amount = interest, Tags = [interestTag] },
            new TransactionSplit(Guid.CreateVersion7()) { Amount = principal },
        ]);
    }

    /// <summary>
    /// The dated amounts of the transactions carrying a named tag, as positive magnitudes.
    /// </summary>
    private async Task<List<(DateTime When, decimal Amount)>> TaggedAmounts(Guid accountId, string tagName, DateOnly month, DateOnly monthEnd, CancellationToken cancellationToken)
    {
        var from = month.ToDateTime(TimeOnly.MinValue);
        var to = monthEnd.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var matches = await transactions
            .Where(t => t.AccountId == accountId && t.TransactionTime >= from && t.TransactionTime < to)
            .Where(t => t.Splits.Any(s => s.Tags.Any(tag => tag.Name == tagName)))
            .Select(t => new { t.TransactionTime, t.Amount })
            .ToListAsync(cancellationToken);

        return [.. matches.OrderBy(m => m.TransactionTime).Select(m => (m.TransactionTime, Math.Abs(m.Amount)))];
    }

    private async Task<bool> AlreadyFilled(Guid accountId, DateOnly month, DateOnly monthEnd, string what, CancellationToken cancellationToken)
    {
        var from = month.ToDateTime(TimeOnly.MinValue);
        var to = monthEnd.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var filled = await transactions.AnyAsync(t => t.AccountId == accountId && t.TransactionTime >= from && t.TransactionTime < to, cancellationToken);

        if (filled)
        {
            logger.LogInformation("The demo {What} already has transactions in {Month:yyyy-MM}. Skipping it.", what, month);
        }

        return filled;
    }

    /// <summary>
    /// The account's balance, computed the same way the balance view computes it.
    /// </summary>
    private async Task<decimal> BalanceOf(Guid accountId, CancellationToken cancellationToken) =>
        await transactions
            .Where(t => t.AccountId == accountId)
            .SumAsync(t => t.TransactionType == TransactionType.Credit ? t.Amount : -Math.Abs(t.Amount), cancellationToken);

    private static DomainTransaction ToDomain(Guid accountId, GeneratedTransaction generated) =>
        DomainTransaction.Create(
            accountId,
            null,
            generated.Credit ?? -(generated.Debit ?? 0m),
            generated.Description,
            generated.Date,
            SubTypeOf(generated.PaymentMethod),
            Source,
            null);

    private static TransactionSubType? SubTypeOf(PaymentMethod? paymentMethod) => paymentMethod switch
    {
        PaymentMethod.Visa => TransactionSubType.Visa,
        PaymentMethod.Eftpos => TransactionSubType.Eftpos,
        PaymentMethod.DirectDebit => TransactionSubType.DirectDebit,
        PaymentMethod.Bpay => TransactionSubType.Bpay,
        PaymentMethod.Osko => TransactionSubType.Osko,
        PaymentMethod.InternalTransfer => TransactionSubType.Transfer,
        PaymentMethod.Atm => TransactionSubType.Atm,
        _ => null,
    };
}
