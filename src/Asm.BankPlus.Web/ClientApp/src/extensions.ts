export type NameValue = {
    name: string,
    value: number,
};

export const toNameValue = (type: any):NameValue[] => {

    let result:NameValue[] = [];

    for (const value in type) {
        const realValue = (value as unknown) as number;
        if (isNaN(realValue)) continue;

        result.push({ name: type[value], value: realValue });
    }

    return result;
}