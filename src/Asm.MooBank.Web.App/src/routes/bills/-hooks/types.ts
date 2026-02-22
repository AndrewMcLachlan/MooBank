export interface BillFilter {
    startDate?: string;
    endDate?: string;
    accountId?: string;
    utilityType?: string;
}

// Re-export generated types that consumers import from this module
export type { CostPerUnitReport, CostDataPoint, ServiceChargeReport, ServiceChargeDataPoint, UsageReport, UsageDataPoint } from "api/types.gen";
