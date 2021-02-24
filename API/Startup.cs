using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace API
{
    public class Startup
    {
        private readonly IConfiguration _config;
        public Startup(IConfiguration config)
        {
            _config = config;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite(_config.GetConnectionString("DefaultConnection"));
            });
            services.AddScoped<ITokenService, TokenService>();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }


        // public static string DecodeToken(string token, string publicRsaKey)
        // {
        //     RSAParameters rsaParams;

        //     using (var tr = new StringReader(publicRsaKey))
        //     {
        //         var pemReader = new PemReader(tr);
        //         var publicKeyParams = pemReader.ReadObject() as RsaKeyParameters;
        //         if (publicKeyParams == null)
        //         {
        //             throw new Exception("Could not read RSA public key");
        //         }
        //         rsaParams = DotNetUtilities.ToRSAParameters(publicKeyParams);
        //     }
        //     using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
        //     {
        //         rsa.ImportParameters(rsaParams);
        //         // This will throw if the signature is invalid
        //         return Jose.JWT.Decode(token, rsa, Jose.JwsAlgorithm.RS256);
        //     }
        // }

        private X509SecurityKey GetIssuerSigningKey()
        {
            var publicCert = new X509Certificate2("jwt_rsa_pub.cer");
            Console.WriteLine(publicCert);
            return new X509SecurityKey(publicCert);
        }

    }
}
