using DittoSDK;
using System.Text.Json.Serialization;

public struct DittoTask(string body, bool isCompleted)
{
    public const string CollectionName = "tasks";

    [JsonPropertyName("_id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("body")]
    public string Body { get; set; } = body;

    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; } = isCompleted;

    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    public override readonly string ToString()
    {
        return $"Task _id: {Id}, body: {Body}, isCompleted: {IsCompleted}, isDeleted: {IsDeleted}";
    }

    public readonly Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { "_id", Id },
            { "body", Body },
            { "isCompleted", IsCompleted },
            { "isDeleted", IsDeleted },
        };
    }
}
