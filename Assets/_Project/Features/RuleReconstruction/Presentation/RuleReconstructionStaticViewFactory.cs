using UnityEngine;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    public readonly struct RuleReconstructionStaticView
    {
        public readonly RectTransform Sidebar;
        public readonly RectTransform BoardPanel;
        public readonly Text TitleText;
        public readonly Text BoardTitleText;
        public readonly Text StatusText;
        public readonly Text AnalysisText;
        public readonly Text ResultBannerText;
        public readonly Button PrevButton;
        public readonly Button NextButton;
        public readonly Button TestButton;
        public readonly Button TargetButton;

        public RuleReconstructionStaticView(
            RectTransform sidebar,
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
            Sidebar = sidebar;
            BoardPanel = boardPanel;
            TitleText = titleText;
            BoardTitleText = boardTitleText;
            StatusText = statusText;
            AnalysisText = analysisText;
            ResultBannerText = resultBannerText;
            PrevButton = prevButton;
            NextButton = nextButton;
            TestButton = testButton;
            TargetButton = targetButton;
        }
    }

    public static class RuleReconstructionStaticViewFactory
    {
        public static RuleReconstructionStaticView Create(Canvas canvas, RuleReconstructionUiFactory ui, Color pageColor, Color panelColor)
        {
            var root = ui.CreatePanel("Root", canvas.transform, pageColor);
            RuleReconstructionUiFactory.Stretch(root, Vector2.zero, Vector2.one, new Vector2(18f, 12f), new Vector2(-18f, -12f));

            var titleText = ui.CreateText("Title", root, string.Empty, 22, TextAnchor.MiddleLeft);
            RuleReconstructionUiFactory.Anchor(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -34f), new Vector2(0f, 0f));

            var prevButton = ui.CreateButton("PrevButton", root, string.Empty, 16, null);
            RuleReconstructionUiFactory.Anchor(prevButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-160f, -34f), new Vector2(-84f, 0f));

            var nextButton = ui.CreateButton("NextButton", root, string.Empty, 16, null);
            RuleReconstructionUiFactory.Anchor(nextButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-76f, -34f), new Vector2(0f, 0f));

            var sidebar = ui.CreatePanel("DynamicSidebarRoot", root, panelColor);
            RuleReconstructionUiFactory.Anchor(sidebar, new Vector2(0f, 0f), new Vector2(0.33f, 1f), new Vector2(0f, 0f), new Vector2(-10f, -46f));

            var boardPanel = ui.CreatePanel("BoardPanel", root, panelColor);
            RuleReconstructionUiFactory.Anchor(boardPanel, new Vector2(0.33f, 0f), new Vector2(1f, 1f), new Vector2(10f, 0f), new Vector2(0f, -46f));

            var boardTitleText = ui.CreateText("BoardTitle", boardPanel, string.Empty, 22, TextAnchor.MiddleLeft);
            RuleReconstructionUiFactory.Anchor(boardTitleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -42f), new Vector2(-120f, -8f));

            var resultBannerText = ui.CreateText("ResultBanner", boardPanel, string.Empty, 54, TextAnchor.MiddleCenter);
            RuleReconstructionUiFactory.Anchor(resultBannerText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0f, 26f), new Vector2(0f, -26f));
            resultBannerText.fontStyle = FontStyle.Bold;
            resultBannerText.raycastTarget = false;
            var bannerOutline = resultBannerText.gameObject.AddComponent<Outline>();
            bannerOutline.effectColor = new Color(0f, 0f, 0f, 0.72f);
            bannerOutline.effectDistance = new Vector2(2f, -2f);

            var actionRoot = ui.CreatePanel("StaticActions", root, Color.clear);
            actionRoot.GetComponent<Image>().raycastTarget = false;
            RuleReconstructionUiFactory.Anchor(actionRoot, new Vector2(0f, 0f), new Vector2(0.33f, 0f), new Vector2(14f, 12f), new Vector2(-24f, 188f));

            var testButton = ui.CreateButton("TestButton", actionRoot, string.Empty, 16, null);
            var targetButton = ui.CreateButton("TargetButton", actionRoot, string.Empty, 16, null);
            RuleReconstructionLayout.AnchorActionButtons(testButton, targetButton);

            var statusText = ui.CreateText("Status", actionRoot, string.Empty, 15, TextAnchor.MiddleLeft);
            RuleReconstructionUiFactory.Anchor(statusText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 18f));

            var analysisText = ui.CreateText("StageAnalysis", actionRoot, string.Empty, 12, TextAnchor.MiddleLeft);
            analysisText.gameObject.SetActive(false);

            return new RuleReconstructionStaticView(
                sidebar,
                boardPanel,
                titleText,
                boardTitleText,
                statusText,
                analysisText,
                resultBannerText,
                prevButton,
                nextButton,
                testButton,
                targetButton);
        }
    }
}
