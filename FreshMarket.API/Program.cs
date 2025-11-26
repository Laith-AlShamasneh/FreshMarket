using FluentValidation.AspNetCore;
using FreshMarket.API.Middleware;
using FreshMarket.Infrastructure;
using FreshMarket.Shared.Common;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            // 1. Get all errors from ModelState
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.First().ErrorMessage
                );

            // 2. Create your custom response
            var apiResponse = ApiResponse<object>.Fail(
                ErrorCodes.Validation.INVALID_INPUT,
                MessageType.InvalidInput,
                Lang.En,
                HttpResponseStatus.BadRequest,
                errors
            );

            return new BadRequestObjectResult(apiResponse);
        };
    });

// 1. Register Layers
builder.Services.AddInfrastructure(builder.Configuration);
//builder.Services.AddApplication();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
