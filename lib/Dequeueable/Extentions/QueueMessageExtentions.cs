using Dequeueable.AzureQueueStorage.Models;
using System.Text.Json;

namespace Dequeueable.AzureQueueStorage.Extentions
{
    internal static class QueueMessageExtentions
    {
        public static string GetValueByPropertyName(this Message queueMessage, string propertyName)
        {
            var document = JsonDocument.Parse(queueMessage.Body.ToMemory());
            var property = GetJsonElement(propertyName, document);

            return property.ValueKind switch
            {
                JsonValueKind.Undefined or JsonValueKind.Null => string.Empty,
                JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => property.ToString(),
                _ => throw new InvalidOperationException($"The value of type {property.ValueKind} cannot be parsed to a string"),
            };
        }

        private static JsonElement GetJsonElement(string propertyName, JsonDocument document)
        {
            var splittedProperty = propertyName.Split(':');
            var property = document.RootElement.GetProperty(splittedProperty[0]);
            for (var i = 1; i < splittedProperty.Length; i++)
            {
                property = property.GetProperty(splittedProperty[i]);
            }

            return property;
        }
    }
}
