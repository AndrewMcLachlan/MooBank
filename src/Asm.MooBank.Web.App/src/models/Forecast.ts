export interface ForecastPlan {
    id: string;
    name: string;
    startDate: string;
    endDate: string;
    accountScopeMode: AccountScopeMode;
    startingBalanceMode: StartingBalanceMode;
    startingBalanceAmount?: number;
    currencyCode?: string;
    incomeStrategy?: IncomeStrategy;
    outgoingStrategy?: OutgoingStrategy;
    assumptions?: Assumptions;
    isArchived: boolean;
    createdUtc: string;
    updatedUtc: string;
    accountIds: string[];
    plannedItems: PlannedItem[];
}

export type AccountScopeMode = "AllAccounts" | "SelectedAccounts";
export type StartingBalanceMode = "CalculatedCurrent" | "ManualAmount";

export interface PlannedItem {
    id: string;
    itemType: PlannedItemType;
    name: string;
    amount: number;
    tagId?: number;
    tagName?: string;
    virtualInstrumentId?: string;
    isIncluded: boolean;
    dateMode: PlannedItemDateMode;

    // Fixed date
    fixedDate?: string;

    // Schedule
    scheduleFrequency?: ScheduleFrequency;
    scheduleAnchorDate?: string;
    scheduleInterval?: number;
    scheduleDayOfMonth?: number;
    scheduleEndDate?: string;

    // Flexible window
    windowStartDate?: string;
    windowEndDate?: string;
    allocationMode?: AllocationMode;

    notes?: string;
}

export type PlannedItemType = "Expense" | "Income";
export type PlannedItemDateMode = "FixedDate" | "Schedule" | "FlexibleWindow";
export type ScheduleFrequency = "Daily" | "Weekly" | "Fortnightly" | "Monthly" | "Yearly";
export type AllocationMode = "EvenlySpread" | "AllAtEnd";

export interface IncomeStrategy {
    version?: number;
    mode?: string;
    manualRecurring?: ManualRecurringIncome;
    manualAdjustments?: ManualAdjustment[];
    historical?: HistoricalIncomeSettings;
}

export interface ManualRecurringIncome {
    amount: number;
    frequency?: string;
    startDate?: string;
    endDate?: string;
}

export interface ManualAdjustment {
    date: string;
    deltaAmount: number;
}

export interface HistoricalIncomeSettings {
    lookbackMonths?: number;
    includeTagIds?: number[];
    excludeTagIds?: number[];
    excludeTransfers?: boolean;
    excludeOffsets?: boolean;
}

export interface OutgoingStrategy {
    version?: number;
    mode?: string;
    lookbackMonths?: number;
    excludeTagIds?: number[];
    excludeTransfers?: boolean;
    excludeOffsets?: boolean;
    excludeAboveAmount?: number;
    seasonality?: SeasonalitySettings;
}

export interface SeasonalitySettings {
    enabled?: boolean;
}

export interface Assumptions {
    version?: number;
    inflationRateAnnual?: number;
    applyInflationToBaseline?: boolean;
    applyInflationToPlanned?: boolean;
    safetyBuffer?: number;
}

export interface ForecastResult {
    planId: string;
    months: ForecastMonth[];
    summary: ForecastSummary;
}

export interface ForecastMonth {
    monthStart: string;
    openingBalance: number;
    incomeTotal: number;
    baselineOutgoingsTotal: number;
    plannedItemsTotal: number;
    closingBalance: number;
}

export interface ForecastSummary {
    lowestBalance: number;
    lowestBalanceMonth: string;
    requiredMonthlyUplift: number;
    monthsBelowZero: number;
    totalIncome: number;
    totalOutgoings: number;
}

export interface CreateForecastPlan {
    name: string;
    startDate: string;
    endDate: string;
    accountScopeMode?: AccountScopeMode;
    startingBalanceMode?: StartingBalanceMode;
    startingBalanceAmount?: number;
    currencyCode?: string;
    incomeStrategy?: IncomeStrategy;
    outgoingStrategy?: OutgoingStrategy;
    assumptions?: Assumptions;
    accountIds?: string[];
}

export interface CreatePlannedItem {
    itemType: PlannedItemType;
    name: string;
    amount: number;
    tagId?: number;
    virtualInstrumentId?: string;
    isIncluded?: boolean;
    dateMode: PlannedItemDateMode;
    fixedDate?: string;
    scheduleFrequency?: ScheduleFrequency;
    scheduleAnchorDate?: string;
    scheduleInterval?: number;
    scheduleDayOfMonth?: number;
    scheduleEndDate?: string;
    windowStartDate?: string;
    windowEndDate?: string;
    allocationMode?: AllocationMode;
    notes?: string;
}
