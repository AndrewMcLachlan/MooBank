import { BaseReport } from "./BaseReport";

export interface ByTagReport extends BaseReport {
    tags: TagValue[],
}

export interface TagValue {
    tagId: number,
    tagName: string,
    grossAmount: number,
    netAmount?: number,
    hasChildren: boolean,
}