namespace Identity.API.Data.Dtos.Pagination;

public record PaginationParameters(int PageNumber = 1, int PageSize = 10);