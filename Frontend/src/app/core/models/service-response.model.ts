export interface PaginationMetaDto {
  pageNumber: number;
  pageSize: number;
  totalRecords: number;
  totalPages: number;
}

export interface ServiceResponse<T> {
  isSuccess?: boolean;
  data?: T | null;
  meta?: PaginationMetaDto | null;
  statusCode: number;
}
