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
        [SerializeField] private Text titleText;
        [SerializeField] private Text boardTitleText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text analysisText;
        [SerializeField] private Text resultBannerText;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button testButton;
        [SerializeField] private Button targetButton;

        public Canvas Canvas => canvas;
        public RectTransform DynamicSidebarRoot => dynamicSidebarRoot;
        public RectTransform BoardPanel => boardPanel;
        public Text TitleText => titleText;
        public Text BoardTitleText => boardTitleText;
        public Text StatusText => statusText;
        public Text AnalysisText => analysisText;
        public Text ResultBannerText => resultBannerText;
        public Button PrevButton => prevButton;
        public Button NextButton => nextButton;
        public Button TestButton => testButton;
        public Button TargetButton => targetButton;

        public bool HasRequiredReferences()
        {
            return canvas != null
                && dynamicSidebarRoot != null
                && boardPanel != null
                && titleText != null
                && boardTitleText != null
                && statusText != null
                && analysisText != null
                && resultBannerText != null
                && prevButton != null
                && nextButton != null
                && testButton != null
                && targetButton != null;
        }

        public void Bind(
            Canvas canvas,
            RectTransform dynamicSidebarRoot,
            RectTransform boardPanel,
            Text titleText,
            Text boardTitleText,
            Text statusText,
            Text analysisText,
            Text resultBannerText,
            Button prevButton,
            Button nextButton,
            Button testButton,
            Button targetButton)
        {
            this.canvas = canvas;
            this.dynamicSidebarRoot = dynamicSidebarRoot;
            this.boardPanel = boardPanel;
            this.titleText = titleText;
            this.boardTitleText = boardTitleText;
            this.statusText = statusText;
            this.analysisText = analysisText;
            this.resultBannerText = resultBannerText;
            this.prevButton = prevButton;
            this.nextButton = nextButton;
            this.testButton = testButton;
            this.targetButton = targetButton;
        }
    }
}
