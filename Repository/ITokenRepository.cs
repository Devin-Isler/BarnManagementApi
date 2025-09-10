using Microsoft.AspNetCore.Identity;

namespace BarnManagementApi.Repository
{
    public interface ITokenRepository
    {
        string CreateJWTToken(IdentityUser user, List<String> roles);
    }
}
