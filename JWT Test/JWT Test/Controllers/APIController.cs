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
			return token;
		}

		[Authorize]
		public IActionResult AutTest()
		{
			return Ok();
		}
	}
}
