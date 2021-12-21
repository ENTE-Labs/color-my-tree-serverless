namespace ColorMyTree.Models.Gift;

public class GiftResponseModel
{
    public string Id { get; set; }

    public OrnamentTransferModel Ornament { get; set; }
    public CardTransferModel Card { get; set; }

    public GiftResponseModel()
    {
    }

    public GiftResponseModel(Data.Gift gift)
    {
        Id = gift.Id;
        Ornament = new OrnamentTransferModel(gift.Ornament);
        Card = new CardTransferModel(gift.Card);
    }
}