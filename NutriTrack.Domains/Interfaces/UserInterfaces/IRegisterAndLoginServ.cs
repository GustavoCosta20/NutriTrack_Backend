using NutriTrack_Domains.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriTrack_Domains.Interfaces.UserInterfaces
{
    public interface IRegisterAndLoginServ
    {
        Task RegisterUser(RegisterUserDto info);
    }
}
