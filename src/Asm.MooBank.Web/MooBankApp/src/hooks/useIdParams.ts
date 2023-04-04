import { useParams } from "react-router-dom";

export const useIdParams= () => {
    const { id } = useParams<{id: string}>();
    if (!id) throw Error("bad params");

    return id;
}