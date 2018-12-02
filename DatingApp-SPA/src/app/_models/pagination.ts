export interface Pagination {
    currentPage: number;
    itemsPerPage: number;
    totalItems: number;
    totalPages: number;
}

// generic T can be users or messages
export class PaginatedResult<T> {
    result: T;
    pagination: Pagination;
}
