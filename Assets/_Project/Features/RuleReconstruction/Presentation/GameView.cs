using UnityEngine;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    [DisallowMultipleComponent]
    public sealed class GameView : MonoBehaviour
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
        [SerializeField] private BoardCell boardCellPrefab;
        [SerializeField] private RuleCard ruleCardPrefab;
        [SerializeField] private BlockButton directionBlockPrefab;
        [SerializeField] private BlockButton rangeBlockPrefab;
        [SerializeField] private BlockButton effectBlockPrefab;
        [SerializeField] private Color redSourceColor;
        [SerializeField] private Color blueSourceColor;
        [SerializeField] private Color greenSourceColor;
        [SerializeField] private Color yellowSourceColor;
        [SerializeField] private Color numberTextColor;
        [SerializeField] private Color affectedTextColor;
        [SerializeField] private Color wrongTextColor;
        [SerializeField] private Color needMoreTextColor;
        [SerializeField] private Color clearTextColor;
        [SerializeField] private Color previewIdleColor;

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
        public BoardCell BoardCellPrefab => boardCellPrefab;
        public RuleCard RuleCardPrefab => ruleCardPrefab;
        public BlockButton DirectionBlockPrefab => directionBlockPrefab;
        public BlockButton RangeBlockPrefab => rangeBlockPrefab;
        public BlockButton EffectBlockPrefab => effectBlockPrefab;
        public Color NumberTextColor => numberTextColor;
        public Color AffectedTextColor => affectedTextColor;
        public Color WrongTextColor => wrongTextColor;
        public Color NeedMoreTextColor => needMoreTextColor;
        public Color ClearTextColor => clearTextColor;
        public Color PreviewIdleColor => previewIdleColor;

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

        public Color GetSourceColor(BoxColor color)
        {
            return color switch
            {
                BoxColor.Red => redSourceColor,
                BoxColor.Blue => blueSourceColor,
                BoxColor.Green => greenSourceColor,
                BoxColor.Yellow => yellowSourceColor,
                _ => Color.white
            };
        }

        public void RefreshStaticButtonVisuals()
        {
            if (prevButton != null)
            {
                IconFactory.ApplyToButton(prevButton, ButtonIconKind.Previous);
            }

            if (nextButton != null)
            {
                IconFactory.ApplyToButton(nextButton, ButtonIconKind.Next);
            }

            if (testButton != null)
            {
                IconFactory.ApplyToButton(testButton, ButtonIconKind.Run);
            }

            if (targetButton != null)
            {
                IconFactory.ApplyToButton(targetButton, ButtonIconKind.Target);
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
            BoardCell boardCellPrefab,
            RuleCard ruleCardPrefab,
            BlockButton directionBlockPrefab,
            BlockButton rangeBlockPrefab,
            BlockButton effectBlockPrefab)
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
