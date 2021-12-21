using System;
using ColorMyTree.Helpers;

namespace ColorMyTree.Models.Data;

public class CmtUser
{
    public string Id { get; set; } = IdentityHelper.GenerateId();
    public string NickName { get; set; }

    public string Login { get; set; }
    public string Password { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
    public string id => Id;
}
