namespace BackendApi.Requests.Auth
{
    public record ResetPasswordRequest(string Email, string ResetCode, string NewPassword);

}