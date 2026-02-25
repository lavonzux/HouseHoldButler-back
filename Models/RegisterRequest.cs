// 自訂註冊端點：支援所有欄位、自動登入、姓名儲存為 Claim
namespace BackendApi.Models
{
    public record RegisterRequest(string Email, string Password, string Name, string Phone);

}
