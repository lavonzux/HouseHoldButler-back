using BackendApi.Models;
using BackendendApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// 設定 EF Core 使用 In-Memory Database，並指定資料庫名稱為 "ApplicationDb"
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseInMemoryDatabase("ApplicationDb"));

// 啟動 Identity API Endpoints（內部已包含 cookie + bearer 支援）
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders(); // 這一行確保 Email / Phone Token Provider 被註冊

// 加入郵件發送服務
builder.Services.AddTransient<IEmailSender, EmailSender>();  // 您實作的 EmailSender

// 明確設定預設方案為 cookie（Identity.Application），避免框架 fallback 到 Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
});

// 註冊授權服務
builder.Services.AddAuthorization();

// 註冊控制器所需的各項核心服務
builder.Services.AddControllers();

// 註冊 OpenAPI 生成器，來支援 Minimal APIs 自動生成對應的 OpenAPI 文件，方便開發者使用 Swagger UI 來測試 API
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// 註冊 Rate Limiter 並新增一個名為 "Default" 的 policy
builder.Services.AddRateLimiter(options =>
{
    // policy：以 Remote IP 為分割，固定視窗限制
    options.AddPolicy("Default", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            // partition key: remote ip 或 fallback 為 "global"
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "global",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5, // 每個視窗允許的請求數
                Window = TimeSpan.FromMinutes(1), // 視窗大小
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// Configure Cookie 自訂 ASP.NET Core Cookie 認證中介軟體的行為
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);

    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
});

// Configure CORS，加入 CORS 支援，允許來自任何來源的請求（開發階段使用，生產環境請把 WithOrigins 改成你正式的前端域名，並移除 AllowAnyHeader / AllowAnyMethod 的寬鬆設定。）
const string myAllowSpecificOrigins = "_AllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(myAllowSpecificOrigins, policyBuilder =>
    {
        policyBuilder.WithOrigins("http://localhost:3000",
            "https://localhost:3000") // ← 你的 React 開發伺服器位址
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); // 允許攜帶 Cookie（重要！）
    });
});

// Configure the HTTP request pipeline. 專案啟動時根路徑為 localhost:7066
// Swagger UI 的預設路徑為 /swagger/index.html
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    // 完全使用 .NET 內建 OpenAPI（無需 Swashbuckle）啟用 Swagger UI
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

// 啟用 CORS，允許 React 開發伺服器的請求
app.UseCors(myAllowSpecificOrigins);

// 啟用 Rate Limiter 中介軟體（需要啟用才能讓 .RequireRateLimiting("Default") 生效）
app.UseRateLimiter();

// 啟用 HTTPS 重定向，確保 API 只能透過 HTTPS 存取（生產環境建議啟用）
//app.UseHttpsRedirection();

// 先認證
app.UseAuthentication();

// 再授權
app.UseAuthorization();

// 自訂義註冊端點
app.MapPost("/api/register", async (RegisterRequest request,
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager) =>
{
    var user = new IdentityUser
    {
        UserName = request.Email,
        Email = request.Email,
        PhoneNumber = string.IsNullOrEmpty(request.Phone) ? null : request.Phone
    };

    var result = await userManager.CreateAsync(user, request.Password);
    if (!result.Succeeded)
    {
        return Results.BadRequest(new { errors = result.Errors });
    }

    // 將「姓名」儲存為標準 Claim（後續可透過 User.Claims 或自訂 /manage/info 取得）
    await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, request.Name));

    // 自動簽入使用者（建立驗證 Cookie）
    await signInManager.SignInAsync(user, isPersistent: false);

    return Results.Ok();
});

// 自訂義忘記密碼端點
app.MapPost("/api/forgotPassword", async (
    [FromBody] ForgotPasswordRequest request,
    UserManager<IdentityUser> userManager,
    IEmailSender emailSender,
    ILogger<Program> logger) =>
{
    // 確認端點是否真的被呼叫
    logger.LogInformation(
        "進入自訂 /api/forgotPassword 端點 - 請求 email: {Email}",
        request.Email
    );

    var user = await userManager.FindByEmailAsync(request.Email);

    logger.LogInformation(
        "查詢 email {Email} 的使用者：{UserExists}, Email 已確認：{IsConfirmed}",
        request.Email,
        user != null ? "存在" : "不存在",
        user != null ? (await userManager.IsEmailConfirmedAsync(user)).ToString() : "N/A"
    );

    //if (user == null || !await userManager.IsEmailConfirmedAsync(user))
    //{
    //    logger.LogInformation(
    //        "跳過寄信 - 原因：使用者不存在或 email 未確認 (email: {Email})",
    //        request.Email
    //    );
    //     //為了安全，無論是否存在都回 200（防範列舉攻擊）
    //    return Results.Ok();
    //}

    // 產生重設密碼的短效驗證碼（6 位數字）
    var token = await userManager.GenerateTwoFactorTokenAsync(user, "Email");

    var resetLinkOrCode = token;

    var htmlMessage = $@"
    <div style=""background: #f5f7fa; border: 1px solid #d1d9e0; border-radius: 8px; padding: 24px 16px; text-align: center; margin: 24px 0;"">
        <div style=""font-size: 1.1em; color: #555; margin-bottom: 12px;"">重設碼</div>
        <strong style=""color: #0066cc; font-size: 1.8em; letter-spacing: 0.2em; font-family: monospace; margin-bottom: 12px;"">{resetLinkOrCode}</strong>
        <div style=""font-size: 1.1em; color: #555;"">請輸入此碼重設密碼</div>
    </div>";

    try
    {
        await emailSender.SendEmailAsync(
            request.Email,
            "重設您的 AI 智慧家庭管家 密碼",
            htmlMessage);

        logger.LogInformation("重設密碼郵件已嘗試寄送至 {Email}", request.Email);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "寄送重設密碼郵件失敗 - email: {Email}", request.Email);
        // 這裡可以選擇仍回 200（使用者體驗），或回 500（開發階段建議）
        // return Results.StatusCode(500);
    }

    return Results.Ok();
})
.RequireRateLimiting("Default"); // 使用已註冊的 policy

// 自訂義重設密碼端點
app.MapPost("/api/resetPassword", async (
    [FromBody] ResetPasswordRequest request,
    UserManager<IdentityUser> userManager,
    ILogger<Program> logger) =>
{
    var user = await userManager.FindByEmailAsync(request.Email);
    if (user == null)
    {
        // 為了安全，不告訴前端「帳號不存在」
        return Results.BadRequest(new { message = "重設失敗，請確認資料" });
    }

    // 使用 VerifyTwoFactorTokenAsync 驗證剛才產生的 6 位數碼
    var isValid = await userManager.VerifyTwoFactorTokenAsync(
        user,
        "Email",                    // 必須與產生時使用的 provider 名稱一致
        request.ResetCode           // 使用者輸入的 6 位數碼
    );

    if (!isValid)
    {
        logger.LogWarning("無效的驗證碼 - email: {Email}", request.Email);
        return Results.BadRequest(new { message = "驗證碼無效或已過期" });
    }

    // 驗證通過 → 直接重設密碼
    var removeResult = await userManager.RemovePasswordAsync(user);
    if (!removeResult.Succeeded)
    {
        return Results.BadRequest(new { errors = removeResult.Errors });
    }

    var addResult = await userManager.AddPasswordAsync(user, request.NewPassword);
    if (!addResult.Succeeded)
    {
        return Results.BadRequest(new { errors = addResult.Errors });
    }

    logger.LogInformation("密碼重設成功 - email: {Email}", request.Email);

    return Results.Ok();
})
.RequireRateLimiting("Default");

// 映射 Identity API Endpoints，這會自動為 IdentityUser 類別生成對應的 API 路由
app.MapIdentityApi<IdentityUser>();

//它負責將 HTTP 請求 映射到你的 Controller（控制器） 動作上
app.MapControllers();

app.Run();
