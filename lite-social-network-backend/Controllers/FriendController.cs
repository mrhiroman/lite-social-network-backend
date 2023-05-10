using lite_social_network_backend.Entities;
using lite_social_network_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lite_social_network_backend.Controllers;

[ApiController]
[Route("api/friends")]
public class FriendController: Controller
{
    private readonly AppDbContext _dbContext;

    public FriendController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Authorize]
    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> AddFriend([FromRoute] int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        var friend = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user != null && friend != null)
        {
            if (user.Friends.Any(f => f.Id == id))
            {
                return BadRequest("Already friends!");
            }

            await _dbContext.Friends.AddAsync(new Friend
            {
                User = user,
                FriendUser = friend
            });
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        return BadRequest();
    }
    
    [Authorize]
    [HttpGet]
    [Route("remove/{id}")]
    public async Task<IActionResult> RemoveFriend([FromRoute] int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        var friend = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user != null && friend != null)
        {
            if (user.Friends.FirstOrDefault(f => f.FriendUser.Id == id) is var friendEntity)
            {
                _dbContext.Friends.Remove(friendEntity);
                await _dbContext.SaveChangesAsync();
                return Ok();
            }
            
            return BadRequest("You are not friends!");
            
        }

        return BadRequest();
    }

    [Authorize]
    [HttpGet]
    [Route("")]
    [ProducesResponseType(typeof(IEnumerable<UserModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFriends()
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        if (user != null)
        {
            var friends = await _dbContext.Users.Where(u => user.FriendedBy.Any(f => f.Id == user.Id))
                                                .Select(u => new UserModel
                                                {
                                                    Id = u.Id,
                                                    Name = u.Name,
                                                    Age = u.Age,
                                                    City = u.City,
                                                    Education = u.Education,
                                                    AvatarUrl = u.ImageUrl
                                                })
                                                .ToListAsync();
            return Json(friends);
        }

        return BadRequest();
    }
}