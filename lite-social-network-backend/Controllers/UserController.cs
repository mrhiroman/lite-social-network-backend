using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using lite_social_network_backend.Entities;
using lite_social_network_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace lite_social_network_backend.Controllers;

[ApiController]
[Route("api/users")]
public class UserController: Controller
{
    private readonly AppDbContext _dbContext;
    private readonly PasswordHashingService _passwordHashingService;
    private HttpClient _httpClient;

    public UserController(AppDbContext dbContext, PasswordHashingService passwordHashingService, HttpClient httpClient)
    {
        _dbContext = dbContext;
        _passwordHashingService = passwordHashingService;
        _httpClient = httpClient;
    }
 
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Token([FromBody] LoginUserModel model)
    {
        var identity = await GetIdentity(model.Email, model.Password); 
        if (identity == null) 
        { 
            return BadRequest(new { errorText = "Invalid username or password." }); 
        }
        
        var now = DateTime.UtcNow;
        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.ISSUER, 
            audience: AuthOptions.AUDIENCE, 
            notBefore: now, 
            claims: identity.Claims, 
            expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)), 
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
        
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
 
        var response = new 
        {
            access_token = encodedJwt
        };
 
        return Json(response);
    }
 
    private async Task<ClaimsIdentity> GetIdentity(string email, string password)
    {
        var hashedPassword = _passwordHashingService.GetSHA256(password);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email && x.Password == hashedPassword); 
        if (user != null)
        {
            var claims = new List<Claim> 
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email), 
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role)
            }; 
            ClaimsIdentity claimsIdentity = 
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, 
                    ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;
        }

        return null;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserModel model)
    {
        if (ValidateRegister(model))
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == model.Email);
            if ( user != null)
                return BadRequest("This email is already registered!"); 
            var hashedPassword = _passwordHashingService.GetSHA256(model.Password);
            await _dbContext.Users.AddAsync(new User
            {
                Email = model.Email,
                Name = model.Name,
                Password = hashedPassword,
                Role = "user",
                Age = 0,
                City = "None",
                Education = "None",
                ImageUrl = ""
            });
        }

        await _dbContext.SaveChangesAsync();
        return Json(new { model.Name, model.Password });
    }

    private bool ValidateRegister(RegisterUserModel model)
    {
        return (model.Email != String.Empty && model.Email.Length > 5 &&
                model.Name != String.Empty && model.Name.Length > 5 &&
                model.Password != string.Empty && model.Password.Length >= 8);
    }

    [Authorize]
    [HttpGet]
    [Route("me")]
    [ProducesResponseType(typeof(UserModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe()
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        if (user != null)
        {
            return Json(new UserModel
            {
                Id = user.Id,
                Name = user.Name,
                Age = user.Age,
                City = user.City,
                Education = user.Education,
                AvatarUrl = user.ImageUrl,
                Friends = user.Friends.Select(f => new UserModel
                {
                    Id = f.FriendUser.Id,
                    Name = f.FriendUser.Name,
                    Age = f.FriendUser.Age,
                    City = f.FriendUser.City,
                    Education = f.FriendUser.Education,
                    AvatarUrl = f.FriendUser.ImageUrl,
                    Friends = new List<UserModel>()
                }).ToList()
            });
        }

        return BadRequest();
    }

    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(typeof(UserModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserInfo([FromRoute] int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user != null)
        {
            return Json(new UserModel
            {
                Id = user.Id,
                Name = user.Name,
                Age = user.Age,
                City = user.City,
                Education = user.Education,
                AvatarUrl = user.ImageUrl
            });
        }

        return BadRequest();
    }
    
    [HttpPost]
    [Route("update")]
    public async Task<IActionResult> UpdateInfo([FromBody] UserInfoModel model)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        if (user != null)
        {
            user.Name = model.Name;
            user.Age = model.Age;
            user.City = model.City;
            user.Education = model.Education;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        return BadRequest();
    }
    
    [HttpPost]
    [Route("upload_avatar")]
    public async Task<IActionResult> UploadAvatar([FromBody] string avatar)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        if (user != null)
        {
            var imageUrl = "";
            if (avatar != "")
            {
                var url = $"https://api.imgbb.com/1/upload?key=d5d402271f3c028176e1b975a518890f";
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(avatar.Split(',')[1]), "image");
                var request = new HttpRequestMessage(HttpMethod.Post, url) {Content = content};
                var uploadResponse = await _httpClient.SendAsync(request);

                if (uploadResponse.IsSuccessStatusCode)
                {
                    var respData = await uploadResponse.Content.ReadAsStringAsync();
                    var json = System.Text.Json.JsonDocument.Parse(respData);
                    imageUrl = json.RootElement.GetProperty("data").GetProperty("url").ToString();
                    user.ImageUrl = imageUrl;
                    _dbContext.Users.Update(user);
                    await _dbContext.SaveChangesAsync();
                    return Ok();
                }
                else
                {
                    return BadRequest("Image upload service returned an error!");
                }
            }

            return Problem("Error uploading image", statusCode: 503);
        }

        return BadRequest();
    }
    
}