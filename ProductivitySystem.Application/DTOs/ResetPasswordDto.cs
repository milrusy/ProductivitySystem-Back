namespace ProductivitySystem.Application.DTOs;

public class ResetPasswordDto
{
    public int UserId { get; set; }

    public string NewPassword { get; set; }
}
