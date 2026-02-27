namespace BackendApi.Models
{
    public record ResetPasswordRequest(string Email, string ResetCode, string NewPassword);

}