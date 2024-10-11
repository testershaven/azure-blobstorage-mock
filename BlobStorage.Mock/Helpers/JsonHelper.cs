using System.Text.Json.Nodes;

namespace Blobstorage.Mock.Helpers;

public static class JsonHelper
{
    public static bool DeepCompare(JsonObject? fullObject, JsonObject partialObject)
    {
        var isEqual = false;

        if (fullObject is null)
        {
            return JsonNode.DeepEquals(fullObject, partialObject);
        }
        else
        {
            foreach (KeyValuePair<string, JsonNode?> item in partialObject)
            {
                if (fullObject.ContainsKey(item.Key))
                {
                    isEqual = item.Value is JsonObject
                        ? DeepCompare(fullObject[item.Key]?.AsObject(), item.Value.AsObject())
                        : JsonNode.DeepEquals(fullObject[item.Key], item.Value);
                }

                if (!isEqual)
                {
                    break;
                }
            }
        }

        return isEqual;
    }
}
