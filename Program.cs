using BackendApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 設定 EF Core 使用 In-Memory Database，並指定資料庫名稱為 "ApplicationDb"
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseInMemoryDatabase("ApplicationDb"));

// 啟動 Identity API Endpoints（內部已包含 cookie + bearer 支援）
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

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

// 啟用 HTTPS 重定向，確保 API 只能透過 HTTPS 存取（生產環境建議啟用）
//app.UseHttpsRedirection();

// 先認證
app.UseAuthentication();

// 再授權
app.UseAuthorization();

// 映射 Identity API Endpoints，這會自動為 IdentityUser 類別生成對應的 API 路由
app.MapIdentityApi<IdentityUser>();

//它負責將 HTTP 請求 映射到你的 Controller（控制器） 動作上
app.MapControllers();

app.Run();
