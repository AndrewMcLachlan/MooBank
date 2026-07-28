import { useQuery } from "@tanstack/react-query";
import { getAllRetirementPlansOptions } from "api/@tanstack/react-query.gen";

export const useRetirementPlans = () =>
    useQuery({
        ...getAllRetirementPlansOptions(),
    });
