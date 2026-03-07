namespace Asm.MooBank.Tools.TransactionGenerator;

/// <summary>
/// Generates realistic transaction data for a transaction (everyday) bank account.
/// </summary>
public class TransactionAccountGenerator
{
    private readonly Random _random = new();
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;
    private readonly string _cardLast4;
    private readonly string _employer;
    private decimal _balance;

    private readonly List<Transaction> _transactions = [];
    private readonly List<PendingTransaction> _pendingTransactions = [];
    private readonly List<Transaction> _refundCandidates = [];

    // Track last occurrence for each template to maintain proper frequency
    private readonly Dictionary<TransactionTemplate, DateTime> _lastOccurrence = [];

    // Location tracking - Brisbane is home base
    private string _currentLocation = "BRISBANE";
    private DateTime? _tripEndDate;
    private DateTime? _lastTripDate;
    private DateTime? _currentLocationDate;  // Track which day's location is set

    public TransactionAccountGenerator(decimal startingBalance, DateTime startDate, DateTime endDate)
    {
        _balance = startingBalance;
        _startDate = startDate;
        _endDate = endDate;
        _cardLast4 = "4827"; // Static card number for consistency
        _employer = Merchants.GetRandomEmployer();
    }

    public List<Transaction> Generate()
    {
        DescriptionBuilder.ResetReceiptCounter();

        var currentDate = _startDate;

        while (currentDate <= _endDate)
        {
            // Update location (handle trips)
            UpdateLocation(currentDate);

            // Process any pending transactions for this date
            ProcessPendingTransactions(currentDate);

            // Generate transactions for this date
            GenerateTransactionsForDate(currentDate);

            currentDate = currentDate.AddDays(1);
        }

        // Sort by date, then by credit first (income before expenses on same day)
        return [.. _transactions.OrderBy(t => t.Date).ThenByDescending(t => t.Credit ?? 0)];
    }

    private void UpdateLocation(DateTime date)
    {
        // Check if current trip has ended
        if (_tripEndDate.HasValue && date > _tripEndDate.Value)
        {
            _currentLocation = Merchants.GetBrisbaneLocation();
            _tripEndDate = null;
        }

        // Annual May holiday (around May 10-20) - always a flight destination
        if (date.Month == 5 && date.Day == 10 && !_tripEndDate.HasValue)
        {
            var (city, distance) = Merchants.GetFlightDestination();
            _currentLocation = city;
            _tripEndDate = date.AddDays(_random.Next(7, 12)); // 7-12 day holiday
            _lastTripDate = date;

            // Generate holiday flight (more expensive)
            GenerateFlight(date, distance, isHoliday: true);
            return;
        }

        // If not on a trip, small chance to start a weekend trip
        if (!_tripEndDate.HasValue)
        {
            // Weekend trips: ~once every 2-3 months
            var daysSinceLastTrip = _lastTripDate.HasValue ? (date - _lastTripDate.Value).Days : 90;

            if (daysSinceLastTrip > 45 && date.DayOfWeek == DayOfWeek.Friday && _random.NextDouble() < 0.15)
            {
                _tripEndDate = date.AddDays(_random.Next(2, 5));
                _lastTripDate = date;

                // 40% chance of a drive trip (Gold Coast, Sunshine Coast, etc.)
                // 60% chance of a flight trip
                if (_random.NextDouble() < 0.4)
                {
                    _currentLocation = Merchants.GetDriveDestination();
                    // No flight needed - just extra fuel
                    GenerateDriveTripFuel(date);
                }
                else
                {
                    var (city, distance) = Merchants.GetFlightDestination();
                    _currentLocation = city;
                    GenerateFlight(date, distance, isHoliday: false);
                }
            }
        }
    }

    private void GenerateFlight(DateTime date, int distanceKm, bool isHoliday)
    {
        var airline = Merchants.GetRandom(Merchants.Airlines);
        var flightCost = Merchants.GetFlightCost(distanceKm, isHoliday);

        var description = DescriptionBuilder.VisaPurchase(airline, _currentLocation, date, _cardLast4);

        _balance -= flightCost;
        _transactions.Add(new Transaction
        {
            Date = date,
            Description = description,
            Debit = Math.Round(flightCost, 2),
            Balance = _balance,
            Category = "Travel",
            Merchant = airline,
            PaymentMethod = PaymentMethod.Visa
        });
    }

    private void GenerateDriveTripFuel(DateTime date)
    {
        // Extra fuel for the road trip ($80-150)
        var fuelStation = Merchants.GetRandom(Merchants.FuelStations);
        var fuelCost = 80m + (decimal)(_random.NextDouble() * 70);

        var description = DescriptionBuilder.VisaPurchase(fuelStation, _currentLocation, date, _cardLast4);

        _balance -= fuelCost;
        _transactions.Add(new Transaction
        {
            Date = date,
            Description = description,
            Debit = Math.Round(fuelCost, 2),
            Balance = _balance,
            Category = "Transport",
            Merchant = fuelStation,
            PaymentMethod = PaymentMethod.Visa
        });
    }

    private string GetCurrentLocation(DateTime date)
    {
        // If on a trip, use the trip location (same city for entire trip)
        if (_tripEndDate.HasValue)
        {
            return _currentLocation;
        }

        // For Brisbane days, use same suburb for the whole day
        if (_currentLocationDate != date.Date)
        {
            _currentLocation = Merchants.GetBrisbaneLocation();
            _currentLocationDate = date.Date;
        }

        return _currentLocation;
    }

    private void GenerateTransactionsForDate(DateTime date)
    {
        // Monthly bills (fixed day of month)
        foreach (var template in TransactionTemplates.MonthlyBills)
        {
            if (template.ScheduleType == ScheduleType.MonthlyOnDay && date.Day == template.DayOfMonth)
            {
                GenerateTransaction(template, date);
            }
        }

        // Yearly events
        foreach (var template in TransactionTemplates.YearlyEvents)
        {
            if (template.ScheduleType == ScheduleType.Yearly &&
                date.Month == template.Month && date.Day == template.Day)
            {
                GenerateTransaction(template, date);
            }
        }

        // Frequency-based transactions
        foreach (var template in TransactionTemplates.FrequencyBased)
        {
            if (ShouldGenerateFrequencyTransaction(template, date))
            {
                GenerateTransaction(template, date);
            }
        }

        // Quarterly bills
        foreach (var template in TransactionTemplates.QuarterlyBills)
        {
            if (ShouldGenerateFrequencyTransaction(template, date))
            {
                GenerateTransaction(template, date);
            }
        }

        // Emergency transactions (random chance)
        foreach (var template in TransactionTemplates.EmergencyTransactions)
        {
            if (ShouldGenerateFrequencyTransaction(template, date))
            {
                GenerateTransaction(template, date);
            }
        }

        // Christmas shopping cluster (December 1-24)
        if (date.Month == 12 && date.Day >= 1 && date.Day <= 24)
        {
            foreach (var template in TransactionTemplates.ChristmasShopping)
            {
                if (ShouldGenerateFrequencyTransaction(template, date))
                {
                    GenerateTransaction(template, date);
                }
            }
        }

        // Savings transfer on 28th (if balance is healthy)
        if (date.Day == 28 && _balance > 5000)
        {
            GenerateTransaction(TransactionTemplates.SavingsTransfer, date);
        }

        // Random refunds (about 1.5% of debit transactions)
        if (_refundCandidates.Count > 0 && _random.NextDouble() < 0.015)
        {
            GenerateRefund(date);
        }
    }

    private bool ShouldGenerateFrequencyTransaction(TransactionTemplate template, DateTime date)
    {
        if (template.ScheduleType != ScheduleType.Frequency || template.FrequencyDays <= 0)
            return false;

        // Check if we've passed the next occurrence date
        if (!_lastOccurrence.TryGetValue(template, out var lastDate))
        {
            // First occurrence - use a random offset from start date
            var initialOffset = _random.Next(0, template.FrequencyDays);
            if ((date - _startDate).Days < initialOffset)
                return false;

            lastDate = _startDate.AddDays(-template.FrequencyDays); // Force first occurrence
        }

        var daysSinceLast = (date - lastDate).Days;
        var targetDays = template.FrequencyDays + _random.Next(-template.FrequencyVariance, template.FrequencyVariance + 1);
        targetDays = Math.Max(1, targetDays);

        if (daysSinceLast < targetDays)
            return false;

        // Apply weekend bias
        if (template.WeekendBias != 1.0)
        {
            var isWeekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            var threshold = isWeekend ? template.WeekendBias : 1.0 / template.WeekendBias;

            // Normalize to a probability
            var probability = threshold / (1.0 + threshold);
            if (_random.NextDouble() > probability)
            {
                // Skip this day, try again tomorrow
                return false;
            }
        }

        return true;
    }

    private void GenerateTransaction(TransactionTemplate template, DateTime date)
    {
        _lastOccurrence[template] = date;

        var merchant = Merchants.GetRandom(template.Merchants);
        var location = GetCurrentLocation(date);
        var amount = CalculateAmount(template, date.Month);
        var description = BuildDescription(template, merchant, location, date, amount);

        Transaction transaction;

        if (template.IsCredit)
        {
            _balance += amount;
            transaction = new Transaction
            {
                Date = date,
                Description = description,
                Credit = amount,
                Balance = _balance,
                Category = template.Category,
                Merchant = merchant,
                PaymentMethod = template.PaymentMethod
            };
        }
        else
        {
            _balance -= amount;
            transaction = new Transaction
            {
                Date = date,
                Description = description,
                Debit = amount,
                Balance = _balance,
                Category = template.Category,
                Merchant = merchant,
                PaymentMethod = template.PaymentMethod
            };

            // Track for potential refunds (only card transactions)
            if (template.PaymentMethod is PaymentMethod.Visa or PaymentMethod.Eftpos && amount > 20)
            {
                _refundCandidates.Add(transaction);
                // Keep only recent candidates (last 50)
                if (_refundCandidates.Count > 50)
                {
                    _refundCandidates.RemoveAt(0);
                }
            }
        }

        _transactions.Add(transaction);

        // Schedule follow-up transaction if defined
        if (template.FollowUpTransaction != null)
        {
            var followUpDate = date.AddDays(template.FollowUpDelayDays + _random.Next(-1, 2));
            if (followUpDate <= _endDate)
            {
                _pendingTransactions.Add(new PendingTransaction
                {
                    Date = followUpDate,
                    Template = template.FollowUpTransaction,
                    SpecificMerchant = template.FollowUpTransaction.Merchants[0]
                });
            }
        }
    }

    private void ProcessPendingTransactions(DateTime date)
    {
        var toProcess = _pendingTransactions.Where(p => p.Date == date).ToList();

        foreach (var pending in toProcess)
        {
            _pendingTransactions.Remove(pending);

            var merchant = pending.SpecificMerchant ?? Merchants.GetRandom(pending.Template.Merchants);
            var location = GetCurrentLocation(date);
            var amount = CalculateAmount(pending.Template, date.Month);
            var description = BuildDescription(pending.Template, merchant, location, date, amount);

            if (pending.Template.IsCredit)
            {
                _balance += amount;
                _transactions.Add(new Transaction
                {
                    Date = date,
                    Description = description,
                    Credit = amount,
                    Balance = _balance,
                    Category = pending.Template.Category,
                    Merchant = merchant,
                    PaymentMethod = pending.Template.PaymentMethod
                });
            }
            else
            {
                _balance -= amount;
                _transactions.Add(new Transaction
                {
                    Date = date,
                    Description = description,
                    Debit = amount,
                    Balance = _balance,
                    Category = pending.Template.Category,
                    Merchant = merchant,
                    PaymentMethod = pending.Template.PaymentMethod
                });
            }
        }
    }

    private void GenerateRefund(DateTime date)
    {
        if (_refundCandidates.Count == 0) return;

        var index = _random.Next(_refundCandidates.Count);
        var original = _refundCandidates[index];
        _refundCandidates.RemoveAt(index);

        // Refund a portion or full amount
        var refundAmount = _random.NextDouble() < 0.7
            ? original.Amount // Full refund
            : Math.Round((decimal)(_random.NextDouble() * 0.5 + 0.3) * original.Amount, 2); // Partial 30-80%

        var description = original.PaymentMethod switch
        {
            PaymentMethod.Visa => DescriptionBuilder.VisaRefund(original.Merchant!, GetCurrentLocation(date), date, _cardLast4),
            PaymentMethod.Eftpos => DescriptionBuilder.EftposRefund(original.Merchant!, date.AddHours(_random.Next(9, 18)), _cardLast4),
            _ => $"Refund - {original.Merchant}"
        };

        _balance += refundAmount;
        _transactions.Add(new Transaction
        {
            Date = date,
            Description = description,
            Credit = refundAmount,
            Balance = _balance,
            Category = original.Category,
            Merchant = original.Merchant,
            PaymentMethod = original.PaymentMethod
        });
    }

    private decimal CalculateAmount(TransactionTemplate template, int month)
    {
        var baseAmount = template.BaseAmount;

        // Apply seasonal multiplier if defined
        if (template.SeasonalMultiplier != null)
        {
            baseAmount *= template.SeasonalMultiplier(month);
        }

        if (template.FixedAmount)
        {
            return Math.Round(baseAmount, 2);
        }

        // Apply variance
        var variance = (decimal)((_random.NextDouble() * 2 - 1) * (double)template.AmountVariance);
        var amount = baseAmount * (1 + variance);

        // Add small random cents
        amount += (decimal)(_random.NextDouble() * 0.99);

        return Math.Round(amount, 2);
    }

    private string BuildDescription(TransactionTemplate template, string merchant, string location, DateTime date, decimal amount)
    {
        var dateTime = date.AddHours(_random.Next(7, 21)).AddMinutes(_random.Next(0, 60));

        return template.PaymentMethod switch
        {
            PaymentMethod.Visa => DescriptionBuilder.VisaPurchase(merchant, location, dateTime, _cardLast4),
            PaymentMethod.Eftpos => DescriptionBuilder.EftposPurchase(merchant, dateTime, _cardLast4),
            PaymentMethod.DirectDebit => DescriptionBuilder.DirectDebit(merchant, GenerateReference()),
            PaymentMethod.DirectCredit when template.Category == "Income" && merchant != "ATO TAX REFUND" && merchant != "MEDICARE AUSTRALIA" =>
                DescriptionBuilder.SalaryDeposit(_employer),
            PaymentMethod.DirectCredit when merchant == "ATO TAX REFUND" =>
                DescriptionBuilder.TaxRefund(),
            PaymentMethod.DirectCredit when merchant == "MEDICARE AUSTRALIA" =>
                DescriptionBuilder.MedicareRebate(),
            PaymentMethod.DirectCredit => DescriptionBuilder.SimpleCredit(merchant),
            PaymentMethod.Bpay => DescriptionBuilder.BpayPayment(merchant, GenerateBillerCode()),
            PaymentMethod.Osko => DescriptionBuilder.OskoPayment("Transfer", "SAVINGS ACCOUNT", GenerateReference()),
            PaymentMethod.InternalTransfer => DescriptionBuilder.InternalTransfer(merchant, "SAVINGS"),
            PaymentMethod.Atm => DescriptionBuilder.AtmWithdrawal(dateTime, 2.50m, "OTHER BANK", _cardLast4),
            _ => merchant
        };
    }

    private string GenerateReference()
    {
        return $"REF{_random.Next(10000000, 99999999)}";
    }

    private string GenerateBillerCode()
    {
        return _random.Next(100000, 999999).ToString();
    }
}
