export interface BaseReport {
    accountId: string,
    start: string,
    end: string,
}

export type reportInterval = "Monthly" | "Yearly";