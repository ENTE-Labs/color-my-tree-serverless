using System;
using ColorMyTree.Helpers;

namespace ColorMyTree.Models.Data;

public class Gift
{
    public string UserId { get; set; }
    public string Id { get; set; } = IdentityHelper.GenerateId();

    public Ornament Ornament { get; set; }
    public Card Card { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public string IpAddress { get; set; }
}
