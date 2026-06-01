using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Expost.RuleReconstruction.Editor
{
    public static class RuleReconstructionSceneViewBuilder
    {
        private static readonly Color PageColor = new(0.18f, 0.29f, 0.47f);
        private static readonly Color PanelColor = new(0.13f, 0.23f, 0.39f);
        private static readonly Color ButtonColor = new(0.28f, 0.37f, 0.50f);

        [MenuItem("GameObject/Rule Reconstruction/Game View", false, 10)]
        public static void CreateGameView()
        {
            EnsureEventSystem();

            var gameObject = new GameObject("Rule Reconstruction Game");
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Rule Reconstruction Game");
            var game = gameObject.AddComponent<RuleReconstructionGame>();

            var canvasObject = new GameObject("Rule Reconstruction Canvas");
            canvasObject.transform.SetParent(gameObject.transform, false);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960f, 540f);
            scaler.matchWidthOrHeight = 0.5f;

            var background = canvasObject.AddComponent<Image>();
            background.color = PageColor;

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = CreatePanel("Root", canvasObject.transform, PageColor);
            Stretch(root, Vector2.zero, Vector2.one, new Vector2(18f, 12f), new Vector2(-18f, -12f));

            var titleText = CreateText("Title", root, string.Empty, 22, TextAnchor.MiddleLeft, font);
            Anchor(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -34f), new Vector2(0f, 0f));

            var prevButton = CreateButton("PrevButton", root, "Prev", 16, font);
            Anchor(prevButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-160f, -34f), new Vector2(-84f, 0f));

            var nextButton = CreateButton("NextButton", root, "Next", 16, font);
            Anchor(nextButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-76f, -34f), new Vector2(0f, 0f));

            var sidebar = CreatePanel("DynamicSidebarRoot", root, PanelColor);
            Anchor(sidebar, new Vector2(0f, 0f), new Vector2(0.33f, 1f), new Vector2(0f, 0f), new Vector2(-10f, -46f));

            var boardPanel = CreatePanel("BoardPanel", root, PanelColor);
            Anchor(boardPanel, new Vector2(0.33f, 0f), new Vector2(1f, 1f), new Vector2(10f, 0f), new Vector2(0f, -46f));

            var boardTitleText = CreateText("BoardTitle", boardPanel, string.Empty, 22, TextAnchor.MiddleLeft, font);
            Anchor(boardTitleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -42f), new Vector2(-120f, -8f));

            var resultBannerText = CreateText("ResultBanner", boardPanel, string.Empty, 54, TextAnchor.MiddleCenter, font);
            Anchor(resultBannerText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0f, 26f), new Vector2(0f, -26f));
            resultBannerText.fontStyle = FontStyle.Bold;
            resultBannerText.raycastTarget = false;
            var bannerOutline = resultBannerText.gameObject.AddComponent<Outline>();
            bannerOutline.effectColor = new Color(0f, 0f, 0f, 0.72f);
            bannerOutline.effectDistance = new Vector2(2f, -2f);

            var actionRoot = CreatePanel("StaticActions", root, Color.clear);
            actionRoot.GetComponent<Image>().raycastTarget = false;
            Anchor(actionRoot, new Vector2(0f, 0f), new Vector2(0.33f, 0f), new Vector2(14f, 12f), new Vector2(-24f, 188f));

            var testButton = CreateButton("TestButton", actionRoot, string.Empty, 16, font);
            Anchor(testButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(-4f, -108f));

            var targetButton = CreateButton("TargetButton", actionRoot, string.Empty, 16, font);
            Anchor(targetButton.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(4f, -160f), new Vector2(0f, -108f));

            var statusText = CreateText("Status", actionRoot, string.Empty, 15, TextAnchor.MiddleLeft, font);
            Anchor(statusText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 18f));

            var analysisText = CreateText("StageAnalysis", actionRoot, string.Empty, 12, TextAnchor.MiddleLeft, font);
            analysisText.gameObject.SetActive(false);

            var view = canvasObject.AddComponent<RuleReconstructionView>();
            view.Bind(
                canvas,
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

            var serializedGame = new SerializedObject(game);
            serializedGame.FindProperty("view").objectReferenceValue = view;
            serializedGame.FindProperty("buildRuntimeLayoutWhenMissing").boolValue = false;
            serializedGame.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = gameObject;
            EditorUtility.SetDirty(game);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem");
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        private static RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            var rectTransform = gameObject.AddComponent<RectTransform>();
            var image = gameObject.AddComponent<Image>();
            image.color = color;
            return rectTransform;
        }

        private static Text CreateText(string name, Transform parent, string text, int fontSize, TextAnchor alignment, Font font)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            var label = gameObject.AddComponent<Text>();
            label.font = font;
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        private static Button CreateButton(string name, Transform parent, string label, int fontSize, Font font)
        {
            var rectTransform = CreatePanel(name, parent, ButtonColor);
            var button = rectTransform.gameObject.AddComponent<Button>();
            button.targetGraphic = rectTransform.GetComponent<Image>();

            var text = CreateText("Text", rectTransform, label, fontSize, TextAnchor.MiddleCenter, font);
            Stretch(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private static void Stretch(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        private static void Anchor(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            Stretch(rectTransform, anchorMin, anchorMax, offsetMin, offsetMax);
        }
    }
}
