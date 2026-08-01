import { useQuery } from "@tanstack/react-query";
import { getRetirementPeopleOptions } from "api/@tanstack/react-query.gen";

/**
 * The people a retirement plan can include, and the superannuation accounts each of them owns.
 *
 * A plan member names a person in the family rather than carrying a name of its own, and the
 * accounts they can be credited with are the ones they own — both come from here, on the same rule
 * the server enforces when the plan is saved.
 *
 * `isPending` matters to the caller: a select cannot show a person who is not yet among its options,
 * so the form must wait for these before it renders.
 */
export const useFamilyMembers = () => {
    const { data, isPending, ...rest } = useQuery({
        ...getRetirementPeopleOptions(),
        staleTime: 5 * 60 * 1000,
    });

    return { members: data ?? [], isPending, ...rest };
};
