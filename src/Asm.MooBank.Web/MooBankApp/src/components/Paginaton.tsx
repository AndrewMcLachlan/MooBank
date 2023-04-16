import { getPagesToDisplay } from "helpers/paging";
import { Pagination as BSPagination } from "react-bootstrap"

export const Pagination : React.FC<PaginationProps> = ({pageNumber, numberOfPages, onChange}) => {
    
    const showNext = pageNumber < numberOfPages;
    const showPrev = pageNumber > 1;

    const pagesToDisplay = getPagesToDisplay(pageNumber, numberOfPages);

    return (
        <BSPagination>
            <BSPagination.First disabled={!showPrev} onClick={() => onChange(pageNumber, 1)} />
            <BSPagination.Prev disabled={!showPrev} onClick={() => onChange(pageNumber, Math.max(0, pageNumber-1))} />
            {pagesToDisplay.map((page) => (
                <BSPagination.Item key={page} active={page === pageNumber} onClick={() => onChange(pageNumber, page)}>
                    {page}
                </BSPagination.Item>
            ))}
            <BSPagination.Next disabled={!showNext} onClick={() => onChange(pageNumber, Math.min(pageNumber + 1, numberOfPages))} />
            <BSPagination.Last disabled={!showNext} onClick={() => onChange(pageNumber, numberOfPages)} />
        </BSPagination>
    );
}

export interface PaginationProps {
    pageNumber: number,
    numberOfPages: number,
    onChange: (currentPageNumber: number, newPageNumber: number) => void;
}