namespace lite_social_network_backend.Models;

public class UserModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string AvatarUrl { get; set; }
    public int Age { get; set; }
    public string City { get; set; }
    public string Education { get; set; }
    
    public List<UserModel> Friends { get; set; }
}