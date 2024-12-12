using System.Text.RegularExpressions;


namespace Identity.API.Services.Validations;

public static class AuthValidation
{
    public static bool CheckEmail(string email)
    {
        var regex = Regex.IsMatch(email, "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$");
        return regex == true ? true : false;
    }
    public static bool CheckPassword(string password)
    {
        var regex = Regex.IsMatch(password, "^(?=.*[A-Z])(?=.*\\d)(?=.*[@#$%^&+=-_'.!]).{6,40}$");
        return regex == true ? true : false;
    }
}