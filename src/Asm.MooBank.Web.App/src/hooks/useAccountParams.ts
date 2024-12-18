import { useParams } from "react-router"

export const useAccountParams = () => {
    const { accountId } = useParams<{accountId: string}>();

    return accountId!;
}
