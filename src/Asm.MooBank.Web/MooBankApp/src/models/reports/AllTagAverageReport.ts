import { BaseReport } from "./BaseReport";
import { TagValue } from "./ByTagReport";

export interface AllTagAverageReport extends BaseReport {
    tags: TagValue[],
}
