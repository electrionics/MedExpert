using MedExpert.Domain.Identity;

namespace MedExpert.Web.Services
{
    public interface IJwtGenerator
    {
        string CreateToken(User user);
    }
}