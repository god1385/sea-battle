using System.Collections.Generic;
using UniRx;

namespace SeaBattle.Common.Debug
{
    public class NetworkLog
    {
        private readonly List<string> _lines = new List<string>();
        private readonly ReactiveProperty<string> _text = new ReactiveProperty<string>(string.Empty);

        public bool Enabled { get; set; }

        public IReadOnlyReactiveProperty<string> Lines => _text;

        /// <summary>
        /// Appends one traffic line when the log is enabled.
        /// </summary>
        public void Append(string line)
        {
            if (!Enabled)
                return;

            _lines.Add(line);
            if (_lines.Count > 80)
                _lines.RemoveAt(0);

            _text.Value = string.Join("\n", _lines);
        }
    }
}
