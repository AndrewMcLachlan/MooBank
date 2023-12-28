import { UseMutationResult } from "@tanstack/react-query";

export type AnyFunction = (...args: any[]) => void;

export interface UseCreateMutationResult<Function extends AnyFunction> extends Omit<UseMutationResult, "mutate"> {
    create: Function;
}

export interface UseUpdateMutationResult<Function extends AnyFunction> extends Omit<UseMutationResult, "mutate"> {
    update: Function;
}