using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
	// 保持 JWT 原生 Claim 欄位名稱
	options.MapInboundClaims = false; 
	//設定驗證選項
	options.TokenValidationParameters = new TokenValidationParameters
	{
		//是否驗證Issuer
		ValidateIssuer = true, 
		ValidIssuer = builder.Configuration["JWT:Issuer"],

		//是否驗證Audience
		ValidateAudience = true,
		ValidAudience = builder.Configuration["JWT:Audience"],

		//是否驗證有效期限，預設是true
		ValidateLifetime = true,
		//是否要給予緩衝，預設是5分鐘，以下設定會變成一過期馬上失效
		ClockSkew = TimeSpan.Zero,

		//是否強制要求簽章的金鑰是信任的發行者提供的，在動態金鑰系統中非常重要，預設是false，建議一律開啟。
		ValidateIssuerSigningKey = true,
			
		//要驗證的簽章金鑰
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:KEY"])),
	};
});

builder.Services.AddAuthorizationBuilder().AddPolicy("Role:Admin", policy =>
{
	policy.RequireRole("Admin");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
