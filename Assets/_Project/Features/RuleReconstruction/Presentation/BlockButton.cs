using UnityEngine;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class BlockButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Outline selectionOutline;
        [SerializeField] private RulePreview directionPreview;
        [SerializeField] private RangeIcon rangeIcon;
        [SerializeField] private Image effectIconImage;

        public Button Button => button;
        public Outline SelectionOutline => selectionOutline;
        public RulePreview DirectionPreview => directionPreview;
        public RangeIcon RangeIcon => rangeIcon;
        public Image EffectIconImage => effectIconImage;

        public void Bind(
            Button button,
            Outline selectionOutline,
            RulePreview directionPreview,
            RangeIcon rangeIcon,
            Image effectIconImage)
        {
            this.button = button;
            this.selectionOutline = selectionOutline;
            this.directionPreview = directionPreview;
            this.rangeIcon = rangeIcon;
            this.effectIconImage = effectIconImage;
        }
    }
}
