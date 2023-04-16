export const getPagesToDisplay = (pageNumber: number, numberOfPages: number): number[] => {

    const pagesToDisplay = [];
    if (pageNumber < 3 || numberOfPages <= 5) {
        for (let i = 1; i <= 5 && i <= numberOfPages; i++) {
            pagesToDisplay.push(i);
        }
    } else if (pageNumber >= numberOfPages - 3) {
        for (let i = numberOfPages - 4; i <= numberOfPages; i++) {
            pagesToDisplay.push(i);
        }
    } else {
        for (let i = pageNumber - 2; i <= pageNumber + 2; i++) {
            pagesToDisplay.push(i);
        }
    }

    return pagesToDisplay;
}

export const getNumberOfPages = (total: number, pageSize: number) => Math.max(1, Math.ceil(total / pageSize));