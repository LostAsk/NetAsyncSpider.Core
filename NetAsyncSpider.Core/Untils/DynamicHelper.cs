using System;
using System.Collections.Generic;
using System.Text;
using System.Dynamic;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace NetAsyncSpider.Core.Untils
{
    internal static class DynamicHelper
    {
        public static ExpandoObject GetExpandoObjectByAppsetting(this IConfiguration configuration, string selection_node)
        {
            var result = new ExpandoObject();
            // retrieve all keys from your settings
            var configs = configuration.AsEnumerable().Where(_ => _.Key.StartsWith(selection_node)).ToList();
            if (configs.Count == 0) return null;
            foreach (var kvp in configs)
            {
                var parent = result as IDictionary<string, object>;
                var path = kvp.Key.Split(':');
                // create or retrieve the hierarchy (keep last path item for later)
                var i = 0;
                for (i = 0; i < path.Length - 1; i++)
                {
                    if (!parent.ContainsKey(path[i]))
                    {
                        parent.Add(path[i], new ExpandoObject());
                    }
                    parent = parent[path[i]] as IDictionary<string, object>;
                }
                if (kvp.Value == null)
                    continue;
                var key = path[i];

                parent.Add(key, kvp.Value);

            }
            
            ReplaceWithArray(null, null, result);
            return result;
        }
        private static void ReplaceWithArray(ExpandoObject parent, string key, ExpandoObject input)
        {
            if (input == null)
                return;

            var dict = input as IDictionary<string, object>;
            var keys = dict.Keys.ToArray();

            // it's an array if all keys are integers
            if (keys.All(k => int.TryParse(k, out var dummy)))
            {
                var array = new object[keys.Length];
                foreach (var kvp in dict)
                {
                    array[int.Parse(kvp.Key)] = kvp.Value;
                    // Edit: If structure is nested deeper we need this next line 
                    ReplaceWithArray(input, kvp.Key, kvp.Value as ExpandoObject);
                }

                var parentDict = parent as IDictionary<string, object>;
                parentDict.Remove(key);
                parentDict.Add(key, array);
            }
            else
            {
                foreach (var childKey in dict.Keys.ToList())
                {
                    ReplaceWithArray(input, childKey, dict[childKey] as ExpandoObject);
                }
            }
        }
    }
}