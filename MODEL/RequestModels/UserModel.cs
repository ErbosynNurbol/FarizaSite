namespace MODEL.RequestModels;

public class UserModel
{
    public string AndroidToken { get; set; }
    public string IOSToken { get; set; }
    public string Phone { get; set; }
    public string Type { get; set; }
    public string VerifyCode { get; set; }
    public string UserName { get; set; }
    public string SurName { get; set; }
    public string OldPassword { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}
