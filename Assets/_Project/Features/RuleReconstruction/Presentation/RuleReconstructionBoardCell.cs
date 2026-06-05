using UnityEngine;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    public sealed class RuleReconstructionBoardCell : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Text label;

        public Image Background => background;
        public Text Label => label;

        public bool HasRequiredReferences()
        {
            return background != null && label != null;
        }

        public void Bind(Image background, Text label)
        {
            this.background = background;
            this.label = label;
        }

        private void Reset()
        {
            background = GetComponent<Image>();
            label = GetComponentInChildren<Text>(true);
        }

        private void OnValidate()
        {
            if (background == null)
            {
                background = GetComponent<Image>();
            }

            if (label == null)
            {
                label = GetComponentInChildren<Text>(true);
            }
        }
    }
}
