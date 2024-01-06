import { Dispatch, SetStateAction, useEffect, useState } from "react";

export const useStateEffect = <S>(initialState: S | (() => S), dependency: S): [S, Dispatch<SetStateAction<S>>] => {

    const state = useState(initialState);

    useEffect(() => {
        state[1](dependency);
    }, [dependency]);

    return state;
};