using ColorMyTree.Models.Data;

namespace ColorMyTree.Models.Gift;

public class OrnamentTransferModel
{
    public string ImageUrl { get; set; }
    public string PrimaryThemeColor { get; set; }
    public string SecondaryThemeColor { get; set; }

    public OrnamentTransferModel(Ornament ornament)
    {
        ImageUrl = ornament.ImageUrl;
        PrimaryThemeColor = ornament.PrimaryThemeColor;
        SecondaryThemeColor = ornament.SecondaryThemeColor;
    }

    public OrnamentTransferModel()
    {
    }
}