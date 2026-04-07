namespace HouseHoldButler.Requests.Auth
{
    public record ResetPasswordRequest(string Email, string ResetCode, string NewPassword);

}