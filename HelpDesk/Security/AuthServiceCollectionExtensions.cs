using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace HelpDesk.Security
{
    public static class AuthServiceCollectionExtensions
    {
        public static IServiceCollection AddHelpDeskAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var options = new AuthOptions
            {
                Key = configuration["Jwt:Key"],
                Issuer = configuration["Jwt:Issuer"] ?? "HelpDesk",
                Audience = configuration["Jwt:Audience"] ?? "HelpDesk",
                ExpiresMinutes = int.TryParse(configuration["Jwt:ExpiresMinutes"], out var minutes) && minutes > 0 ? minutes : 480,
                Enforce = string.Equals(configuration["Auth:Enforce"], "true", StringComparison.OrdinalIgnoreCase)
            };

            if (string.IsNullOrEmpty(options.Key) || options.Key.Length < AuthOptions.MinimumKeyLength)
            {
                throw new InvalidOperationException(
                    $"Jwt:Key must be set to at least {AuthOptions.MinimumKeyLength} characters (environment variable Jwt__Key).");
            }

            services.AddSingleton(Microsoft.Extensions.Options.Options.Create(options));
            services.AddSingleton<ITokenService, JwtTokenService>();
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, CurrentUser>();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(bearer =>
                {
                    bearer.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = options.Issuer,
                        ValidateAudience = true,
                        ValidAudience = options.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
                    bearer.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            if (!string.IsNullOrEmpty(context.Token))
                            {
                                return System.Threading.Tasks.Task.CompletedTask;
                            }
                            var path = context.HttpContext.Request.Path.Value ?? string.Empty;
                            if (path.StartsWith("/hub", StringComparison.OrdinalIgnoreCase))
                            {
                                // SignalR cannot send headers on every transport, so it sends the token in the query string.
                                context.Token = context.Request.Query["access_token"];
                            }
                            else if (path.StartsWith("/OrderReport", StringComparison.OrdinalIgnoreCase))
                            {
                                // Printable reports open in a new tab by a plain link, which cannot carry a header.
                                context.Token = context.Request.Cookies[AuthOptions.ReportCookie];
                            }
                            return System.Threading.Tasks.Task.CompletedTask;
                        }
                    };
                });

            services.AddSingleton<IAuthorizationHandler, EnforceAuthenticationHandler>();
            services.AddAuthorization(authorization =>
            {
                authorization.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            return services;
        }
    }
}
