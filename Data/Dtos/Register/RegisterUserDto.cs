namespace Identity.API.Data.Dtos.Register;

public record RegisterUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "UserAdmin";
    public Guid CompanyId { get; set; }
    public RegisterUserDto()
    {
        CompanyId = Guid.Empty;
    }
}