using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Common;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.FreshMarketManagement;

public class OrderRepository(
    FreshMarketDbContext context,
    ILogger<OrderRepository> logger,
    IUserContext userContext)
    : Repository<Order>(context, logger), IOrder
{
    private readonly FreshMarketDbContext _context = context;
    private readonly IUserContext _userContext = userContext;

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(long? userId, CancellationToken ct = default)
    {
        if (!userId.HasValue || userId <= 0) return Array.Empty<Order>();

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .Include(o => o.BillingAddress)
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderStatus)
                .Include(o => o.PaymentStatus)
                .Include(o => o.Coupon)
                .OrderByDescending(o => o.PlacedAt)
                .ToListAsync(ct),
            logger,
            "Get Orders by UserId",
            new { UserId = userId }
        );
    }

    public async Task<IReadOnlyList<Order>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
    {
        Guard.AgainstEmpty(sessionId, nameof(sessionId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderItems.Any(oi => oi.SessionId == sessionId))
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .Include(o => o.BillingAddress)
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderStatus)
                .Include(o => o.PaymentStatus)
                .Include(o => o.Coupon)
                .OrderByDescending(o => o.PlacedAt)
                .ToListAsync(ct),
            logger,
            "Get Orders by SessionId",
            new { SessionId = sessionId }
        );
    }

    public async Task<Order?> GetWithDetailsAsync(long orderId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(orderId, nameof(orderId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ThenInclude(p => p.ProductMedia)
                .Include(o => o.BillingAddress)
                .Include(o => o.ShippingAddress)
                .Include(o => o.Coupon)
                .Include(o => o.PaymentTransactions)
                .Include(o => o.OrderStatus)
                .Include(o => o.PaymentStatus)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == orderId, ct),
            logger,
            "Get Order with Full Details",
            new { OrderId = orderId }
        );
    }

    public async Task<Order?> GetWithDetailsBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
    {
        Guard.AgainstEmpty(sessionId, nameof(sessionId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ThenInclude(p => p.ProductMedia)
                .Include(o => o.BillingAddress)
                .Include(o => o.ShippingAddress)
                .Include(o => o.Coupon)
                .Include(o => o.PaymentTransactions)
                .Include(o => o.OrderStatus)
                .Include(o => o.PaymentStatus)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderItems.Any(oi => oi.SessionId == sessionId), ct),
            logger,
            "Get Order with Details by SessionId",
            new { SessionId = sessionId }
        );
    }

    public async Task<IReadOnlyList<Order>> GetPendingPaymentAsync(CancellationToken ct = default)
    {
        return await ExecutionHelper.ExecuteAsync(
            () => _context.Orders
                .AsNoTracking()
                .Where(o => o.PaymentStatusId == (int)PaymentStatus.Pending)
                .Include(o => o.User)
                .Include(o => o.PaymentStatus)
                .OrderBy(o => o.PlacedAt)
                .ToListAsync(ct),
            logger,
            "Get Orders with Pending Payment"
        );
    }

    public async Task<IReadOnlyList<Order>> GetByStatusAsync(int orderStatusId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(orderStatusId, nameof(orderStatusId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderStatusId == orderStatusId)
                .Include(o => o.OrderStatus)
                .Include(o => o.User)
                .OrderByDescending(o => o.PlacedAt)
                .ToListAsync(ct),
            logger,
            "Get Orders by Status",
            new { OrderStatusId = orderStatusId }
        );
    }

    public async Task<IPaginatedList<Order>> GetPaginatedByUserIdAsync(long? userId, PaginationRequest request, CancellationToken ct = default)
    {
        if (!userId.HasValue || userId <= 0) return new PaginatedList<Order>([], request.PageNumber, request.PageSize, 0);

        var query = _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId);

        return await BuildPaginatedQuery(query, request, ct);
    }

    public async Task<IPaginatedList<Order>> GetPaginatedBySessionIdAsync(Guid sessionId, PaginationRequest request, CancellationToken ct = default)
    {
        Guard.AgainstEmpty(sessionId, nameof(sessionId));

        var query = _context.Orders
            .AsNoTracking()
            .Where(o => o.OrderItems.Any(oi => oi.SessionId == sessionId));

        return await BuildPaginatedQuery(query, request, ct);
    }

    private async Task<IPaginatedList<Order>> BuildPaginatedQuery(IQueryable<Order> query, PaginationRequest request, CancellationToken ct)
    {
        // Search
        if (!string.IsNullOrWhiteSpace(request.SearchValue))
        {
            var search = request.SearchValue.Trim();
            query = query.Where(o =>
                EF.Functions.Like(o.OrderNumber, $"%{search}%") ||
                EF.Functions.Like(o.ShippingTrackingNumber ?? "", $"%{search}%") ||
                EF.Functions.Like(o.CouponCode ?? "", $"%{search}%") ||
                EF.Functions.Like(o.BillingAddress!.Phone ?? "", $"%{search}%")
            );
        }

        // Sort
        query = (request.SortBy?.Trim().ToLower()) switch
        {
            "orderdate" or "placedat" => request.SortDirection == SortDirection.Ascending
                ? query.OrderBy(o => o.PlacedAt)
                : query.OrderByDescending(o => o.PlacedAt),

            "total" or "grandtotal" or "totalamount" => request.SortDirection == SortDirection.Ascending
                ? query.OrderBy(o => o.GrandTotal)
                : query.OrderByDescending(o => o.GrandTotal),

            "status" => request.SortDirection == SortDirection.Ascending
                ? query.OrderBy(o => o.OrderStatus.NameEn)
                : query.OrderByDescending(o => o.OrderStatus.NameEn),

            "ordernumber" => request.SortDirection == SortDirection.Ascending
                ? query.OrderBy(o => o.OrderNumber)
                : query.OrderByDescending(o => o.OrderNumber),

            _ => query.OrderByDescending(o => o.PlacedAt)
        };

        return await ExecutionHelper.ExecuteAsync(
            () => query.ToPaginatedListAsync(request, ct),
            logger,
            "Get Paginated Orders",
            new { Request = request }
        );
    }
}
