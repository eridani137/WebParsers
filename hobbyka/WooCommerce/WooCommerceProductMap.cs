using CsvHelper.Configuration;

namespace hobbyka.WooCommerce;

public sealed class WooCommerceProductMap : ClassMap<WooCommerceProduct>
{
    public WooCommerceProductMap()
    {
        Map(m => m.ID).Name("ID");
        Map(m => m.Type).Name("Type");
        Map(m => m.SKU).Name("SKU");
        Map(m => m.Name).Name("Name");
        Map(m => m.Published).Name("Published");
        Map(m => m.IsFeatured).Name("Is featured?");
        Map(m => m.VisibilityInCatalog).Name("Visibility in catalog");
        Map(m => m.ShortDescription).Name("Short description");
        Map(m => m.Description).Name("Description");
        Map(m => m.DateSalePriceStarts).Name("Date sale price starts");
        Map(m => m.DateSalePriceEnds).Name("Date sale price ends");
        Map(m => m.TaxStatus).Name("Tax status");
        Map(m => m.TaxClass).Name("Tax class");
        Map(m => m.InStock).Name("In stock?");
        Map(m => m.Stock).Name("Stock");
        Map(m => m.LowStockAmount).Name("Low stock amount");
        Map(m => m.BackordersAllowed).Name("Backorders allowed?");
        Map(m => m.SoldIndividually).Name("Sold individually?");
        Map(m => m.Weight).Name("Weight (kg)");
        Map(m => m.Length).Name("Length (cm)");
        Map(m => m.Width).Name("Width (cm)");
        Map(m => m.Height).Name("Height (cm)");
        Map(m => m.AllowCustomerReviews).Name("Allow customer reviews?");
        Map(m => m.PurchaseNote).Name("Purchase note");
        Map(m => m.SalePrice).Name("Sale price");
        Map(m => m.RegularPrice).Name("Regular price");
        Map(m => m.Categories).Name("Categories");
        Map(m => m.Tags).Name("Tags");
        Map(m => m.ShippingClass).Name("Shipping class");
        Map(m => m.Images).Name("Images");
        Map(m => m.DownloadLimit).Name("Download limit");
        Map(m => m.DownloadExpiryDays).Name("Download expiry days");
        Map(m => m.Parent).Name("Parent");
        Map(m => m.GroupedProducts).Name("Grouped products");
        Map(m => m.Upsells).Name("Upsells");
        Map(m => m.CrossSells).Name("Cross-sells");
        Map(m => m.ExternalUrl).Name("External URL");
        Map(m => m.ButtonText).Name("Button text");
        Map(m => m.Position).Name("Position");
        Map(m => m.Attribute1Name).Name("Attribute 1 name");
        Map(m => m.Attribute1Values).Name("Attribute 1 value(s)");
        Map(m => m.Attribute1Visible).Name("Attribute 1 visible");
        Map(m => m.Attribute1Global).Name("Attribute 1 global");
        Map(m => m.Attribute2Name).Name("Attribute 2 name");
        Map(m => m.Attribute2Values).Name("Attribute 2 value(s)");
        Map(m => m.Attribute2Visible).Name("Attribute 2 visible");
        Map(m => m.Attribute2Global).Name("Attribute 2 global");
    }
}