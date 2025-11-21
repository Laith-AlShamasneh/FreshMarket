using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.FreshMarketManagement;

public class ProductVariantRepository(
    FreshMarketDbContext context,
    ILogger<ProductVariantRepository> logger)
    : Repository<ProductVariant>(context, logger), IProductVariant
{
}