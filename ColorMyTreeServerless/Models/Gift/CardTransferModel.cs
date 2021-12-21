using ColorMyTree.Models.Data;

namespace ColorMyTree.Models.Gift;

public class CardTransferModel
{
    public string Message { get; set; }
    public string ImageUrl { get; set; }
    public string NickName { get; set; }

    public CardTransferModel(Card card)
    {
        Message = card.Message;
        ImageUrl = card.ImageUrl;
        NickName = card.NickName;
    }

    public CardTransferModel()
    {
    }
}