using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using SiteSubscriptionServer.HubConfig;
using SiteSubscriptionServer.Interface;
using SiteSubscriptionServer.SignalRMiddleware;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SiteSubscriptionServer
{
    public class Startup
    {
        private readonly SymmetricSecurityKey SecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("signalr@app")/*Guid.NewGuid().ToByteArray()*/);
        private readonly JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder => builder
                    .WithOrigins("http://localhost:4200"/*"http://gensolve-signalr-proto.s3-website.ap-south-1.amazonaws.com/"*/)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
            //    {
            //        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
            //        policy.RequireClaim(ClaimTypes.NameIdentifier);
            //    });
            //});

            services.AddSingleton<ISiteConnectionManager, SiteConnectionManager>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = SecurityKey,
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                x.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;
                        //string origin = context.Request.Headers["Origin"];
                        //context.Response.Headers.Add("Access-Control-Allow-Origin", origin);
                        if (!string.IsNullOrEmpty(accessToken) &&
                                (context.HttpContext.WebSockets.IsWebSocketRequest || context.Request.Headers["Accept"] == "text/event-stream") &&
                                path.StartsWithSegments("/hubs/site"))
                        {
                            context.Token = context.Request.Query["access_token"];
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddJwtBearer(options =>
            //    {
            //        options.TokenValidationParameters =
            //        new TokenValidationParameters
            //        {
            //            LifetimeValidator = (before, expires, token, parameters) => expires > DateTime.UtcNow,
            //            ValidateAudience = false,
            //            ValidateIssuer = false,
            //            ValidateActor = false,
            //            ValidateLifetime = true,
            //            IssuerSigningKey = SecurityKey
            //        };

            //        options.Events = new JwtBearerEvents
            //        {
            //            OnMessageReceived = context =>
            //            {
            //                var accessToken = context.Request.Query["access_token"];

            //                var path = context.HttpContext.Request.Path;
            //                if (!string.IsNullOrEmpty(accessToken) &&
            //                    (context.HttpContext.WebSockets.IsWebSocketRequest || context.Request.Headers["Accept"] == "text/event-stream") &&
            //                    path.StartsWithSegments("/hubs/site"))
            //                {
            //                    context.Token = context.Request.Query["access_token"];
            //                }
            //                return Task.CompletedTask;
            //            },
            //            OnAuthenticationFailed = context =>
            //            {
            //                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            //                {
            //                    context.Response.Headers.Add("Token-Expired", "true");
            //                }
            //                return Task.CompletedTask;
            //            }
            //        };
            //    });
            // services.AddAuthorization(options => { });
            services.AddSignalR();

            services.AddHostedService<Worker>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //app.UseCors(builder =>
            //{
            //    builder.WithOrigins("http://gensolve-signalr-proto.s3-website.ap-south-1.amazonaws.com/")
            //    .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            //});
            app.UseCors("CorsPolicy");
            app.UseRouting();
            
            app.UseAuthentication();
            //app.UseMiddleware<WebSocketsMiddleware>();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<SiteHub>("/hubs/site");
            });
        }
    }
}
