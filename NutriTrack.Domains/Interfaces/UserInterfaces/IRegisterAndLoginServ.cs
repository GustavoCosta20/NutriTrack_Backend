using NutriTrack_Domains.Dtos;

namespace NutriTrack_Domains.Interfaces.UserInterfaces
{
    public interface IRegisterAndLoginServ
    {
        Task RegisterUser(RegisterUserDto info);

        Task<string> LoginUser(UserDataLoginDto info);

    }
}
