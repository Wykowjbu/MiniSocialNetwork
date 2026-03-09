using System;
using System.Collections.Generic;

namespace MiniSocialNetwork.Models;

public partial class VwHomeFeed
{
    public int PostId { get; set; }

    public string Content { get; set; } = null!;

    public string? ImagePath { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string? AvatarPath { get; set; }
}
