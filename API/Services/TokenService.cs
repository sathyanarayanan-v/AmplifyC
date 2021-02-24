using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace API.Services
{
    public class TokenService : ITokenService
    {

        public TokenService(IConfiguration config)
        {
        }
        public string signToken(AppUser user)
        {
            string issued = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            string expire = DateTimeOffset.Now.AddHours(10).ToUnixTimeSeconds().ToString();
            string privateKeyPem = File.ReadAllText("jwt_rsa_pk.pem");
            var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, Convert.ToBase64String(Encoding.UTF8.GetBytes(user.Id.ToString()+"_"+user.UserName))),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat,issued),
                    new Claim(JwtRegisteredClaimNames.Exp,expire),
                    new Claim(JwtRegisteredClaimNames.Iss,Convert.ToBase64String(Encoding.UTF8.GetBytes("AmplifyC Authentication API"))),
                    new Claim(JwtRegisteredClaimNames.Aud,Convert.ToBase64String(Encoding.UTF8.GetBytes("AmplifyC Client")))
                };
            return CreateToken(claims, privateKeyPem);
        }

        public static string CreateToken(System.Security.Claims.Claim[] claims, string privateRsaKey)
        {
            RSAParameters rsaParams;
            using (var tr = new StringReader(privateRsaKey))
            {
                var pemReader = new PemReader(tr);
                var keyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;
                if (keyPair == null)
                {
                    throw new Exception("Could not read RSA private key");
                }
                var privateRsaParams = keyPair.Private as RsaPrivateCrtKeyParameters;
                rsaParams = DotNetUtilities.ToRSAParameters(privateRsaParams);
            }
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(rsaParams);

                Dictionary<string, object> payload = claims.ToDictionary(k => k.Type, v => (object)v.Value);

                return Jose.JWT.Encode(payload, rsa, Jose.JwsAlgorithm.RS256);
            }
        }
    }
}