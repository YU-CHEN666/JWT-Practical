using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace JWT_Test.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class APIController : ControllerBase
	{
		private readonly IConfiguration _configuration;
		public APIController(IConfiguration iConfiguration)
		{
			_configuration = iConfiguration;
		}
		public string Login()
		{
			/*去資料庫取得帳號密碼，判斷對不對，不對中斷函式，對的話繼續*/

			//建立身分聲明 (Claims)
			var claims = new List<Claim>
			{
				new Claim(JwtRegisteredClaimNames.Sub, "userName"),
				new Claim(ClaimTypes.Role, "Admin"),
				new Claim(JwtRegisteredClaimNames.Iat,DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
				new Claim("what","hey") //自訂欄位
			};

			//取得appsettings.json裡自訂的私鑰(KEY)
			//私鑰必須要32字元(256bit)
			//不然後續會報錯
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:KEY"]));

			//設定JWT Token
			var jwt = new JwtSecurityToken
			(
				issuer: _configuration["JWT:Issuer"], //iss (發行者)
				audience: _configuration["JWT:Audience"], //aud (接收者)
				claims: claims,
				notBefore:DateTime.Now,
				expires: DateTime.Now.AddMinutes(5),  //exp (過期時間)
				signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
			);

			var jwtt = new JwtSecurityToken();

			//產生JWT Token，這就就是那一長字串
			var token = new JwtSecurityTokenHandler().WriteToken(jwt);

			CookieOptions options = new CookieOptions
			{
				// 只能由伺服器存取，JavaScript 無法讀取（防範 XSS），一定要設定為True
				HttpOnly = true,

				// 前端只有在 HTTPS 連線時才傳送,一定要設定為True
				Secure = true,

				// 限制跨站請求（防範 CSRF）
				SameSite = SameSiteMode.Lax
			};
			Response.Cookies.Append("JWTToken",token, options);
			return "已成功將Token寫入Cookie";
		}

		//獲取Role沒權限的Token
		public string LoginRoleNo()
		{
			var claims = new List<Claim>
			{
				new Claim(JwtRegisteredClaimNames.Sub, "userName"),
				new Claim(ClaimTypes.Role, "Ad"),
				new Claim(JwtRegisteredClaimNames.Iat,DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
				new Claim("what","hey") 
			};

			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:KEY"]));

			var jwt = new JwtSecurityToken
			(
				issuer: _configuration["JWT:Issuer"],
				audience: _configuration["JWT:Audience"], 
				claims: claims,
				notBefore: DateTime.Now,
				expires: DateTime.Now.AddMinutes(5),  
				signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
			);

			var jwtt = new JwtSecurityToken();

			var token = new JwtSecurityTokenHandler().WriteToken(jwt);

			CookieOptions options = new CookieOptions
			{
				// 只能由伺服器存取，JavaScript 無法讀取（防範 XSS），一定要設定為True
				HttpOnly = true,

				// 前端只有在 HTTPS 連線時才傳送,一定要設定為True
				Secure = true,

				// 限制跨站請求（防範 CSRF）
				SameSite = SameSiteMode.Lax
			};
			Response.Cookies.Append(_configuration["TokenCookieName"], token, options);
			return "已成功將Token寫入Cookie";
		}

		//權限測試有登入就好
		[Authorize]
		public IActionResult AutTest()
		{
			return Ok();
		}

		//權限測試有登入並且Role="Admin"
		[Authorize(Policy = "Role:Admin")]
		public IActionResult AutRoleTest()
		{
			return Ok();
		}
	}
}
