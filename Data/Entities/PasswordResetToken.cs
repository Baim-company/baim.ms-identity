using System.Text.Json.Serialization;

namespace Identity.API.Data.Entities;

public class PasswordResetToken
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public DateTime ExpiryDate { get; set; }

    public Guid UserId { get; set; }
    [JsonIgnore]
    public virtual ApplicationUser User { get; set; }
}