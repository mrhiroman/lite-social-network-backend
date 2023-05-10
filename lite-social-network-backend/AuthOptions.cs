using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace lite_social_network_backend;

public static class AuthOptions
{
    public const string ISSUER = "lite-socNetwork"; 
    public const string AUDIENCE = "lite-socNetworkClient"; 
    const string KEY = "key_for_encryption_13kkevkegkler";   // get from environment
    public const int LIFETIME = 15; 
    public static SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
    }
}