using SeaBattle.Common.Messages;
using UnityEngine;

namespace SeaBattle.Utilities.Json
{
    public static class WireJson
    {
        public static string Pack<T>(string type, T payload) =>
            JsonUtility.ToJson(new WireEnvelope
            {
                Type = type,
                Payload = JsonUtility.ToJson(payload)
            });

        public static WireEnvelope Unpack(string json) => JsonUtility.FromJson<WireEnvelope>(json);
    }
}
