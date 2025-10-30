using PointingParty.Client.Components;

namespace PointingParty.Client.Tests;

public class QrCodeTests : BunitContext
{
    [Fact]
    public void Renders_Qr_As_Image_DataUri()
    {
        // Arrange
        var cut = Render<QrCode>(parameters => parameters.Add(p => p.GameId, "TestGame"));

        // Act - look for an <img> with a data:image src
        var imgElements = cut.FindAll("img");
        var hasDataUri = imgElements.Any(i => (i.GetAttribute("src") ?? string.Empty).StartsWith("data:image/"));

        // Assert - an image data URI should be rendered
        Assert.True(hasDataUri, "Expected an <img> with a data:image/ src.");
    }

    [Fact]
    public void Clicking_Qr_Opens_Modal_With_Larger_Image()
    {
        // Arrange
        var cut = Render<QrCode>(parameters => parameters.Add(p => p.GameId, "TestGame"));

        // Pre-check what's rendered initially
        var initialImgs = cut.FindAll("img")
            .Count(i => (i.GetAttribute("src") ?? string.Empty).StartsWith("data:image/"));

        // Act - click the button that should show the modal
        var button = cut.Find("button");
        button.Click();

        // After clicking, the modal should be present and contain at least one data-uri image.
        var imgsAfter = cut.FindAll("img")
            .Count(i => (i.GetAttribute("src") ?? string.Empty).StartsWith("data:image/"));

        Assert.True(imgsAfter > initialImgs,
            "Expected clicking the QR to open a modal containing a larger image data URI.");
    }
}
