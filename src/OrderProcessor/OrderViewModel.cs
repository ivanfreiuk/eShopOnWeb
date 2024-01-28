using Newtonsoft.Json;

namespace OrderProcessor;

public class OrderViewModel
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }
    public string ShippingAddress { get; set; }
    public List<string> Items { get; set; }
    public decimal Price { get; set; }
}
