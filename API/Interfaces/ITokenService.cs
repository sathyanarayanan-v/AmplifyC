using API.Entities;

namespace API.Interfaces
{
    public interface ITokenService
    {
        string signToken(AppUser user);
    }
}