export const onKeyLeave = (e: React.KeyboardEvent<HTMLInputElement | HTMLTextAreaElement>, setter: (value: string) => void) => {
    if (e.key === "Enter" || e.key === "Tab") {
        setter(e.currentTarget.value);
    }
}