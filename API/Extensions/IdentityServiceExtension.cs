using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions
{
    public static class IdentityServiceExtension
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = GetIssuerSigningKey(),
                    ValidateLifetime = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidAudience = Convert.ToBase64String(Encoding.UTF8.GetBytes("AmplifyC Client")).ToString(),
                    ValidIssuer = Convert.ToBase64String(Encoding.UTF8.GetBytes("AmplifyC Authentication API")).ToString()
                };
            });

            return services;

        }

        private static X509SecurityKey GetIssuerSigningKey()
        {
            var publicCert = new X509Certificate2("jwt_rsa_pub.cer");
            Console.WriteLine(publicCert);
            return new X509SecurityKey(publicCert);
        }

    }
}