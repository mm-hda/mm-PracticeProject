namespace backend.Dto;

public class TokenDto
{
    public string? Token { get; set; }

    public string? Message { get; set; }

    public Guid UserId { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Role { get; set; }
}
