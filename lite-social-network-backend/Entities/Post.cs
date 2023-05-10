namespace lite_social_network_backend.Entities;

public class Post
{
    public int Id { get; set; }
    public string Text { get; set; }
    public string ImageUrl { get; set; }
    public DateTime CreationDate { get; set; }
    public virtual User Author { get; set; }
    public virtual List<Like> Likes { get; set; }
}