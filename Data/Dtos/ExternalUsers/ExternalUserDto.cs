using Identity.API.Data.Dtos.Settings;
using Identity.API.Data.Entities;
using Identity.API.Data.Enums;

namespace Identity.API.Data.Dtos.ExternalUsers;

public class ExternalUserDto
{
    public Guid Id { get; set; }
    public string? Id1C { get; set; }
    public string? Position { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Patronymic { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Role { get; set; } = "User";


    public string Email { get; set; } = string.Empty;
    public string PersonalEmail { get; set; } = string.Empty;


    public string PhoneNumber { get; set; } = string.Empty;
    public string BusinessPhoneNumber { get; set; } = string.Empty;


    public Gender Gender { get; set; } = Gender.Man;
    public string AvatarPath { get; set; } = "";

    public ExternalUserDto() { }
    public ExternalUserDto(SettingsDto settingDto, string role)
    {
        Id = settingDto.Id;
        Name = settingDto.Name;
        Surname = settingDto.Surname;
        Patronymic = settingDto.Patronymic;

        BirthDate = settingDto.BirthDate;
        Position = settingDto.Position;


        Email = settingDto.Email!;
        PersonalEmail = settingDto.PersonalEmail;

        PhoneNumber = settingDto.PhoneNumber;
        BusinessPhoneNumber = settingDto.BusinessPhoneNumber;


        Gender = settingDto.Gender;
        Role = role;
    }
    public ExternalUserDto(ApplicationUser applicationUser, string role)
    {
        Id = applicationUser.Id;
        Id1C = applicationUser.Id1C ?? "";
        Name = applicationUser.Name;
        Surname = applicationUser.Surname ?? "";
        Patronymic = applicationUser.Patronymic ?? "";

        BirthDate = applicationUser.BirthDate;
        Position = applicationUser.Position ?? "";


        Email = applicationUser.Email!;
        PersonalEmail = applicationUser.PersonalEmail ?? "";

        PhoneNumber = applicationUser.PhoneNumber ?? "";
        BusinessPhoneNumber = applicationUser.BusinessPhoneNumber ?? "";


        Gender = applicationUser.Gender;
        Role = role;
        AvatarPath = applicationUser.AvatarPath;
    }
}