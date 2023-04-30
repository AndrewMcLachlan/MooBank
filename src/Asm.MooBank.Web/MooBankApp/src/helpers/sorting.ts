import { sortDirection } from "store/state";

export const changeSortDirection = (sortDirection: sortDirection) => 
     sortDirection === "Ascending" ? "Descending" : "Ascending";
