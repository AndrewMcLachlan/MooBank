export const valueAsNumber = (formControl: any, defaultValue: number = 0): number => {
    if (formControl.value === undefined || formControl.value === null || formControl.value === "") {
        return defaultValue;
    }

    return formControl.valueAsNumber;
}