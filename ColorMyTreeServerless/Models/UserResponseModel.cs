using System.Collections.Generic;
using System.Linq;
using ColorMyTree.Models.Data;
using ColorMyTree.Models.Gift;

namespace ColorMyTree.Models;

public class UserResponseModel
{
    public string Id { get; set; }
    public string NickName { get; set; }
    public IEnumerable<GiftResponseModel> Gifts { get; set; }

    public bool IsAfterChristmas { get; set; }

    public UserResponseModel()
    {
    }

    public UserResponseModel(CmtUser user, Data.Gift[] gifts)
    {
        Id = user.Id;
        NickName = user.NickName;

        Gifts = gifts.Select(g => new GiftResponseModel(g));
    }
}
