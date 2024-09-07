export interface StockValueReport {
    instrumentId: string;
    symbol: string;
    start: Date;
    end: Date;
    granularity: number;
    points: StockValueReportPoint[];
    investment: StockValueReportPoint[];
}

export interface StockValueReportPoint {
    date: Date;
    value: number;
}
