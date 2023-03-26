import { useEffect } from "react";
import { useApp } from "../providers";

export function usePageTitle(title?: string) {

    const { appName } = useApp();

    useEffect(() => {
        document.title = `${title} : ${appName}`;
    }, [title]);
}