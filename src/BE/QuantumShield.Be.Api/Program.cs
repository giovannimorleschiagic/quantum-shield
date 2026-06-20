using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using QuantumShield.Be.Business;
using QuantumShield.Be.Domain.Options;
using QuantumShield.Be.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
const string AllowAllCorsPolicy = "AllowAll";

builder.Services
    .AddOptions<BearerAuthenticationOptions>()
    .Bind(builder.Configuration.GetSection(BearerAuthenticationOptions.SectionName))
    .Validate(static options =>
            !string.IsNullOrWhiteSpace(options.TenantId)
            && !string.IsNullOrWhiteSpace(options.Audience),
        "Authentication configuration is incomplete.")
    .ValidateOnStart();

var authenticationOptions = builder.Configuration.GetSection(BearerAuthenticationOptions.SectionName).Get<BearerAuthenticationOptions>()
    ?? throw new InvalidOperationException("Authentication configuration is missing.");

builder.Services.AddOpenApi();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowAllCorsPolicy, policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://login.microsoftonline.com/{authenticationOptions.TenantId}/v2.0";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = authenticationOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Authentication.JwtBearer");

                var hasAuthorizationHeader = context.Request.Headers.ContainsKey("Authorization");
                logger.LogInformation(
                    "JWT message received for {Method} {Path}. Authorization header present: {HasAuthorizationHeader}",
                    context.Request.Method,
                    context.Request.Path,
                    hasAuthorizationHeader);

                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Authentication.JwtBearer");

                var tenantId = context.Principal?.FindFirst("tid")?.Value;
                var audience = context.Principal?.FindFirst("aud")?.Value;
                var issuer = context.Principal?.FindFirst("iss")?.Value;

                logger.LogInformation(
                    "JWT token validated. Issuer: {Issuer}. Audience: {Audience}. TenantId: {TenantId}",
                    issuer,
                    audience,
                    tenantId);

                if (!string.Equals(tenantId, authenticationOptions.TenantId, StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogWarning(
                        "JWT token rejected because tenant id {ActualTenantId} does not match configured tenant {ExpectedTenantId}.",
                        tenantId,
                        authenticationOptions.TenantId);
                    context.Fail("The bearer token was issued for a different tenant.");
                }

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Authentication.JwtBearer");

                logger.LogError(
                    context.Exception,
                    "JWT authentication failed for {Method} {Path}.",
                    context.Request.Method,
                    context.Request.Path);

                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Authentication.JwtBearer");

                logger.LogWarning(
                    "JWT challenge triggered for {Method} {Path}. Error: {Error}. Description: {ErrorDescription}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Error,
                    context.ErrorDescription);

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());
builder.Services.AddBusiness();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(AllowAllCorsPolicy);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program;
