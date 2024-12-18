import { useParams } from "react-router";

export const useAccountRoute = () => {

    const { id, virtualId } = useParams<{ id: string, virtualId?: string }>();

    const base = `/accounts/${id}`;

    return virtualId ? `${base}/virtual/${virtualId}` : base;
}
