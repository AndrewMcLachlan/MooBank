export const trimEnd = (charsToRemove: string, string: string) => {

    if (!string || !charsToRemove || charsToRemove === "") return string;

    const length = charsToRemove.length;

    let trimmedString = string;

    while (trimmedString.endsWith(charsToRemove)) {
        trimmedString = trimmedString.slice(0, -length);
    }

    return trimmedString;
}