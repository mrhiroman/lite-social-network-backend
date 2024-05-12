using System.Text.Json.Serialization;
using lite_social_network_backend.Entities;
using lite_social_network_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lite_social_network_backend.Controllers;

[ApiController]
[Route("api/posts")]
public class PostController: Controller
{
    private readonly AppDbContext _dbContext;
    private HttpClient _httpClient;

    public PostController(AppDbContext dbContext, HttpClient httpClient)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
    }
    
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(IEnumerable<PostModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserPosts([FromRoute]int userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user != null)
        {
            var posts = await _dbContext.Posts.Where(x => x.Author.Id == user.Id).ToListAsync();
            return Json(posts.Select(p => new PostModel
            {
                Id = p.Id,
                Author = new UserModel
                {
                    Id = p.Author.Id,
                    Name = p.Author.Name,
                    Age = p.Author.Age,
                    City = p.Author.City,
                    Education = p.Author.Education,
                    AvatarUrl = p.Author.ImageUrl
                },
                Likes = p.Likes.Select(l => new LikeModel
                {
                    PostId = l.Post.Id,
                    UserId = l.User.Id
                }).ToList(),
                Text = p.Text,
                ImageUrl = p.ImageUrl,
                CreationDate = p.CreationDate
            })
                .OrderByDescending(p => p.CreationDate)
            );
        }

        return BadRequest();
    }

    [Authorize]
    [HttpGet]
    [Route("news")]
    [ProducesResponseType(typeof(IEnumerable<PostModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNewsPosts()
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        if (user != null)
        {
            var friends = await _dbContext.Friends.Where(f => f.User.Id == user.Id).Select(f => f.FriendUser.Id).ToListAsync();
            var posts = await _dbContext.Posts.Where(p => friends.Contains(p.Author.Id)).ToListAsync();
            return Json(posts.Select(p => new PostModel
            {
                Id = p.Id,
                Author = new UserModel
                {
                    Id = p.Author.Id,
                    Name = p.Author.Name,
                    Age = p.Author.Age,
                    City = p.Author.City,
                    Education = p.Author.Education,
                    AvatarUrl = p.Author.ImageUrl
                },
                Likes = p.Likes.Select(l => new LikeModel
                {
                    PostId = l.Post.Id,
                    UserId = l.User.Id
                }).ToList(),
                Text = p.Text,
                ImageUrl = p.ImageUrl,
                CreationDate = p.CreationDate
            })
                .OrderByDescending(p => p.CreationDate));
        }

        return BadRequest();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddPost([FromBody] AddPostModel model)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        if (user != null)
        {
            var imageUrl = "";
            if (model.Image != "")
            {
                var url = $"https://api.imgbb.com/1/upload?key=d5d402271f3c028176e1b975a518890f";
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(model.Image.Split(',')[1]), "image");
                var request = new HttpRequestMessage(HttpMethod.Post, url) {Content = content};
                var uploadResponse = await _httpClient.SendAsync(request);

                if (uploadResponse.IsSuccessStatusCode)
                {
                    var respData = await uploadResponse.Content.ReadAsStringAsync();
                    var json = System.Text.Json.JsonDocument.Parse(respData);
                    imageUrl = json.RootElement.GetProperty("data").GetProperty("url").ToString();
                }
                else
                {
                    return BadRequest("Image upload service returned an error!");
                }
            }

            await _dbContext.Posts.AddAsync(new Post
                {
                    Author = user,
                    Text = model.Text,
                    ImageUrl = imageUrl,
                    CreationDate = DateTime.Now
                });
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        return BadRequest();
    }
    
    [Authorize]
    [HttpPost]
    [Route("update/{id}")]
    public async Task<IActionResult> UpdatePost([FromBody] AddPostModel model, [FromRoute] int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        var post = await _dbContext.Posts.FirstOrDefaultAsync(p => p.Id == id && user != null && p.Author.Id == user.Id);
        if (user != null && post != null)
        {
            var imageUrl = "";
            if (model.Image != "")
            {
                var url = $"https://api.imgbb.com/1/upload?key=d5d402271f3c028176e1b975a518890f";
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(model.Image.Split(',')[1]), "image");
                var request = new HttpRequestMessage(HttpMethod.Post, url) {Content = content};
                var uploadResponse = await _httpClient.SendAsync(request);

                if (uploadResponse.IsSuccessStatusCode)
                {
                    var respData = await uploadResponse.Content.ReadAsStringAsync();
                    var json = System.Text.Json.JsonDocument.Parse(respData);
                    imageUrl = json.RootElement.GetProperty("data").GetProperty("url").ToString();
                }
                else
                {
                    return BadRequest("Image upload service returned an error!");
                }
            }

            post.Text = model.Text;
            post.ImageUrl = post.ImageUrl;
            post.CreationDate = post.CreationDate.Subtract(TimeSpan.FromHours(3));
            _dbContext.Posts.Update(post);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        return BadRequest();
    }

    [Authorize]
    [HttpGet]
    [Route("remove/{id}")]
    public async Task<IActionResult> RemovePost([FromRoute] int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        var post = await _dbContext.Posts.FirstOrDefaultAsync(p => p.Id == id && user != null && p.Author.Id == user.Id);
        if (user != null && post != null)
        {
            var postLikes = await _dbContext.Likes.Where(l => l.Post.Id == post.Id).ToListAsync();
            _dbContext.Likes.RemoveRange(postLikes);
            _dbContext.Posts.Remove(post);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        return BadRequest();
    }




}