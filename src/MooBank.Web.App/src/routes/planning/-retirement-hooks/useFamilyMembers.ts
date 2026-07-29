import { useQuery } from "@tanstack/react-query";
import { getMyFamilyOptions } from "api/@tanstack/react-query.gen";

/**
 * The people a retirement plan can include, and the accounts each of them owns.
 *
 * A plan member is a person in the family rather than a typed-in name, and the accounts they can be
 * credited with are the ones they own — both of which come from here.
 */
export const useFamilyMembers = () => {
    const { data, ...rest } = useQuery({
        ...getMyFamilyOptions(),
        staleTime: 5 * 60 * 1000,
    });

    return { members: data?.members ?? [], ...rest };
};
