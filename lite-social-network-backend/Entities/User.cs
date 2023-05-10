namespace lite_social_network_backend.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
    public int Age { get; set; }
    public string City { get; set; }
    public string Education { get; set; }
    public string ImageUrl { get; set; }
    public virtual List<Post> Posts { get; set; }
    public virtual List<Like> Likes { get; set; }
    public virtual List<Friend> Friends { get; set; }
    public virtual List<Friend> FriendedBy { get; set; }
}