using ITStoreApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ITStoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        //public static Member member = new Member();
        private readonly SqlDataAccess _db;

        public IConfiguration _config;

        public AuthController(IConfiguration config, SqlDataAccess db)
        {
            _config = config;
            _db = db;
        }

        [HttpPost("register")]
        public async Task<ActionResult<Member>> RegisterAsync(Member request)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            
            string sql = @"insert into dbo.Member2 (FirstName, LastName, Email, Password, Role) 
                          values (@FirstName, @LastName, @Email, @Password, @Role);";
            var member = new Member()
            {
                Email = request.Email,
                Password = passwordHash,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = request.Role
            };
            
            await _db.SaveData(sql, member);

            return Ok(member);
        }

        // ฟังก์ชัน Login รับพารามิเตอร์เป็น MemberDto request
        [HttpPost("login")]
        public ActionResult<Member> Login(MemberDto request)
        {
            // สร้างคำสั่ง SQL เพื่อดึงข้อมูลสมาชิกจากฐานข้อมูลโดยใช้ Email
            string sql = "select * from dbo.Member2 where Email=@Email";

            // เรียกใช้ฟังก์ชัน LoadSingleData เพื่อดึงข้อมูลสมาชิกจากฐานข้อมูล
            // โดยส่งคำสั่ง SQL และพารามิเตอร์ Email ที่ได้รับจาก request
            Member memberAccount = _db.LoadSingleData<Member, dynamic>(sql, new { Email = request.Email }).Result;

            // ตรวจสอบว่าพบข้อมูลสมาชิกหรือไม่ และตรวจสอบรหัสผ่านว่าตรงกันหรือไม่
            if (memberAccount == null || !BCrypt.Net.BCrypt.Verify(request.Password, memberAccount.Password))
            {
                // ถ้าไม่พบข้อมูลสมาชิกหรือรหัสผ่านไม่ตรงกัน ให้ส่งสถานะ BadRequest พร้อมข้อความ "Invalid Email or Password"
                return BadRequest(new { message = "Invalid Email or Password" });
            }

            // ถ้าข้อมูลสมาชิกและรหัสผ่านถูกต้อง ให้สร้าง Token สำหรับการยืนยันตัวตน
            string token = CreateToken(memberAccount);

            // ส่งสถานะ Ok พร้อมข้อความ "Login Success" และ Token ที่สร้างขึ้น
            return Ok(new { message = "Login Success", token = token! });
        }
        private string CreateToken(Member member)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim("email", member.Email),
                new Claim("name", $"{member.FirstName} {member.LastName}"),
                new Claim("role", member.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value!));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var token = new JwtSecurityToken(
                claims: claims, 
                expires:DateTime.Now.AddDays(1),
                signingCredentials:cred
                );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }
}
