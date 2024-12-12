using Microsoft.AspNetCore.Identity;

namespace Identity.API.Data.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public string Description { get; set; }
}