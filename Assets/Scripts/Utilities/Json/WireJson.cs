using SeaBattle.Common.Messages;
using UnityEngine;

namespace SeaBattle.Utilities.Json
{
    public static class WireJson
    {
        /// <summary>
        /// Packs a message into one JSON string the transport can queue.
        /// </summary>
        public static string Pack<T>(string type, T payload)
        {
            return JsonUtility.ToJson(new WireEnvelope
            {
                Type = type,
                Payload = JsonUtility.ToJson(payload)
            });
        }

        /// <summary>
        /// Reads the type and payload back out of a packed message.
        /// </summary>
        public static WireEnvelope Unpack(string json) => JsonUtility.FromJson<WireEnvelope>(json);
    }
}
