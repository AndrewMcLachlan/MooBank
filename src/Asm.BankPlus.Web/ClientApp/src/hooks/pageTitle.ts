import { useEffect } from "react";
import { useSelector } from "react-redux";

import { State } from "../store/state";

export function usePageTitle(title: string) {

    let appName = useSelector((state: State) => state.app.appName);

    useEffect(() => {
        document.title = `${title} : ${appName}`;
    }, [title]);
}