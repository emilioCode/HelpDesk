using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
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
                ExpiresMinutes = int.TryParse(configuration["Jwt:ExpiresMinutes"], out var minutes) && minutes > 0 ? minutes : AuthOptions.DefaultExpiresMinutes,
                // Safe by default: requests need a token unless Auth:Enforce is explicitly set to false.
                ActiveCheckSeconds = int.TryParse(configuration["Jwt:ActiveCheckSeconds"], out var seconds) && seconds >= 0 ? seconds : AuthOptions.DefaultActiveCheckSeconds,
                Enforce = !string.Equals(configuration["Auth:Enforce"], "false", StringComparison.OrdinalIgnoreCase)
            };

            if (string.IsNullOrEmpty(options.Key) || options.Key.Length < AuthOptions.MinimumKeyLength)
            {
                throw new InvalidOperationException(
                    $"Jwt:Key must be set to at least {AuthOptions.MinimumKeyLength} characters (environment variable Jwt__Key).");
            }

            services.AddSingleton(Microsoft.Extensions.Options.Options.Create(options));
            services.AddSingleton<ITokenService, JwtTokenService>();
            services.AddHttpContextAccessor();
            services.AddMemoryCache();
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
                        OnTokenValidated = async context =>
                        {
                            // A token stays valid until it expires, so what it says is checked against the user now:
                            // a user disabled meanwhile, or whose role or company changed, must sign in again.
                            if (!int.TryParse(context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId))
                            {
                                context.Fail("The token has no user.");
                                return;
                            }
                            var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
                            var key = "active-user:" + userId;
                            if (!cache.TryGetValue(key, out UserIdentity current))
                            {
                                try
                                {
                                    var users = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                                    current = await users.GetActiveIdentityAsync(userId);
                                }
                                catch
                                {
                                    // If the user cannot be checked, the request is refused rather than let through.
                                    current = null;
                                }
                                if (options.ActiveCheckSeconds > 0)
                                {
                                    cache.Set(key, current, TimeSpan.FromSeconds(options.ActiveCheckSeconds));
                                }
                            }
                            if (current == null)
                            {
                                context.Fail("The user is no longer active.");
                                return;
                            }
                            var role = context.Principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                            var company = context.Principal.FindFirst(JwtTokenService.CompanyClaim)?.Value;
                            if (role != (current.Acceso ?? string.Empty) || company != current.IdEmpresa.ToString())
                            {
                                context.Fail("The role or the company of the user changed.");
                            }
                        },
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
