namespace Identity.API.Data.Dtos.ExternalUsers;

public record ExternalUserCompanyDto
{
    public ExternalUserDto User { get; set; }
    public Guid CompanyId { get; set; }

    public ExternalUserCompanyDto()
    {
        CompanyId = Guid.NewGuid();
        User = new ExternalUserDto();
    }
    public ExternalUserCompanyDto(ExternalUserDto user, Guid companyId)
    {
        User = user;
        CompanyId = companyId;
    }
}