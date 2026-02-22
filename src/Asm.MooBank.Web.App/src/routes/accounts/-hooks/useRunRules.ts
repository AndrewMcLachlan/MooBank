import { useMutation } from "@tanstack/react-query";
import { runRulesMutation } from "api/@tanstack/react-query.gen";

export const useRunRules = () => useMutation({ ...runRulesMutation() });
