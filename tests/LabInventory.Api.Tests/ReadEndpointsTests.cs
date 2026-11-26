using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LabInventory.Api.Endpoints;

namespace LabInventory.Api.Tests;

public class ReadEndpointsTests(InventoryApiFactory factory) : IClassFixture<InventoryApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private async Task<T> GetJson<T>(string url)
    {
        var response = await _client.GetAsync(url, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<T>(Json, Ct))!;
    }

    private async Task<int> ProductIdBySku(string sku)
    {
        var products = await GetJson<List<ProductSummary>>($"/api/products?search={sku}");
        return Assert.Single(products).Id;
    }

    [Fact]
    public async Task Search_without_term_returns_all_products_ordered_by_sku()
    {
        var products = await GetJson<List<ProductSummary>>("/api/products");

        Assert.Equal(16, products.Count);
        Assert.Equal(products.OrderBy(p => p.Sku, StringComparer.Ordinal).Select(p => p.Sku), products.Select(p => p.Sku));
    }

    [Fact]
    public async Task Search_is_substring_match_so_etanol_also_matches_metanol()
    {
        var products = await GetJson<List<ProductSummary>>("/api/products?search=etanol");

        Assert.Equal(["REA-001", "REA-005"], products.Select(p => p.Sku));
    }

    [Fact]
    public async Task Search_does_not_ignore_accents_yet()
    {
        // Limitación conocida de LIKE en SQLite: "acido" no encuentra "Ácido sulfúrico".
        var products = await GetJson<List<ProductSummary>>("/api/products?search=acido");

        Assert.Empty(products);
    }

    [Fact]
    public async Task Search_rejects_terms_longer_than_100_characters()
    {
        var response = await _client.GetAsync($"/api/products?search={new string('a', 101)}", Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_product_returns_404_when_missing()
    {
        var response = await _client.GetAsync("/api/products/9999", Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Stock_separates_available_from_expired_lots()
    {
        var id = await ProductIdBySku("REA-001");

        var stock = await GetJson<ProductStock>($"/api/products/{id}/stock");

        Assert.Equal(43m, stock.AvailableQuantity);
        Assert.Equal(12m, stock.ExpiredQuantity);
        Assert.False(stock.BelowReorderPoint);
        Assert.Equal(3, stock.Lots.Count);
        Assert.True(stock.Lots.Single(l => l.LotNumber == "ET-2401").IsExpired);
    }

    [Fact]
    public async Task Stock_flags_products_below_reorder_point()
    {
        var id = await ProductIdBySku("REA-002");

        var stock = await GetJson<ProductStock>($"/api/products/{id}/stock");

        Assert.Equal(3.5m, stock.AvailableQuantity);
        Assert.True(stock.BelowReorderPoint);
    }

    [Fact]
    public async Task Stock_returns_404_when_product_missing()
    {
        var response = await _client.GetAsync("/api/products/9999/stock", Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Expiring_lots_default_window_is_30_days_and_excludes_already_expired()
    {
        var lots = await GetJson<List<ExpiringLot>>("/api/lots/expiring");

        Assert.Equal(["BF-2409", "HS-2402", "AC-2406", "RN-2312", "ET-2405"], lots.Select(l => l.LotNumber));
        Assert.Equal([3, 5, 12, 20, 25], lots.Select(l => l.DaysUntilExpiration));
        Assert.All(lots, l => Assert.True(l.ExpirationDate >= InventoryApiFactory.Today));
    }

    [Fact]
    public async Task Expiring_lots_respects_custom_window()
    {
        var lots = await GetJson<List<ExpiringLot>>("/api/lots/expiring?days=7");

        Assert.Equal(["BF-2409", "HS-2402"], lots.Select(l => l.LotNumber));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(366)]
    public async Task Expiring_lots_rejects_window_out_of_range(int days)
    {
        var response = await _client.GetAsync($"/api/lots/expiring?days={days}", Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Orders_can_be_filtered_by_status()
    {
        var all = await GetJson<List<RestockOrderView>>("/api/orders");
        var pending = await GetJson<List<RestockOrderView>>("/api/orders?status=Pending");

        Assert.Equal(2, all.Count);
        var order = Assert.Single(pending);
        Assert.Equal("REA-002", order.Sku);
    }

    [Fact]
    public async Task Get_order_returns_404_when_missing()
    {
        var response = await _client.GetAsync("/api/orders/9999", Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
