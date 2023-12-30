import { UseMutationResult } from "@tanstack/react-query";

export type AnyFunction = (...args: any[]) => void;

export interface UseCreateMutationResult<Func extends AnyFunction> extends Omit<UseMutationResult, "mutate"> {
    create: Func;
}

export interface UseUpdateMutationResult<Func extends AnyFunction> extends Omit<UseMutationResult, "mutate"> {
    update: Func;
}