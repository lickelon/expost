using UnityEngine;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    [DisallowMultipleComponent]
    public sealed class RuleReconstructionView : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform dynamicSidebarRoot;
        [SerializeField] private RectTransform boardPanel;
        [SerializeField] private RectTransform boardRoot;
        [SerializeField] private RectTransform ruleListRoot;
        [SerializeField] private RectTransform directionBlockRoot;
        [SerializeField] private RectTransform rangeBlockRoot;
        [SerializeField] private RectTransform effectBlockRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text boardTitleText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text analysisText;
        [SerializeField] private Text resultBannerText;
        [SerializeField] private Image resultBannerIcon;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button testButton;
        [SerializeField] private Button targetButton;
        [SerializeField] private RuleReconstructionBoardCell boardCellPrefab;
        [SerializeField] private RuleReconstructionRuleCard ruleCardPrefab;
        [SerializeField] private RuleReconstructionBlockButton directionBlockPrefab;
        [SerializeField] private RuleReconstructionBlockButton rangeBlockPrefab;
        [SerializeField] private RuleReconstructionBlockButton effectBlockPrefab;

        public Canvas Canvas => canvas;
        public RectTransform DynamicSidebarRoot => dynamicSidebarRoot;
        public RectTransform BoardPanel => boardPanel;
        public RectTransform BoardRoot => boardRoot;
        public RectTransform RuleListRoot => ruleListRoot;
        public RectTransform DirectionBlockRoot => directionBlockRoot;
        public RectTransform RangeBlockRoot => rangeBlockRoot;
        public RectTransform EffectBlockRoot => effectBlockRoot;
        public Text TitleText => titleText;
        public Text BoardTitleText => boardTitleText;
        public Text StatusText => statusText;
        public Text AnalysisText => analysisText;
        public Text ResultBannerText => resultBannerText;
        public Image ResultBannerIcon => resultBannerIcon;
        public Button PrevButton => prevButton;
        public Button NextButton => nextButton;
        public Button TestButton => testButton;
        public Button TargetButton => targetButton;
        public RuleReconstructionBoardCell BoardCellPrefab => boardCellPrefab;
        public RuleReconstructionRuleCard RuleCardPrefab => ruleCardPrefab;
        public RuleReconstructionBlockButton DirectionBlockPrefab => directionBlockPrefab;
        public RuleReconstructionBlockButton RangeBlockPrefab => rangeBlockPrefab;
        public RuleReconstructionBlockButton EffectBlockPrefab => effectBlockPrefab;

        public bool HasRequiredReferences()
        {
            return canvas != null
                && dynamicSidebarRoot != null
                && boardPanel != null
                && boardRoot != null
                && ruleListRoot != null
                && directionBlockRoot != null
                && rangeBlockRoot != null
                && effectBlockRoot != null
                && titleText != null
                && boardTitleText != null
                && statusText != null
                && analysisText != null
                && resultBannerText != null
                && resultBannerIcon != null
                && prevButton != null
                && nextButton != null
                && testButton != null
                && targetButton != null
                && boardCellPrefab != null
                && ruleCardPrefab != null
                && directionBlockPrefab != null
                && rangeBlockPrefab != null
                && effectBlockPrefab != null;
        }

        public void RefreshStaticButtonVisuals()
        {
            if (prevButton != null)
            {
                RuleReconstructionIconFactory.ApplyToButton(prevButton, ButtonIconKind.Previous, 18f);
            }

            if (nextButton != null)
            {
                RuleReconstructionIconFactory.ApplyToButton(nextButton, ButtonIconKind.Next, 18f);
            }

            if (testButton != null)
            {
                RuleReconstructionIconFactory.ApplyToButton(testButton, ButtonIconKind.Run, 22f);
            }

            if (targetButton != null)
            {
                RuleReconstructionIconFactory.ApplyToButton(targetButton, ButtonIconKind.Target, 22f);
            }
        }

        public void Bind(
            Canvas canvas,
            RectTransform dynamicSidebarRoot,
            RectTransform boardPanel,
            RectTransform boardRoot,
            RectTransform ruleListRoot,
            RectTransform directionBlockRoot,
            RectTransform rangeBlockRoot,
            RectTransform effectBlockRoot,
            Text titleText,
            Text boardTitleText,
            Text statusText,
            Text analysisText,
            Text resultBannerText,
            Image resultBannerIcon,
            Button prevButton,
            Button nextButton,
            Button testButton,
            Button targetButton,
            RuleReconstructionBoardCell boardCellPrefab,
            RuleReconstructionRuleCard ruleCardPrefab,
            RuleReconstructionBlockButton directionBlockPrefab,
            RuleReconstructionBlockButton rangeBlockPrefab,
            RuleReconstructionBlockButton effectBlockPrefab)
        {
            this.canvas = canvas;
            this.dynamicSidebarRoot = dynamicSidebarRoot;
            this.boardPanel = boardPanel;
            this.boardRoot = boardRoot;
            this.ruleListRoot = ruleListRoot;
            this.directionBlockRoot = directionBlockRoot;
            this.rangeBlockRoot = rangeBlockRoot;
            this.effectBlockRoot = effectBlockRoot;
            this.titleText = titleText;
            this.boardTitleText = boardTitleText;
            this.statusText = statusText;
            this.analysisText = analysisText;
            this.resultBannerText = resultBannerText;
            this.resultBannerIcon = resultBannerIcon;
            this.prevButton = prevButton;
            this.nextButton = nextButton;
            this.testButton = testButton;
            this.targetButton = targetButton;
            this.boardCellPrefab = boardCellPrefab;
            this.ruleCardPrefab = ruleCardPrefab;
            this.directionBlockPrefab = directionBlockPrefab;
            this.rangeBlockPrefab = rangeBlockPrefab;
            this.effectBlockPrefab = effectBlockPrefab;
        }
    }
}
