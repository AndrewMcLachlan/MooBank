import { Action } from "redux";

export interface ActionWithData<T> extends Action {
    data: T;
}