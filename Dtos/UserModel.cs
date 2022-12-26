namespace WebApiTraductorPMV.Dtos;

public class UserModel
{
    public int UserId { get; set; }
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public List<string>? Roles { get; set; }

}
