namespace Asm.MooBank.Tools.TransactionGenerator;

/// <summary>
/// Australian merchant data organized by category.
/// </summary>
public static class Merchants
{
    public static readonly string[] Supermarkets =
    [
        "WOOLWORTHS", "COLES", "ALDI", "IGA", "HARRIS FARM MARKETS",
        "FOODWORKS", "COSTCO", "SUPA IGA"
    ];

    public static readonly string[] CoffeeShops =
    [
        "THE COFFEE CLUB", "GLORIA JEANS", "STARBUCKS", "MUFFIN BREAK",
        "ZARAFFAS COFFEE", "HUDSONS COFFEE", "SOUL ORIGIN", "GUZMAN Y GOMEZ"
    ];

    public static readonly string[] Restaurants =
    [
        "MCDONALDS", "KFC", "HUNGRY JACKS", "SUBWAY", "DOMINOS PIZZA",
        "PIZZA HUT", "NANDOS", "OPORTO", "RED ROOSTER", "GRILL'D",
        "LONE STAR RIB HOUSE", "HOGS BREATH CAFE", "SIZZLER",
        "THAI ORCHID", "FORTUNE PALACE", "SAKURA JAPANESE", "INDIA GATE"
    ];

    public static readonly string[] Takeaway =
    [
        "UBER EATS", "MENULOG", "DOORDASH", "DELIVEROO",
        "MCDONALDS", "KFC", "DOMINOS PIZZA", "SUBWAY"
    ];

    public static readonly string[] FuelStations =
    [
        "BP", "SHELL", "CALTEX", "7-ELEVEN", "AMPOL", "UNITED PETROLEUM",
        "COSTCO FUEL", "METRO PETROLEUM", "LIBERTY OIL"
    ];

    public static readonly string[] BottleShops =
    [
        "DAN MURPHYS", "BWS", "LIQUORLAND", "VINTAGE CELLARS",
        "FIRST CHOICE LIQUOR", "ALDI LIQUOR"
    ];

    public static readonly string[] Pharmacies =
    [
        "CHEMIST WAREHOUSE", "PRICELINE PHARMACY", "TERRY WHITE CHEMMART",
        "AMCAL PHARMACY", "BLOOMS THE CHEMIST", "SOUL PATTINSON"
    ];

    public static readonly string[] Retail =
    [
        "BUNNINGS WAREHOUSE", "JB HI-FI", "KMART", "TARGET", "BIG W",
        "OFFICEWORKS", "HARVEY NORMAN", "THE GOOD GUYS", "MYER", "DAVID JONES"
    ];

    public static readonly string[] ClothingStores =
    [
        "COTTON ON", "UNIQLO", "H&M", "ZARA", "COUNTRY ROAD",
        "KATHMANDU", "REBEL SPORT", "ATHLETES FOOT", "NIKE", "ADIDAS"
    ];

    public static readonly string[] Entertainment =
    [
        "HOYTS CINEMAS", "EVENT CINEMAS", "VILLAGE CINEMAS", "IMAX",
        "TIMEZONE", "STRIKE BOWLING"
    ];

    public static readonly string[] Streaming =
    [
        "NETFLIX.COM", "SPOTIFY", "STAN", "DISNEY PLUS", "AMAZON PRIME",
        "APPLE.COM", "YOUTUBE PREMIUM", "BINGE", "PARAMOUNT PLUS"
    ];

    public static readonly string[] Utilities =
    [
        "ORIGIN ENERGY", "AGL ENERGY", "ENERGY AUSTRALIA", "RED ENERGY",
        "ALINTA ENERGY", "SIMPLY ENERGY"
    ];

    public static readonly string[] Water =
    [
        "SYDNEY WATER", "MELBOURNE WATER", "SA WATER", "URBAN UTILITIES",
        "YARRA VALLEY WATER", "SOUTH EAST WATER"
    ];

    public static readonly string[] Telecom =
    [
        "TELSTRA", "OPTUS", "VODAFONE", "TPG", "AUSSIE BROADBAND",
        "BELONG", "BOOST MOBILE", "AMAYSIM"
    ];

    public static readonly string[] Insurance =
    [
        "NRMA INSURANCE", "ALLIANZ", "SUNCORP", "QBE INSURANCE",
        "AAMI", "GIO", "BUDGET DIRECT", "YOUI"
    ];

    public static readonly string[] HealthInsurance =
    [
        "MEDIBANK PRIVATE", "BUPA", "HCF", "NIB HEALTH FUNDS",
        "AUSTRALIAN UNITY", "HBF"
    ];

    public static readonly string[] Medical =
    [
        "DR", "MEDICAL CENTRE", "PATHOLOGY", "RADIOLOGY",
        "SPECIALIST", "ALLIED HEALTH"
    ];

    public static readonly string[] Dental =
    [
        "DENTAL CARE", "FAMILY DENTAL", "SMILE DENTAL",
        "PACIFIC SMILES", "DENTAL CORP"
    ];

    public static readonly string[] Haircut =
    [
        "JUST CUTS", "BARBER SHOP", "HAIR HOUSE WAREHOUSE",
        "TONI&GUY", "HAIR BY"
    ];

    public static readonly string[] Gyms =
    [
        "FITNESS FIRST", "ANYTIME FITNESS", "GOODLIFE HEALTH CLUBS",
        "F45 TRAINING", "PLUS FITNESS", "JETTS FITNESS"
    ];

    public static readonly string[] Transport =
    [
        "UBER", "DIDI", "OLA", "13CABS", "SILVER SERVICE"
    ];

    public static readonly string[] Parking =
    [
        "WILSON PARKING", "SECURE PARKING", "CARE PARK", "ACE PARKING"
    ];

    public static readonly string[] PublicTransport =
    [
        "OPAL TOP UP", "MYKI TOP UP", "GO CARD TOP UP", "METROCARD"
    ];

    public static readonly string[] CarService =
    [
        "ULTRATUNE", "MIDAS", "KMART TYRE AUTO", "GOODYEAR AUTOCARE",
        "BRIDGESTONE", "BOB JANE T-MARTS", "MYCAR"
    ];

    public static readonly string[] Airlines =
    [
        "QANTAS AIRWAYS", "VIRGIN AUSTRALIA", "JETSTAR", "REX AIRLINES"
    ];

    public static readonly string[] Hotels =
    [
        "ACCOR HOTELS", "MARRIOTT", "HILTON", "IHG HOTELS",
        "QUEST APARTMENTS", "MANTRA GROUP", "BOOKING.COM", "AIRBNB"
    ];

    public static readonly string[] GiftShops =
    [
        "TYPO", "SMIGGLE", "KIKKI.K", "PETER ALEXANDER",
        "DUSK", "BED BATH N TABLE"
    ];

    public static readonly string[] Employers =
    [
        "ACME PTY LTD", "SMITH AND ASSOCIATES", "TECH SOLUTIONS AUSTRALIA",
        "GLOBAL SERVICES PTY LTD", "AUSTRALIAN ENTERPRISES"
    ];

    // Brisbane metro area locations (home base)
    public static readonly string[] BrisbaneLocations =
    [
        "BRISBANE", "BRISBANE CBD", "SOUTH BRISBANE", "FORTITUDE VALLEY",
        "CHERMSIDE", "CARINDALE", "INDOOROOPILLY", "TOOWONG", "SUNNYBANK",
        "MT GRAVATT", "GARDEN CITY", "WESTFIELD CHERMSIDE"
    ];

    // Drive destinations from Brisbane (no flights needed)
    public static readonly string[] DriveDestinations =
    [
        "GOLD COAST", "SUNSHINE COAST", "BYRON BAY", "TOOWOOMBA"
    ];

    // Flight destinations with approximate distance from Brisbane
    public static readonly (string City, int DistanceKm)[] FlightDestinations =
    [
        ("SYDNEY", 900),
        ("CANBERRA", 1200),
        ("TOWNSVILLE", 1300),
        ("CAIRNS", 1700),
        ("MELBOURNE", 1700),
        ("ADELAIDE", 2000),
        ("HOBART", 2100),
        ("PERTH", 3600)
    ];

    private static readonly Random _random = new();

    public static string GetRandom(string[] merchants) =>
        merchants[_random.Next(merchants.Length)];

    public static string GetBrisbaneLocation() =>
        BrisbaneLocations[_random.Next(BrisbaneLocations.Length)];

    public static string GetDriveDestination() =>
        DriveDestinations[_random.Next(DriveDestinations.Length)];

    public static (string City, int DistanceKm) GetFlightDestination() =>
        FlightDestinations[_random.Next(FlightDestinations.Length)];

    public static string GetRandomEmployer() =>
        Employers[_random.Next(Employers.Length)];

    /// <summary>
    /// Calculate approximate flight cost based on distance from Brisbane.
    /// </summary>
    public static decimal GetFlightCost(int distanceKm, bool isHoliday = false)
    {
        // Base cost increases with distance
        // Short haul (Sydney/Canberra): $150-300
        // Medium haul (Melbourne/Cairns): $200-400
        // Long haul (Perth): $300-600
        // Holiday flights are 2-3x more expensive (peak season, booked late)

        var baseCost = distanceKm switch
        {
            < 1000 => 120m + (decimal)(_random.NextDouble() * 130),   // $120-250
            < 1500 => 180m + (decimal)(_random.NextDouble() * 170),   // $180-350
            < 2500 => 250m + (decimal)(_random.NextDouble() * 200),   // $250-450
            _ => 350m + (decimal)(_random.NextDouble() * 300)          // $350-650 (Perth)
        };

        return isHoliday ? baseCost * (2m + (decimal)(_random.NextDouble() * 0.5)) : baseCost;
    }
}
