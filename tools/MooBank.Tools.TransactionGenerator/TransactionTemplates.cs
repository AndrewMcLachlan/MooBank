namespace Asm.MooBank.Tools.TransactionGenerator;

/// <summary>
/// Payment method types that determine how the description is formatted.
/// </summary>
public enum PaymentMethod
{
    Visa,
    Eftpos,
    DirectDebit,
    DirectCredit,
    Bpay,
    Osko,
    InternalTransfer,
    Atm
}

/// <summary>
/// Defines when a transaction should occur.
/// </summary>
public enum ScheduleType
{
    /// <summary>Occurs roughly every N days with variance.</summary>
    Frequency,
    /// <summary>Occurs on a specific day of each month.</summary>
    MonthlyOnDay,
    /// <summary>Occurs on a specific day of a specific month.</summary>
    Yearly
}

/// <summary>
/// Template defining a type of transaction with its generation parameters.
/// </summary>
public class TransactionTemplate
{
    public required string Category { get; init; }
    public required string[] Merchants { get; init; }
    public required decimal BaseAmount { get; init; }
    public decimal AmountVariance { get; init; } = 0.1m; // 10% variance by default
    public required PaymentMethod PaymentMethod { get; init; }
    public required ScheduleType ScheduleType { get; init; }

    // For Frequency schedule
    public int FrequencyDays { get; init; }
    public int FrequencyVariance { get; init; } = 2; // Days variance

    // For MonthlyOnDay schedule
    public int DayOfMonth { get; init; }

    // For Yearly schedule
    public int Month { get; init; }
    public int Day { get; init; }

    public bool IsCredit { get; init; } = false;
    public bool FixedAmount { get; init; } = false;

    // Weekend preference: higher values mean more likely on weekends
    public double WeekendBias { get; init; } = 1.0; // 1.0 = no bias, 2.0 = twice as likely on weekends

    // Seasonal multiplier function (month 1-12) -> amount multiplier
    public Func<int, decimal>? SeasonalMultiplier { get; init; }

    // Track last occurrence for frequency-based scheduling
    public DateTime? LastOccurrence { get; set; }

    // For generating related transactions (e.g., Medicare after Doctor)
    public TransactionTemplate? FollowUpTransaction { get; init; }
    public int FollowUpDelayDays { get; init; } = 3;
}

/// <summary>
/// Collection of all transaction templates organized by category.
/// </summary>
public static class TransactionTemplates
{
    // Seasonal multiplier for electricity (higher in summer and winter)
    private static readonly Func<int, decimal> ElectricitySeasonalMultiplier = month =>
        month switch
        {
            12 or 1 or 2 => 1.4m,  // Summer (Australia)
            6 or 7 or 8 => 1.3m,   // Winter
            _ => 1.0m
        };

    // Seasonal multiplier for gas (higher in winter)
    private static readonly Func<int, decimal> GasSeasonalMultiplier = month =>
        month switch
        {
            6 or 7 or 8 => 1.5m,   // Winter
            5 or 9 => 1.2m,        // Shoulder seasons
            _ => 0.8m              // Summer - less heating
        };

    public static readonly TransactionTemplate[] FrequencyBased =
    [
        // Supermarket - big weekly shop
        new TransactionTemplate
        {
            Category = "Groceries",
            Merchants = Merchants.Supermarkets,
            BaseAmount = 180m,
            AmountVariance = 0.25m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 7,
            FrequencyVariance = 1
        },
        // Supermarket - small top-up
        new TransactionTemplate
        {
            Category = "Groceries",
            Merchants = Merchants.Supermarkets,
            BaseAmount = 25m,
            AmountVariance = 0.3m,
            PaymentMethod = PaymentMethod.Eftpos,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 3,
            FrequencyVariance = 1
        },
        // Coffee
        new TransactionTemplate
        {
            Category = "Coffee",
            Merchants = Merchants.CoffeeShops,
            BaseAmount = 5.50m,
            AmountVariance = 0.15m,
            PaymentMethod = PaymentMethod.Eftpos,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 2,
            FrequencyVariance = 1,
            WeekendBias = 0.7 // Less likely on weekends
        },
        // Restaurant dining
        new TransactionTemplate
        {
            Category = "Dining",
            Merchants = Merchants.Restaurants,
            BaseAmount = 75m,
            AmountVariance = 0.35m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 12,
            FrequencyVariance = 4,
            WeekendBias = 2.5 // Much more likely on weekends
        },
        // Takeaway
        new TransactionTemplate
        {
            Category = "Takeaway",
            Merchants = Merchants.Takeaway,
            BaseAmount = 35m,
            AmountVariance = 0.3m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 8,
            FrequencyVariance = 3,
            WeekendBias = 1.5
        },
        // Fuel
        new TransactionTemplate
        {
            Category = "Transport",
            Merchants = Merchants.FuelStations,
            BaseAmount = 85m,
            AmountVariance = 0.2m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 12,
            FrequencyVariance = 3
        },
        // Bottle shop
        new TransactionTemplate
        {
            Category = "Alcohol",
            Merchants = Merchants.BottleShops,
            BaseAmount = 50m,
            AmountVariance = 0.4m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 14,
            FrequencyVariance = 5,
            WeekendBias = 1.8
        },
        // Pharmacy
        new TransactionTemplate
        {
            Category = "Health",
            Merchants = Merchants.Pharmacies,
            BaseAmount = 45m,
            AmountVariance = 0.5m,
            PaymentMethod = PaymentMethod.Eftpos,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 28,
            FrequencyVariance = 7
        },
        // Doctor visit with Medicare follow-up
        new TransactionTemplate
        {
            Category = "Health",
            Merchants = ["MEDICAL CENTRE", "DR SMITH MEDICAL", "FAMILY MEDICAL PRACTICE", "HEALTHPOINT CLINIC"],
            BaseAmount = 85m,
            AmountVariance = 0.2m,
            PaymentMethod = PaymentMethod.Eftpos,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 75,
            FrequencyVariance = 20,
            FollowUpTransaction = new TransactionTemplate
            {
                Category = "Health",
                Merchants = ["MEDICARE AUSTRALIA"],
                BaseAmount = 38m,
                AmountVariance = 0.1m,
                PaymentMethod = PaymentMethod.DirectCredit,
                ScheduleType = ScheduleType.Frequency,
                FrequencyDays = 0, // Triggered by parent
                IsCredit = true
            },
            FollowUpDelayDays = 5
        },
        // Haircut
        new TransactionTemplate
        {
            Category = "Personal Care",
            Merchants = Merchants.Haircut,
            BaseAmount = 45m,
            AmountVariance = 0.15m,
            PaymentMethod = PaymentMethod.Eftpos,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 42,
            FrequencyVariance = 5
        },
        // Retail shopping
        new TransactionTemplate
        {
            Category = "Shopping",
            Merchants = Merchants.Retail,
            BaseAmount = 65m,
            AmountVariance = 0.5m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 21,
            FrequencyVariance = 10,
            WeekendBias = 2.0
        },
        // Clothing
        new TransactionTemplate
        {
            Category = "Clothing",
            Merchants = Merchants.ClothingStores,
            BaseAmount = 85m,
            AmountVariance = 0.5m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 35,
            FrequencyVariance = 15,
            WeekendBias = 2.0
        },
        // Entertainment (cinema, etc.)
        new TransactionTemplate
        {
            Category = "Entertainment",
            Merchants = Merchants.Entertainment,
            BaseAmount = 35m,
            AmountVariance = 0.3m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 30,
            FrequencyVariance = 15,
            WeekendBias = 3.0 // Very weekend-heavy
        },
        // Public transport top-up
        new TransactionTemplate
        {
            Category = "Transport",
            Merchants = Merchants.PublicTransport,
            BaseAmount = 50m,
            AmountVariance = 0.2m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 14,
            FrequencyVariance = 5,
            WeekendBias = 0.5 // Weekday commuting
        },
        // Uber/rideshare
        new TransactionTemplate
        {
            Category = "Transport",
            Merchants = Merchants.Transport,
            BaseAmount = 25m,
            AmountVariance = 0.4m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 10,
            FrequencyVariance = 5,
            WeekendBias = 2.0
        },
        // Dentist (6-monthly)
        new TransactionTemplate
        {
            Category = "Health",
            Merchants = Merchants.Dental,
            BaseAmount = 220m,
            AmountVariance = 0.3m,
            PaymentMethod = PaymentMethod.Eftpos,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 180,
            FrequencyVariance = 30
        },
        // Car service
        new TransactionTemplate
        {
            Category = "Transport",
            Merchants = Merchants.CarService,
            BaseAmount = 450m,
            AmountVariance = 0.4m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 180,
            FrequencyVariance = 30
        },
        // Gifts
        new TransactionTemplate
        {
            Category = "Gifts",
            Merchants = Merchants.GiftShops,
            BaseAmount = 55m,
            AmountVariance = 0.5m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 45,
            FrequencyVariance = 20
        }
    ];

    public static readonly TransactionTemplate[] MonthlyBills =
    [
        // Salary (net after tax, ~$110k gross annual)
        new TransactionTemplate
        {
            Category = "Income",
            Merchants = Merchants.Employers,
            BaseAmount = 7000m,
            PaymentMethod = PaymentMethod.DirectCredit,
            ScheduleType = ScheduleType.MonthlyOnDay,
            DayOfMonth = 1,
            IsCredit = true,
            FixedAmount = true
        },
        // Health Insurance
        new TransactionTemplate
        {
            Category = "Insurance",
            Merchants = Merchants.HealthInsurance,
            BaseAmount = 380m,
            AmountVariance = 0.05m,
            PaymentMethod = PaymentMethod.DirectDebit,
            ScheduleType = ScheduleType.MonthlyOnDay,
            DayOfMonth = 3
        },
        // Gym membership
        new TransactionTemplate
        {
            Category = "Health",
            Merchants = Merchants.Gyms,
            BaseAmount = 90m,
            PaymentMethod = PaymentMethod.DirectDebit,
            ScheduleType = ScheduleType.MonthlyOnDay,
            DayOfMonth = 10,
            FixedAmount = true
        },
        // Mobile phone
        new TransactionTemplate
        {
            Category = "Utilities",
            Merchants = Merchants.Telecom,
            BaseAmount = 55m,
            AmountVariance = 0.1m,
            PaymentMethod = PaymentMethod.DirectDebit,
            ScheduleType = ScheduleType.MonthlyOnDay,
            DayOfMonth = 15
        },
        // Internet
        new TransactionTemplate
        {
            Category = "Utilities",
            Merchants = Merchants.Telecom,
            BaseAmount = 80m,
            PaymentMethod = PaymentMethod.DirectDebit,
            ScheduleType = ScheduleType.MonthlyOnDay,
            DayOfMonth = 15,
            FixedAmount = true
        },
        // Netflix
        new TransactionTemplate
        {
            Category = "Entertainment",
            Merchants = ["NETFLIX.COM"],
            BaseAmount = 23m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.MonthlyOnDay,
            DayOfMonth = 20,
            FixedAmount = true
        },
        // Spotify
        new TransactionTemplate
        {
            Category = "Entertainment",
            Merchants = ["SPOTIFY"],
            BaseAmount = 13m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.MonthlyOnDay,
            DayOfMonth = 22,
            FixedAmount = true
        },
        // Mortgage
        new TransactionTemplate
        {
            Category = "Housing",
            Merchants = ["HOME LOAN"],
            BaseAmount = 2200m,
            PaymentMethod = PaymentMethod.DirectDebit,
            ScheduleType = ScheduleType.MonthlyOnDay,
            DayOfMonth = 28,
            FixedAmount = true
        }
    ];

    public static readonly TransactionTemplate[] QuarterlyBills =
    [
        // Electricity
        new TransactionTemplate
        {
            Category = "Utilities",
            Merchants = Merchants.Utilities,
            BaseAmount = 350m,
            AmountVariance = 0.3m,
            PaymentMethod = PaymentMethod.Bpay,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 90,
            FrequencyVariance = 5,
            SeasonalMultiplier = ElectricitySeasonalMultiplier
        },
        // Gas
        new TransactionTemplate
        {
            Category = "Utilities",
            Merchants = Merchants.Utilities,
            BaseAmount = 120m,
            AmountVariance = 0.3m,
            PaymentMethod = PaymentMethod.Bpay,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 90,
            FrequencyVariance = 5,
            SeasonalMultiplier = GasSeasonalMultiplier
        },
        // Water
        new TransactionTemplate
        {
            Category = "Utilities",
            Merchants = Merchants.Water,
            BaseAmount = 220m,
            AmountVariance = 0.2m,
            PaymentMethod = PaymentMethod.Bpay,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 90,
            FrequencyVariance = 5
        },
        // Council rates
        new TransactionTemplate
        {
            Category = "Housing",
            Merchants = ["COUNCIL RATES"],
            BaseAmount = 450m,
            AmountVariance = 0.05m,
            PaymentMethod = PaymentMethod.Bpay,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 90,
            FrequencyVariance = 5
        }
    ];

    public static readonly TransactionTemplate[] YearlyEvents =
    [
        // Car rego
        new TransactionTemplate
        {
            Category = "Transport",
            Merchants = ["SERVICE NSW", "VICROADS", "TRANSPORT QLD"],
            BaseAmount = 820m,
            AmountVariance = 0.05m,
            PaymentMethod = PaymentMethod.Bpay,
            ScheduleType = ScheduleType.Yearly,
            Month = 3,
            Day = 15
        },
        // Car insurance
        new TransactionTemplate
        {
            Category = "Insurance",
            Merchants = Merchants.Insurance,
            BaseAmount = 950m,
            AmountVariance = 0.1m,
            PaymentMethod = PaymentMethod.Bpay,
            ScheduleType = ScheduleType.Yearly,
            Month = 4,
            Day = 1
        },
        // Home/contents insurance
        new TransactionTemplate
        {
            Category = "Insurance",
            Merchants = Merchants.Insurance,
            BaseAmount = 750m,
            AmountVariance = 0.1m,
            PaymentMethod = PaymentMethod.Bpay,
            ScheduleType = ScheduleType.Yearly,
            Month = 6,
            Day = 1
        },
        // Note: Holiday flights are generated dynamically in AccountGenerator.UpdateLocation()
        // Holiday accommodation
        new TransactionTemplate
        {
            Category = "Travel",
            Merchants = Merchants.Hotels,
            BaseAmount = 1200m,
            AmountVariance = 0.3m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.Yearly,
            Month = 5,
            Day = 12
        },
        // Tax refund
        new TransactionTemplate
        {
            Category = "Income",
            Merchants = ["ATO TAX REFUND"],
            BaseAmount = 2500m,
            AmountVariance = 0.3m,
            PaymentMethod = PaymentMethod.DirectCredit,
            ScheduleType = ScheduleType.Yearly,
            Month = 8,
            Day = 15,
            IsCredit = true
        },
        // Christmas bonus
        new TransactionTemplate
        {
            Category = "Income",
            Merchants = Merchants.Employers,
            BaseAmount = 3000m,
            AmountVariance = 0.3m,
            PaymentMethod = PaymentMethod.DirectCredit,
            ScheduleType = ScheduleType.Yearly,
            Month = 12,
            Day = 15,
            IsCredit = true
        }
    ];

    // Emergency/unexpected transactions (randomly selected throughout the year)
    public static readonly TransactionTemplate[] EmergencyTransactions =
    [
        new TransactionTemplate
        {
            Category = "Transport",
            Merchants = ["EMERGENCY REPAIRS", "ROADSIDE ASSIST", "MECHANIC"],
            BaseAmount = 800m,
            AmountVariance = 0.5m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 365 // Roughly once a year
        },
        new TransactionTemplate
        {
            Category = "Health",
            Merchants = ["EMERGENCY MEDICAL", "HOSPITAL", "SPECIALIST"],
            BaseAmount = 400m,
            AmountVariance = 0.6m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 300
        },
        new TransactionTemplate
        {
            Category = "Housing",
            Merchants = ["APPLIANCE DIRECT", "HARVEY NORMAN", "THE GOOD GUYS"],
            BaseAmount = 600m,
            AmountVariance = 0.5m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 400
        },
        new TransactionTemplate
        {
            Category = "Housing",
            Merchants = ["PLUMBER", "ELECTRICIAN", "HANDYMAN"],
            BaseAmount = 350m,
            AmountVariance = 0.5m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 250
        }
    ];

    // Special December spending cluster
    public static readonly TransactionTemplate[] ChristmasShopping =
    [
        new TransactionTemplate
        {
            Category = "Gifts",
            Merchants = Merchants.Retail,
            BaseAmount = 150m,
            AmountVariance = 0.5m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 5,
            FrequencyVariance = 2
        },
        new TransactionTemplate
        {
            Category = "Gifts",
            Merchants = Merchants.ClothingStores,
            BaseAmount = 100m,
            AmountVariance = 0.5m,
            PaymentMethod = PaymentMethod.Visa,
            ScheduleType = ScheduleType.Frequency,
            FrequencyDays = 7,
            FrequencyVariance = 3
        }
    ];

    // Transfer to savings (conditional)
    public static readonly TransactionTemplate SavingsTransfer = new()
    {
        Category = "Transfer",
        Merchants = ["SAVINGS"],
        BaseAmount = 1000m,
        AmountVariance = 0.3m,
        PaymentMethod = PaymentMethod.Osko,
        ScheduleType = ScheduleType.MonthlyOnDay,
        DayOfMonth = 28
    };
}
