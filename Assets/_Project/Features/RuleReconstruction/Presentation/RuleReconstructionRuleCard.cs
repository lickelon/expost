using UnityEngine;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class RuleReconstructionRuleCard : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image panelImage;
        [SerializeField] private Outline panelOutline;
        [SerializeField] private Image selectionIndicator;
        [SerializeField] private Image sourceSlotImage;
        [SerializeField] private RuleReconstructionRulePreview directionPreview;
        [SerializeField] private RuleReconstructionRangeIcon rangeIcon;
        [SerializeField] private Image effectIconImage;

        public Button Button => button;
        public Image PanelImage => panelImage;
        public Outline PanelOutline => panelOutline;
        public Image SelectionIndicator => selectionIndicator;
        public Image SourceSlotImage => sourceSlotImage;
        public RulePreviewView DirectionPreview => directionPreview.ToView();
        public RangeIconView RangeIcon => rangeIcon.ToView();
        public Image EffectIconImage => effectIconImage;

        public void Bind(
            Button button,
            Image panelImage,
            Outline panelOutline,
            Image selectionIndicator,
            Image sourceSlotImage,
            RuleReconstructionRulePreview directionPreview,
            RuleReconstructionRangeIcon rangeIcon,
            Image effectIconImage)
        {
            this.button = button;
            this.panelImage = panelImage;
            this.panelOutline = panelOutline;
            this.selectionIndicator = selectionIndicator;
            this.sourceSlotImage = sourceSlotImage;
            this.directionPreview = directionPreview;
            this.rangeIcon = rangeIcon;
            this.effectIconImage = effectIconImage;
        }
    }
}
