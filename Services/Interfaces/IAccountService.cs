using WebApiTraductorPMV.Dtos;

namespace WebApiTraductorPMV.Services;

public interface IAccountService
{
    UserTokenDTO? Login(UserLogin model);
}