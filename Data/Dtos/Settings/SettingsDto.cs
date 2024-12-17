using Identity.API.Data.Enums;
using System.Text.Json.Serialization;

namespace Identity.API.Data.Dtos.Settings;

public record SettingsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Patronymic { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; } = DateTime.UtcNow;
    public string Position { get; set; } = string.Empty;


    public string Email { get; set; }
    public string PersonalEmail { get; set; } = string.Empty;


    public string PhoneNumber { get; set; } = string.Empty;
    public string BusinessPhoneNumber { get; set; } = string.Empty;


    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Gender Gender { get; set; } = Gender.Man;
}