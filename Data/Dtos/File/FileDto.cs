namespace Identity.API.Data.Dtos.File;

public record FileDto
{
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public byte[] FileContent { get; set; }
}