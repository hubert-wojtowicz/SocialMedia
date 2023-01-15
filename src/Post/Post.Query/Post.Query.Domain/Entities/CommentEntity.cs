using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Post.Query.Domain.Entities;

[Table("Comments")]
public class CommentEntity
{
    [Key]
    public Guid CommentId { get; set; }

    public string Username { get; set; }

    public DateTime DateCommented { get; set; }

    public string Comment { get; set; }

    public bool Edited { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual PostEntity Post { get; set; }
}