using System.Globalization;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Importers;
using Asm.MooBank.Institution.AustralianSuper.Domain;
using Asm.MooBank.Institution.AustralianSuper.Models;
using Microsoft.Extensions.Logging;
using TransactionType = Asm.MooBank.Models.TransactionType;

namespace Asm.MooBank.Institution.AustralianSuper.Importers;

internal partial class Importer(IQueryable<TransactionRaw> rawTransactions, ITransactionRawRepository transactionRawRepository, ITransactionRepository transactionRepository, ILogger<Importer> logger) : IImporter
{
    private const int Columns = 24;
    private const int DateColumn = 0;
    private const int CategoryColumn = 1;
    private const int TitleColumn = 2;
    private const int DescriptionColumn = 3;
    private const int PaymentPeriodColumn = 4;
    private const int SGContributionsColumn = 5;
    private const int EmployerAdditionalColumn = 6;
    private const int SalarySacrificeColumn = 7;
    private const int MemberAdditionalColumn = 8;
    private const int TotalAmountColumn = 23;

    public async Task<MooBank.Models.TransactionImportResult> Import(Guid accountId, Stream contents, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(contents);
        var rawTransactions = new List<TransactionRaw>();

        // Throw away header row
        await reader.ReadLineAsync(cancellationToken);

        int lineCount = 1;

        while (!reader.EndOfStream)
        {
            DateOnly transactionTime = DateOnly.MinValue;

            string line = (await reader.ReadLineAsync(cancellationToken))!;
            lineCount++;

            string[] prelimColumns = line.Split(",");

            List<string> columns = [];

            string? current = null;

            decimal sgContributions = 0;
            decimal employerAdditional = 0;
            decimal salarySacrifice = 0;
            decimal memberAdditional = 0;

            foreach (string str in prelimColumns)
            {
                if (str.StartsWith('\"') && !str.EndsWith('\"'))
                {
                    current = str;
                }
                else if (!str.StartsWith('\"') && str.EndsWith('\"'))
                {
                    columns.Add((current + str).Trim('"').Replace("\"\"", "\""));
                }
                else
                {
                    columns.Add(str);
                }
            }

            #region Validation
            if (columns.Count != Columns)
            {
                logger.LogWarning("Unrecognised entry at line {lineCount}", lineCount);
                continue;
            }

            if (!DateOnly.TryParseExact(columns[DateColumn], "yyyy-MM-dd", out transactionTime))
            {
                logger.LogWarning("Incorrect date format at line {lineCount}", lineCount);
                continue;
            }
            if (String.IsNullOrWhiteSpace(columns[TitleColumn]))
            {
                logger.LogWarning("Description not supplied at line {lineCount}", lineCount);
                continue;
            }

            if (String.IsNullOrEmpty(columns[TotalAmountColumn]))
            {
                logger.LogWarning("Total amount not supplied at line {lineCount}", lineCount);
                continue;
            }

            bool isContribution = columns[CategoryColumn]?.Trim() == "CONTRIBUTIONS";

            if (isContribution &&
                String.IsNullOrWhiteSpace(columns[PaymentPeriodColumn]) &&
                !Decimal.TryParse(columns[SGContributionsColumn], out sgContributions) &&
                !Decimal.TryParse(columns[EmployerAdditionalColumn], out employerAdditional) &&
                !Decimal.TryParse(columns[SalarySacrificeColumn], out salarySacrifice) &&
                !Decimal.TryParse(columns[MemberAdditionalColumn], out memberAdditional))

            {
                logger.LogWarning("Incorrect contribution format at line {lineCount}", lineCount);
                continue;
            }

            if (!Decimal.TryParse(columns[TotalAmountColumn], out decimal totalAmount))
            {
                logger.LogWarning("Incorrect total amount format at line {lineCount}", lineCount);
                continue;
            }
            #endregion

            if (rawTransactions.Any(t => t.Description == columns[DescriptionColumn] && t.Date == transactionTime && t.TotalAmount == totalAmount))
            {
                logger.LogInformation("Duplicate transaction found {description} {date} {totalAmount}", columns[DescriptionColumn], transactionTime, totalAmount);
                continue;
            }

            var transaction = new Transaction
            {
                AccountId = accountId,
                Amount = totalAmount,
                Description = $"{columns[TitleColumn].Trim()} {columns[DescriptionColumn]?.Trim()}".Trim(),
                Extra = isContribution ? new TransactionExtra
                {
                    SGContributions = sgContributions,
                    EmployerAdditional = employerAdditional,
                    SalarySacrifice = salarySacrifice,
                    MemberAdditional = memberAdditional,
                } : null,
                TransactionTime = transactionTime.ToStartOfDay(),
                TransactionType = totalAmount < 0 ? TransactionType.Debit : TransactionType.Credit,
                Source = "AustralianSuper Import",
            };

            var transactionRaw = new TransactionRaw
            {
                AccountId = accountId,
                Category = columns[CategoryColumn]?.Trim(),
                Date = transactionTime,
                Description = columns[DescriptionColumn],
                EmployerAdditional = isContribution ? employerAdditional : null,
                MemberAdditional = isContribution ? memberAdditional : null,
                PaymentPeriodEnd = isContribution ? DateOnly.ParseExact(columns[PaymentPeriodColumn].Split('/')[0], "yyyy-MM-dd", CultureInfo.InvariantCulture) : null,
                PaymentPeriodStart = isContribution ? DateOnly.ParseExact(columns[PaymentPeriodColumn].Split('/')[1], "yyyy-MM-dd", CultureInfo.InvariantCulture) : null,
                SalarySacrifice = isContribution ? salarySacrifice : null,
                SGContributions = isContribution ? sgContributions : null,
                Title = columns[TitleColumn].Trim(),
                TotalAmount = totalAmount,
                Imported = DateTime.Now,
                Transaction = transaction,
            };

            rawTransactions.Add(transactionRaw);
        }

        transactionRawRepository.AddRange(rawTransactions);

        return new MooBank.Models.TransactionImportResult(rawTransactions.Select(r => r.Transaction));
    }

    public async Task Reprocess(Guid accountId, CancellationToken cancellationToken = default)
    {
        var transactions = await transactionRepository.GetTransactions(accountId, cancellationToken);
        var transactionIds = transactions.Select(t => t.Id);

        var rawTransactions = await transactionRawRepository.GetAll(accountId, cancellationToken);
        var processed = rawTransactions.Where(t => t.TransactionId != null && transactionIds.Contains(t.TransactionId.Value));
        var unprocessed = rawTransactions.Except(processed, new Asm.Domain.IIdentifiableEqualityComparer<TransactionRaw, Guid>());

        foreach (var raw in processed)
        {
            bool isContribution = raw.Category == "CONTRIBUTIONS";

            raw.Transaction.Description = raw.Description;
            raw.Transaction.Extra = isContribution ? new TransactionExtra
            {
                SGContributions = raw.SGContributions,
                EmployerAdditional = raw.EmployerAdditional,
                SalarySacrifice = raw.SalarySacrifice,
                MemberAdditional = raw.MemberAdditional,
            } : null;
            raw.Transaction.PurchaseDate = raw.Date.ToDateTime(TimeOnly.MinValue);
            raw.Transaction.TransactionTime = raw.Date.ToStartOfDay();


        }

    }
}
