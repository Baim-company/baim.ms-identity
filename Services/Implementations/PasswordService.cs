using Identity.API.Services.Abstractions;
using System.Text;

namespace Identity.API.Services.Implementations;

public class PasswordService : IPasswordService
{
    public string GenerateRandomPassword()
    {
        Random random = new Random();

        string upperCaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string lowerCaseChars = "abcdefghijklmnopqrstuvwxyz";
        string digits = "0123456789";
        string specialChars = "!@#$%^&*()-_=+";

        StringBuilder temporaryPassword = new StringBuilder();
        temporaryPassword.Append(upperCaseChars[random.Next(upperCaseChars.Length)]);
        temporaryPassword.Append(lowerCaseChars[random.Next(lowerCaseChars.Length)]);
        temporaryPassword.Append(digits[random.Next(digits.Length)]);
        temporaryPassword.Append(specialChars[random.Next(specialChars.Length)]);

        string allValidChars = upperCaseChars + lowerCaseChars + digits + specialChars;
        for (int i = temporaryPassword.Length; i < 10; i++)
        {
            temporaryPassword.Append(allValidChars[random.Next(allValidChars.Length)]);
        }

        return ShufflePassword(temporaryPassword.ToString(), random);
    }

    private string ShufflePassword(string password, Random random)
    {
        char[] array = password.ToCharArray();
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
        return new string(array);
    }
}
