using Identity.API.Data.Enums;
using System.Text.Json.Serialization;

namespace Identity.API.Data.Dtos.Settings;

public record SettingsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Surname { get; set; } = "";
    public string Patronymic { get; set; } = "";
    public DateTime BirthDate { get; set; } = DateTime.UtcNow;
    public string Position { get; set; } = string.Empty;


    public string Email { get; set; }
    public string PersonalEmail { get; set; } = "";


    public string PhoneNumber { get; set; } = "";
    public string BusinessPhoneNumber { get; set; } = "";


    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Gender Gender { get; set; } = Gender.Man;
}