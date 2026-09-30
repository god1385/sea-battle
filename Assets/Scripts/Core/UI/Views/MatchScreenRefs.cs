using UnityEngine;
using UnityEngine.UI;

namespace SeaBattle.Core.UI.Views
{
    public class MatchScreenRefs : MonoBehaviour
    {
        [SerializeField] private Button restart;
        [SerializeField] private Toggle logToggle;
        [SerializeField] private Text log;
        [SerializeField] private PlayerColumnRefs first;
        [SerializeField] private PlayerColumnRefs second;

        public Button RestartButton => restart;

        public Toggle LogToggle => logToggle;

        public Text Log => log;

        public PlayerColumnRefs First => first;

        public PlayerColumnRefs Second => second;

        public void Assign(
            Button restartButton,
            Toggle toggle,
            Text logText,
            PlayerColumnRefs firstColumn,
            PlayerColumnRefs secondColumn)
        {
            restart = restartButton;
            logToggle = toggle;
            log = logText;
            first = firstColumn;
            second = secondColumn;
        }

        public void ApplyConfig(bool logEnabled, int delayMilliseconds, int lossPercent)
        {
            logToggle.isOn = logEnabled;
            first.SetDelay(delayMilliseconds);
            second.SetDelay(delayMilliseconds);
            first.SetLoss(lossPercent);
            second.SetLoss(lossPercent);
        }
    }
}
