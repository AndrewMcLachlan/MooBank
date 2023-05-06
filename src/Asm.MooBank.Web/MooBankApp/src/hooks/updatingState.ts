import { useEffect, useState } from "react"

export const useUpdatingState = <T>(value: T): [T, React.Dispatch<React.SetStateAction<T>>] => {

    const [state, setState] = useState<T>(value);

    useEffect(() => {
        setState(value);
    }, [value]);

    return [state, setState];
}