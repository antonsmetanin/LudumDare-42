using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class FillBar : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;

        public void SetValue(float current, float max) => _fillImage.fillAmount = current / max;
    }
}