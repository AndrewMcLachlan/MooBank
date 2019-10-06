import React, { useEffect, Ref } from "react";


export const useClickAway = (setShow: (value: boolean) => void, ref: React.RefObject<any>) => {

    function handleClickOutside(event: Event) {
        if (ref.current && !ref.current.contains(event.target)) {
            setShow(false);
        }
    }

    useEffect(() => {
        document.addEventListener("mousedown", handleClickOutside);
        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };
    });
}
