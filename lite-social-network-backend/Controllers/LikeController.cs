using lite_social_network_backend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lite_social_network_backend.Controllers;

[ApiController]
[Route("api/likes")]
public class LikeController : Controller
{
    private readonly AppDbContext _dbContext;

    public LikeController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    [Authorize]
    [HttpGet]
    [Route("{postId}")]
    public async Task<IActionResult> LikePost([FromRoute] int postId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        var post = await _dbContext.Posts.FirstOrDefaultAsync(p => p.Id == postId);
        if (user != null && post != null)
        {
            var like = await _dbContext.Likes.FirstOrDefaultAsync(l => l.Post.Id == postId && l.User.Id == user.Id);
            if (like != null)
            {
                return BadRequest("Already liked that post");
            }

            await _dbContext.Likes.AddAsync(new Like
            {
                Post = post,
                User = user
            });
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        return BadRequest();
    }
    
    [Authorize]
    [HttpGet]
    [Route("remove/{postId}")]
    public async Task<IActionResult> RemoveLikeOnPost([FromRoute] int postId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        var post = await _dbContext.Posts.FirstOrDefaultAsync(p => p.Id == postId);
        if (user != null && post != null)
        {
            var like = await _dbContext.Likes.FirstOrDefaultAsync(l => l.Post.Id == postId && l.User.Id == user.Id);
            if (like != null)
            {
                _dbContext.Likes.Remove(like);
                await _dbContext.SaveChangesAsync();
                return Ok();
            }

            return BadRequest("Not liked that post yet");

        }

        return BadRequest();
    }
    
}