namespace Identity.API.Data.Dtos.Responses;

public record ResponseDto<T>
{
    public string Message { get; set; } = "Error!";
    public T? Data { get; set; } 

    public ResponseDto() { }
    public ResponseDto(string message)
    {
        Message = message;
    }
    public ResponseDto(string message, T? data)
    {
        Message = message;
        Data = data;
    }
}