using System.ComponentModel.DataAnnotations.Schema;

namespace lite_social_network_backend.Entities;

public class Friend
{
    public int Id { get;  set; }
    public int UserId { get; set; }
    public int FriendUserId { get; set; }
    public virtual User User { get; set; }
    public virtual User FriendUser { get; set; }
}