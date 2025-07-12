using Newtonsoft.Json;

namespace DnDClient.Services;

public class Serdeser
{
    public static string Serialize(Object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    public static T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}