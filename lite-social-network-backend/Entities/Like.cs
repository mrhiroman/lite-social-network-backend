namespace lite_social_network_backend.Entities;

public class Like
{
    public int Id { get; set; }
    public virtual User User { get; set; }
    public virtual Post Post { get; set; }
}