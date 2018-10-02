using Newtonsoft.Json;

namespace FS.Core.Utils
{
    public static class Extensions
    {
        public static string Serialize<T>(this T instance)
        {
            string result = string.Empty;

            if (instance != null)
            {
                result = JsonConvert.SerializeObject(instance, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            }

            return result;
        }
    }
}
