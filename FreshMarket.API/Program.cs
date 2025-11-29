using FluentValidation.AspNetCore;
using FreshMarket.API.Middleware;
using FreshMarket.Application;
using FreshMarket.Shared.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

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

                return new OkObjectResult(apiResponse);
            };
        });

    builder.Services.AddApplication(builder.Configuration);
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();

    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("Jwt:Secret is missing.");
    var key = Encoding.UTF8.GetBytes(secretKey);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IUserContext, HttpUserContext>();

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "FreshMarket API v1");
        });
    }

    app.UseStaticFiles();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseMiddleware<UserContextMiddleware>();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
