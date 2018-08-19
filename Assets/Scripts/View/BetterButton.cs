using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class BetterButton : MonoBehaviour
    {
        public Button Button;
        public TextMeshProUGUI Label;

        private Color _textOriginalColor;

        private void Awake()
        {
            _textOriginalColor = Label.color;
        }

        public void SetInteractable(bool value)
        {
            Button.interactable = value;
            Label.color = value ? _textOriginalColor : Button.colors.disabledColor;
        }
    }
}