import { useParams } from "react-router-dom"

export const useAccountParams = () => {
    const { accountId } = useParams<{accountId: string}>();

    return accountId!;
}