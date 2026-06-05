using UnityEngine;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class RuleReconstructionBlockButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Outline selectionOutline;
        [SerializeField] private RuleReconstructionRulePreview directionPreview;
        [SerializeField] private RuleReconstructionRangeIcon rangeIcon;
        [SerializeField] private Image effectIconImage;

        public Button Button => button;
        public Outline SelectionOutline => selectionOutline;
        public RuleReconstructionRulePreview DirectionPreview => directionPreview;
        public RuleReconstructionRangeIcon RangeIcon => rangeIcon;
        public Image EffectIconImage => effectIconImage;

        public void Bind(
            Button button,
            Outline selectionOutline,
            RuleReconstructionRulePreview directionPreview,
            RuleReconstructionRangeIcon rangeIcon,
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
