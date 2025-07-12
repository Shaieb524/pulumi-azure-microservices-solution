using System.Collections.Generic;
using System.Text.Json;

namespace SharedResources.helpers
{
    public class ConfigParser
    {
        public static Dictionary<string, object> ConvertJsonElementToDictionary(JsonElement element)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var property in element.EnumerateObject())
            {
                string propertyName = property.Name;
                JsonElement propertyValue = property.Value;

                switch (propertyValue.ValueKind)
                {
                    case JsonValueKind.Object:
                        dictionary[propertyName] = ConvertJsonElementToDictionary(propertyValue);
                        break;

                    case JsonValueKind.Array:
                        dictionary[propertyName] = ConvertJsonElementToArray(propertyValue);
                        break;

                    case JsonValueKind.String:
                        dictionary[propertyName] = propertyValue.GetString() ?? string.Empty;
                        break;

                    case JsonValueKind.Number:
                        if (propertyValue.TryGetInt32(out int intValue))
                            dictionary[propertyName] = intValue;
                        else if (propertyValue.TryGetInt64(out long longValue))
                            dictionary[propertyName] = longValue;
                        else if (propertyValue.TryGetDouble(out double doubleValue))
                            dictionary[propertyName] = doubleValue;
                        break;

                    case JsonValueKind.True:
                        dictionary[propertyName] = true;
                        break;

                    case JsonValueKind.False:
                        dictionary[propertyName] = false;
                        break;

                    case JsonValueKind.Null:
                        dictionary[propertyName] = null!;
                        break;
                }
            }

            return dictionary;
        }

        private static object[] ConvertJsonElementToArray(JsonElement arrayElement)
        {
            var list = new List<object>();

            foreach (var item in arrayElement.EnumerateArray())
            {
                switch (item.ValueKind)
                {
                    case JsonValueKind.Object:
                        list.Add(ConvertJsonElementToDictionary(item));
                        break;
                    case JsonValueKind.Array:
                        list.Add(ConvertJsonElementToArray(item));
                        break;
                    case JsonValueKind.String:
                        list.Add(item.GetString() ?? string.Empty);
                        break;
                    case JsonValueKind.Number:
                        if (item.TryGetInt32(out int intValue))
                            list.Add(intValue);
                        else if (item.TryGetDouble(out double doubleValue))
                            list.Add(doubleValue);
                        break;
                    case JsonValueKind.True:
                        list.Add(true);
                        break;
                    case JsonValueKind.False:
                        list.Add(false);
                        break;
                }
            }

            return list.ToArray();
        }

        public static T GetValue<T>(Dictionary<string, object> dict, string key, T defaultValue = default!)
        {
            if (dict.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return defaultValue;
        }

        public static Dictionary<string, object> GetNestedDict(Dictionary<string, object> dict, string key)
        {
            return GetValue<Dictionary<string, object>>(dict, key) ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Flattens a nested dictionary into a collection of key-value pairs using a specified separator.
        /// This is useful for converting hierarchical configuration into flat app settings.
        /// </summary>
        /// <param name="nestedDictionary">The nested dictionary to flatten</param>
        /// <param name="separator">The separator to use between levels (default: "__")</param>
        /// <returns>A dictionary of flattened key-value pairs</returns>
        public static Dictionary<string, string> FlattenDictionary(Dictionary<string, object> nestedDictionary, string separator = "__")
        {
            var flattened = new Dictionary<string, string>();
            FlattenDictionaryRecursive(flattened, nestedDictionary, "", separator);
            return flattened;
        }

        /// <summary>
        /// Adds flattened configuration values to a list of key-value pairs.
        /// This is a generic helper that can be used with any key-value pair type.
        /// </summary>
        /// <typeparam name="T">The type of key-value pair to create</typeparam>
        /// <param name="target">The list to add the flattened pairs to</param>
        /// <param name="nestedDictionary">The nested dictionary to flatten</param>
        /// <param name="keySelector">Function to create a key-value pair from name and value</param>
        /// <param name="separator">The separator to use between levels (default: "__")</param>
        public static void AddFlattenedSettings<T>(
            ICollection<T> target,
            Dictionary<string, object> nestedDictionary,
            Func<string, string, T> keySelector,
            string separator = "__")
        {
            var flattened = FlattenDictionary(nestedDictionary, separator);
            foreach (var kvp in flattened)
            {
                target.Add(keySelector(kvp.Key, kvp.Value));
            }
        }

        private static void FlattenDictionaryRecursive(
            Dictionary<string, string> result,
            Dictionary<string, object> source,
            string prefix,
            string separator)
        {
            foreach (var kvp in source)
            {
                var key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}{separator}{kvp.Key}";

                if (kvp.Value is Dictionary<string, object> nestedDict)
                {
                    FlattenDictionaryRecursive(result, nestedDict, key, separator);
                }
                else
                {
                    result[key] = kvp.Value?.ToString() ?? "";
                }
            }
        }
    }
}