using SeaBattle.Core.Game;
using UnityEngine;

namespace SeaBattle.Common.Config
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Sea Battle/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [SerializeField] private int width = 6;
        [SerializeField] private int height = 6;
        [SerializeField] private int[] shipLengths = { 3, 2, 2, 1 };
        [SerializeField] private int deliveryDelayMilliseconds = 400;
        [SerializeField] private bool messageLogEnabled = false;

        public int DeliveryDelayMilliseconds => deliveryDelayMilliseconds;

        public bool MessageLogEnabled => messageLogEnabled;

        public GameRules CreateRules() => new GameRules(width, height, shipLengths);
    }
}
