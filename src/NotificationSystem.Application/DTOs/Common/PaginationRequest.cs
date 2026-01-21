namespace NotificationSystem.Application.DTOs.Common;

public record PaginationRequest(
    int PageNumber = 1,
    int PageSize = 10
);
