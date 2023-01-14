using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices.Marshalling;
using CQRS.Core.Domain;
using CQRS.Core.Messages;
using Post.Common.Events;

namespace Post.Cmd.Domain.Aggregates;

public class PostAggregate : AggregateRoot
{
    private bool _active;
    private string _author = null!;
    private readonly Dictionary<Guid, Tuple<string, string>> _comments = new();

    public bool Active { get => _active; set => _active = value; }

    public PostAggregate()
    {
    }

    public PostAggregate(Guid id, string author, string message)
    {
        RaiseEvent(new PostCreatedEvent
        {
            Id = id,
            Author = author,
            Message = message,
            DatePosted = DateTime.UtcNow
        });
    }

    public void Apply(PostCreatedEvent @event)
    {
        _id = @event.Id;
        _active = true;
        _author = @event.Author;
    }

    public void UpdateMessage(string message)
    {
        if (!_active)
        {
            throw new InvalidOperationException("You can't edit the message of an inactive post!");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new InvalidOperationException("You must provide non-empty body of the post!");
        }

        RaiseEvent(new MessageUpdatedEvent
        {
            Id = _id,
            Message = message
        });
    }

    public void Apply(MessageUpdatedEvent @event)
    {
        _id = @event.Id;
        
    }

    public void LikePost()
    {
        if (!_active)
        {
            throw new InvalidOperationException("You can't like an inactive post!");
        }

        RaiseEvent(new PostLikedEvent
        {
            Id = _id
        });
    }

    public void Apply(PostLikedEvent @event)
    {
        _id = @event.Id;
    }

    public void AddComment(string comment, string author)
    {
        if (!_active)
        {
            throw new InvalidOperationException("You can't add comment to an inactive post!");
        }

        if (string.IsNullOrWhiteSpace(comment))
        {
            throw new InvalidOperationException("You must provide non-empty comment!");
        }

        RaiseEvent(new CommentAddedEvent
        {
            Id = _id,
            CommentId = Guid.NewGuid(),
            Comment = comment,
            Author = author,
            CommentDate = DateTime.UtcNow
        });
    }

    public void Apply(CommentAddedEvent @event)
    {
        _id = @event.Id;
        _comments.Add(@event.CommentId, new Tuple<string, string>(@event.Comment, @event.Author));
    }

    public void EditComment(Guid commentId, string comment, string author)
    {
        if (!_active)
        {
            throw new InvalidOperationException("You can't edit comment of an inactive post!");
        }

        if (_comments[commentId].Item2.Equals(author, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new InvalidOperationException("You are not allowed to edit comment of other user!");
        }

        RaiseEvent(new CommentUpdatedEvent
        {
            Id = _id,
            CommentId = commentId,
            Comment = comment,
            Author = author,
            EditDate = DateTime.UtcNow,
        });
    }

    public void Apply(CommentUpdatedEvent @event)
    {
        _id = @event.Id;
        _comments[@event.CommentId] = Tuple.Create(@event.Comment, @event.Author);
    }

    public void RemoveComment(Guid commentId, string username)
    {
        if (!_active)
        {
            throw new InvalidOperationException("Can not remove comment of inactive post!");
        }

        if (!_comments[commentId].Item2.Equals(username, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new InvalidOperationException("Can not remove comment of other user!");
        }

        RaiseEvent(new CommentRemovedEvent
        {
            Id = _id,
            CommentId = commentId,
        });
    }

    public void Apply(CommentRemovedEvent @event)
    {
        _id = @event.Id;
        _comments.Remove(@event.CommentId);
    }

    public void RemovePost(string username)
    {
        if (!_active)
        {
            throw new InvalidOperationException("The post has already been removed");
        }

        if (!_author.Equals(username, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new InvalidOperationException("Can not remove post that was created by someone else");
        }

        RaiseEvent(new PostRemovedEvent
        {
            Id = _id
        });
    }

    public void Apply(PostRemovedEvent @event)
    {
        _id = @event.Id;
        _active = false;
    }
}