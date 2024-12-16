using Identity.API.Data.Dtos.Register;
using Identity.API.Data.Enums;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Data.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string Name { get; set; }
    public string? Surname { get; set; } 
    public string? Patronymic { get; set; }

    public DateTime BirthDate { get; set; } = DateTime.UtcNow;
    public Gender Gender { get; set; } = Gender.Man;
    public string? Position { get; set; }


    public string? PersonalEmail { get; set; }
    public string? BusinessPhoneNumber { get; set; }


    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; } = DateTime.UtcNow;


    public string AvatarPath { get; set; } = "user-icon.png";


    public ApplicationUser()
    {
        Id = Guid.NewGuid();
        Name = "";
    }
    public ApplicationUser(RegisterUserDto userDto, bool isOldClient)
    {
        Id = Guid.NewGuid();
        Name = userDto.Name;
        Surname = userDto.Surname;
        Email = userDto.Email;
        UserName = userDto.Email;
        RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);
    }
}