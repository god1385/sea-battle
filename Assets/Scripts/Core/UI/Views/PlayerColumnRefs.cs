using UnityEngine;
using UnityEngine.UI;

namespace SeaBattle.Core.UI.Views
{
    public class PlayerColumnRefs : MonoBehaviour
    {
        [SerializeField] private Text status;
        [SerializeField] private CellView[] ownCells;
        [SerializeField] private CellView[] enemyCells;
        [SerializeField] private Button disconnect;
        [SerializeField] private Button connect;
        [SerializeField] private InputField delay;

        public Text Status => status;

        public CellView[] OwnCells => ownCells;

        public CellView[] EnemyCells => enemyCells;

        public Button Disconnect => disconnect;

        public Button Connect => connect;

        public InputField Delay => delay;

        /// <summary>
        /// Wires the column after the scene builder creates its controls and grids.
        /// </summary>
        public void Assign(
            Text statusText,
            CellView[] own,
            CellView[] enemy,
            Button disconnectButton,
            Button connectButton,
            InputField delayField)
        {
            status = statusText;
            ownCells = own;
            enemyCells = enemy;
            disconnect = disconnectButton;
            connect = connectButton;
            delay = delayField;
        }

        /// <summary>
        /// Shows the configured delivery delay in the field.
        /// </summary>
        public void SetDelay(int milliseconds) => delay.text = milliseconds.ToString();
    }
}
