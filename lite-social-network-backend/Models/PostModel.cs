using lite_social_network_backend.Entities;

namespace lite_social_network_backend.Models;

public class PostModel
{
    public int Id { get; set; }
    public string Text { get; set; }
    public string ImageUrl { get; set; }
    public DateTime CreationDate { get; set; }
    public UserModel Author { get; set; }
    public List<LikeModel> Likes { get; set; }
}