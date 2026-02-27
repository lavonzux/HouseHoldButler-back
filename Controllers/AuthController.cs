using BackendApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using System.Security.Claims;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IEmailSender emailSender,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = string.IsNullOrEmpty(request.Phone) ? null : request.Phone
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        // 將「姓名」儲存為標準 Claim（後續可透過 User.Claims 或自訂 /manage/info 取得）
        await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, request.Name));

        // 自動簽入使用者（建立驗證 Cookie）
        await _signInManager.SignInAsync(user, isPersistent: false);

        return Ok();
    }

    [HttpPost("forgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        _logger.LogInformation("進入自訂 /api/auth/forgotPassword 端點 - 請求 email: {Email}", request.Email);

        var user = await _userManager.FindByEmailAsync(request.Email);

        _logger.LogInformation(
            "查詢 email {Email} 的使用者：{UserExists}, Email 已確認：{IsConfirmed}",
            request.Email,
            user != null ? "存在" : "不存在",
            user != null ? (await _userManager.IsEmailConfirmedAsync(user)).ToString() : "N/A"
        );

        if (user == null)
        {
            // 安全考量：不透露帳號是否存在
            return Ok();
        }

        var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
        var resetCode = token; // 未來可改成短碼 + 儲存

        var html = $@"    
        <h3>重設碼</h3>
        <h4>{resetCode}</h4>
        <h3>請輸入此碼重設密碼</h3>
        ";

        try
        {
            await _emailSender.SendEmailAsync(
                request.Email,
                "重設您的 AI 智慧家庭管家 密碼",
                html);

            _logger.LogInformation("重設密碼郵件已嘗試寄送至 {Email}", request.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "寄送重設密碼郵件失敗 - email: {Email}", request.Email);
            // 仍回 200 較友善，或依需求回 500
        }

        return Ok();
    }

    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return BadRequest(new { message = "重設失敗，請確認資料" });
        }

        // 使用 VerifyTwoFactorTokenAsync 驗證剛才產生的 6 位數碼
        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", request.ResetCode);

        if (!isValid)
        {
            _logger.LogWarning("無效的驗證碼 - email: {Email}", request.Email);
            return BadRequest(new { message = "驗證碼無效或已過期" });
        }

        // 驗證通過 → 直接重設密碼
        var remove = await _userManager.RemovePasswordAsync(user);
        if (!remove.Succeeded)
        {
            return BadRequest(remove.Errors);
        }

        var add = await _userManager.AddPasswordAsync(user, request.NewPassword);
        if (!add.Succeeded)
        {
            return BadRequest(add.Errors);
        }

        _logger.LogInformation("密碼重設成功 - {Email}", request.Email);

        return Ok();
    }
}