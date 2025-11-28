using FluentValidation.AspNetCore;
using FreshMarket.API.Middleware;
using FreshMarket.Shared.Common;
using Microsoft.AspNetCore.Mvc;
using FreshMarket.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var userContext = context.HttpContext.RequestServices.GetRequiredService<IUserContext>();
            var lang = userContext.Lang;

            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.First().ErrorMessage
                );

            var apiResponse = ApiResponse<object>.Fail(
                ErrorCodes.Validation.INVALID_INPUT,
                Messages.Get(MessageType.InvalidInput, lang),
                HttpResponseStatus.BadRequest,
                errors
            );

            // 4. Return HTTP 400 Bad Request with the custom payload
            return new BadRequestObjectResult(apiResponse);
        };
    });

// 1. Register Layers
builder.Services.AddApplication(builder.Configuration);

// 2. Register FluentValidation with ASP.NET Core
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, HttpUserContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<UserContextMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseStaticFiles();

app.Run();
