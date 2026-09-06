import type { BillPeriod } from "api/types.gen";

export interface BillFilter {
    /** A period named rather than dated, resolved by the server. Excludes startDate/endDate. */
    period?: BillPeriod;
    startDate?: string;
    endDate?: string;
    accountId?: string;
    utilityType?: string;
}

// Re-export generated types that consumers import from this module
export type { CostPerUnitReport, CostDataPoint, ServiceChargeReport, ServiceChargeDataPoint, UsageReport, UsageDataPoint } from "api/types.gen";
