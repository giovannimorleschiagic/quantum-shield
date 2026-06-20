using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using QuantumShield.Be.Business;
using QuantumShield.Be.Domain.Options;
using QuantumShield.Be.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddControllers();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://login.microsoftonline.com/{authenticationOptions.TenantId}/v2.0";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = authenticationOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var tenantId = context.Principal?.FindFirst("tid")?.Value;
                if (!string.Equals(tenantId, authenticationOptions.TenantId, StringComparison.OrdinalIgnoreCase))
                {
                    context.Fail("The bearer token was issued for a different tenant.");
                }

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

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program;
