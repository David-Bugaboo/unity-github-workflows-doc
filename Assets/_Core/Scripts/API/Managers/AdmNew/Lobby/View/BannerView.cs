using System.Collections.Generic;

public class BannerView : BaseUploaderView
{
    protected override string UploadType => "BANNER";
    protected override string[] SlotKeys => new[] { "1", "2", "3" };
    
    public BannerView() : base()
    {
        _slotPaths = new string[3];
    }
}