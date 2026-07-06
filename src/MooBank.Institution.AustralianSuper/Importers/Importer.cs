using System.Globalization;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Importers;
using Asm.MooBank.Institution.AustralianSuper.Domain;
using Asm.MooBank.Institution.AustralianSuper.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Institution.AustralianSuper.Importers;

internal partial class Importer(ITransactionRawRepository transactionRawRepository, ITransactionRepository transactionRepository, ILogger<Importer> logger) : IImporter
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
    private const string DateFormat = "yyyy-MM-dd";
    private const string ContributionsCategory = "CONTRIBUTIONS";

    public async Task<MooBank.Models.TransactionImportResult> Import(Guid instrumentId, Guid? institutionAccountId, Stream contents, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(contents);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            BadDataFound = null,
        };
        using var parser = new CsvParser(reader, config);

        // Throw away header row
        await parser.ReadAsync();

        List<string[]> rows = [];

        while (await parser.ReadAsync())
        {
            rows.Add(parser.Record ?? []);
        }

        var checkTransactions = await GetCheckTransactions(instrumentId, rows, cancellationToken);

        var rawTransactionEntities = new List<TransactionRaw>();

        int lineCount = 1;

        foreach (string[] columns in rows)
        {
            lineCount++;

            #region Validation
            if (columns.Length != Columns)
            {
                logger.LogWarning("Unrecognised entry at line {lineCount}", lineCount);
                continue;
            }

            if (!DateOnly.TryParseExact(columns[DateColumn], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly transactionTime))
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

            bool isContribution = columns[CategoryColumn]?.Trim() == ContributionsCategory;

            // Parse the contribution amounts unconditionally so that valid values are always captured.
            bool amountsValid = TryParseAmount(columns[SGContributionsColumn], out decimal sgContributions) &
                                TryParseAmount(columns[EmployerAdditionalColumn], out decimal employerAdditional) &
                                TryParseAmount(columns[SalarySacrificeColumn], out decimal salarySacrifice) &
                                TryParseAmount(columns[MemberAdditionalColumn], out decimal memberAdditional);

            DateOnly? paymentPeriodStart = null;
            DateOnly? paymentPeriodEnd = null;

            if (isContribution)
            {
                if (String.IsNullOrWhiteSpace(columns[PaymentPeriodColumn]) || !amountsValid)
                {
                    logger.LogWarning("Incorrect contribution format at line {lineCount}", lineCount);
                    continue;
                }

                if (!TryParsePaymentPeriod(columns[PaymentPeriodColumn], out paymentPeriodStart, out paymentPeriodEnd))
                {
                    logger.LogWarning("Incorrect payment period format at line {lineCount}", lineCount);
                    continue;
                }
            }

            if (!Decimal.TryParse(columns[TotalAmountColumn], NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal totalAmount))
            {
                logger.LogWarning("Incorrect total amount format at line {lineCount}", lineCount);
                continue;
            }
            #endregion

            if (checkTransactions.Any(t => t.Description == columns[DescriptionColumn] && t.Date == transactionTime && t.TotalAmount == totalAmount))
            {
                logger.LogInformation("Duplicate transaction found {description} {date} {totalAmount}", columns[DescriptionColumn], transactionTime, totalAmount);
                continue;
            }

            Transaction transaction = Transaction.Create(
                instrumentId,
                null, // No account holder ID in this context
                totalAmount,
                $"{columns[TitleColumn].Trim()} {columns[DescriptionColumn]?.Trim()}".Trim(),
                transactionTime.ToStartOfDay(),
                null, // No sub-type in this context
                "AustralianSuper Import",
                institutionAccountId
            );

            transaction.Extra = isContribution ? new TransactionExtra
            {
                SGContributions = sgContributions,
                EmployerAdditional = employerAdditional,
                SalarySacrifice = salarySacrifice,
                MemberAdditional = memberAdditional,
            } : null;

            var transactionRaw = new TransactionRaw
            {
                AccountId = instrumentId,
                Category = columns[CategoryColumn]?.Trim(),
                Date = transactionTime,
                Description = columns[DescriptionColumn],
                EmployerAdditional = isContribution ? employerAdditional : null,
                MemberAdditional = isContribution ? memberAdditional : null,
                PaymentPeriodEnd = paymentPeriodEnd,
                PaymentPeriodStart = paymentPeriodStart,
                SalarySacrifice = isContribution ? salarySacrifice : null,
                SGContributions = isContribution ? sgContributions : null,
                Title = columns[TitleColumn].Trim(),
                TotalAmount = totalAmount,
                Imported = DateTime.Now,
                Transaction = transaction,
            };

            rawTransactionEntities.Add(transactionRaw);
        }

        transactionRawRepository.AddRange(rawTransactionEntities);

        return new MooBank.Models.TransactionImportResult(rawTransactionEntities.Select(r => r.Transaction));
    }

    public async Task Reprocess(Guid instrumentId, Guid institutionAccountId, CancellationToken cancellationToken = default)
    {
        var transactionIds = (await transactionRepository.GetTransactionIds(instrumentId, cancellationToken: cancellationToken)).ToHashSet();

        var rawTransactions = await transactionRawRepository.GetAll(instrumentId, cancellationToken);
        var processed = rawTransactions.Where(t => t.TransactionId != null && transactionIds.Contains(t.TransactionId.Value));

        foreach (var raw in processed)
        {
            bool isContribution = raw.Category == ContributionsCategory;

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

    /// <summary>
    /// Loads the existing raw transactions that fall within the date range of the file being imported,
    /// projected down to only the fields needed for duplicate detection.
    /// </summary>
    private async Task<IReadOnlyCollection<TransactionRawSummary>> GetCheckTransactions(Guid instrumentId, IEnumerable<string[]> rows, CancellationToken cancellationToken)
    {
        List<DateOnly> dates = [];

        foreach (string[] row in rows)
        {
            if (row.Length > DateColumn && DateOnly.TryParseExact(row[DateColumn], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
            {
                dates.Add(date);
            }
        }

        if (dates.Count == 0) return [];

        return (await transactionRawRepository.GetSummaries(instrumentId, dates.Min(), dates.Max(), cancellationToken)).ToList();
    }

    /// <summary>
    /// Parses an amount column. Empty values are treated as zero.
    /// </summary>
    private static bool TryParseAmount(string? value, out decimal result)
    {
        if (String.IsNullOrWhiteSpace(value))
        {
            result = 0;
            return true;
        }

        return Decimal.TryParse(value, NumberStyles.Currency, CultureInfo.InvariantCulture, out result);
    }

    /// <summary>
    /// Parses a payment period in the format "start/end" (e.g. "2024-06-01/2024-06-14").
    /// If the dates appear in reverse order they are swapped so that start is always the earlier date.
    /// </summary>
    private static bool TryParsePaymentPeriod(string value, out DateOnly? start, out DateOnly? end)
    {
        start = null;
        end = null;

        string[] parts = value.Split('/');

        if (parts.Length != 2 ||
            !DateOnly.TryParseExact(parts[0].Trim(), DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsedStart) ||
            !DateOnly.TryParseExact(parts[1].Trim(), DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsedEnd))
        {
            return false;
        }

        if (parsedStart > parsedEnd)
        {
            (parsedStart, parsedEnd) = (parsedEnd, parsedStart);
        }

        start = parsedStart;
        end = parsedEnd;
        return true;
    }
}
