import { useEffect } from "react";
import { useApp } from "../components";

export function usePageTitle(title: string) {

    const { appName } = useApp();

    useEffect(() => {
        document.title = `${title} : ${appName}`;
    }, [title]);
}