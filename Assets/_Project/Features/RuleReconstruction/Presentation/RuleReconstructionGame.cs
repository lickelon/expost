using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Rule Reconstruction/Rule Reconstruction Game")]
    public sealed class RuleReconstructionGame : MonoBehaviour
    {
        private static readonly BoxColor[] AllColors =
        {
            BoxColor.Red,
            BoxColor.Blue,
            BoxColor.Green,
            BoxColor.Yellow
        };

        private readonly Dictionary<BoxColor, Image> sourceSlotImages = new();
        private readonly Dictionary<BoxColor, RulePreviewView> previewViews = new();
        private readonly Dictionary<BoxColor, RangeIconView> rangeIconViews = new();
        private readonly Dictionary<BoxColor, Text> effectIconTexts = new();
        private readonly Dictionary<BoxColor, Image> rulePanelImages = new();
        private readonly Dictionary<BoxColor, Outline> rulePanelOutlines = new();
        private readonly Dictionary<DirectionType, RulePreviewView> directionBlockPreviews = new();
        private readonly Dictionary<DirectionType, Outline> directionBlockOutlines = new();
        private readonly Dictionary<RangeType, Outline> rangeBlockOutlines = new();
        private readonly Dictionary<EffectType, Outline> effectBlockOutlines = new();
        private readonly List<BoardCellView> boardCells = new();

        private RuleReconstructionSession session;
        private BoardState displayBoard;
        private HashSet<GridPosition> activeAffectedCells = new();
        private Coroutine runRoutine;
        [SerializeField] private RuleReconstructionView view;
        [SerializeField] private bool buildRuntimeLayoutWhenMissing = true;
        private BoxColor selectedRuleColor;
        private bool isRunning;
        private bool showResult;
        private bool showMismatch;
        private bool showResultBanner;
        private bool usingSceneView;
        private Canvas canvas;
        private RectTransform sidebar;
        private RectTransform boardPanel;
        private RectTransform boardRoot;
        private Text titleText;
        private Text boardTitleText;
        private Text statusText;
        private Text analysisText;
        private Text resultBannerText;
        private Button prevButton;
        private Button nextButton;
        private Button testButton;
        private Button targetButton;
        private Font uiFont;
        private RuleReconstructionUiFactory ui;

        private readonly Color pageColor = new(0.18f, 0.29f, 0.47f);
        private readonly Color panelColor = new(0.13f, 0.23f, 0.39f);
        private readonly Color cellColor = new(0.16f, 0.17f, 0.19f);
        private readonly Color buttonColor = new(0.28f, 0.37f, 0.50f);
        private readonly Color rulePanelColor = new(0.17f, 0.29f, 0.48f);
        private readonly Color selectedRuleOutlineColor = new(0.54f, 0.93f, 1f);
        private readonly Color directionSlotOutlineColor = new(0.54f, 0.93f, 1f);
        private readonly Color rangeSlotOutlineColor = new(1f, 0.86f, 0.20f);
        private readonly Color effectSlotOutlineColor = new(0.35f, 0.95f, 0.56f);
        private readonly Color affectedTextColor = new(0.54f, 0.93f, 1f);
        private readonly Color wrongTextColor = new(1f, 0.86f, 0.20f);
        private readonly Color clearTextColor = new(0.35f, 0.95f, 0.56f);

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (session != null)
            {
                return;
            }

            session = new RuleReconstructionSession(StageRepository.LoadStages(), AllColors);
            selectedRuleColor = StageRuleAnalyzer.GetStageColors(CurrentStage)[0];
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ui = new RuleReconstructionUiFactory(uiFont, buttonColor);

            usingSceneView = TryBindSceneView();
            if (!usingSceneView)
            {
                if (!buildRuntimeLayoutWhenMissing)
                {
                    Debug.LogError("RuleReconstructionGame requires a bound RuleReconstructionView.", this);
                    return;
                }

                CreateCanvas();
                BuildLayout();
            }
            else
            {
                BuildSidebar();
                BuildBoardCells();
            }

            WireStaticButtons();
            ResetDisplay();
        }

        private bool TryBindSceneView()
        {
            if (view == null)
            {
                view = GetComponentInChildren<RuleReconstructionView>(true);
            }

            if (view == null || !view.HasRequiredReferences())
            {
                return false;
            }

            canvas = view.Canvas;
            sidebar = view.DynamicSidebarRoot;
            boardPanel = view.BoardPanel;
            titleText = view.TitleText;
            boardTitleText = view.BoardTitleText;
            statusText = view.StatusText;
            analysisText = view.AnalysisText;
            resultBannerText = view.ResultBannerText;
            prevButton = view.PrevButton;
            nextButton = view.NextButton;
            testButton = view.TestButton;
            targetButton = view.TargetButton;
            DisableStaticActionPanelRaycast();
            return true;
        }

        private void DisableStaticActionPanelRaycast()
        {
            var staticActions = canvas.transform.Find("Root/StaticActions");
            if (staticActions == null)
            {
                return;
            }

            var image = staticActions.GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = false;
            }
        }

        private void WireStaticButtons()
        {
            if (view == null || !view.HasRequiredReferences())
            {
                return;
            }

            view.PrevButton.onClick.RemoveAllListeners();
            view.PrevButton.onClick.AddListener(() => MoveStage(-1));
            view.NextButton.onClick.RemoveAllListeners();
            view.NextButton.onClick.AddListener(() => MoveStage(1));
            view.TestButton.onClick.RemoveAllListeners();
            view.TestButton.onClick.AddListener(StartRun);
            view.TargetButton.onClick.RemoveAllListeners();
            view.TargetButton.onClick.AddListener(ResetDisplay);
            SetButtonIcon(view.PrevButton, ButtonIconKind.Previous, 18f);
            SetButtonIcon(view.NextButton, ButtonIconKind.Next, 18f);
            SetButtonIcon(view.TestButton, ButtonIconKind.Run, 22f);
            SetButtonIcon(view.TargetButton, ButtonIconKind.Target, 22f);
            LayoutActionButtons(view.TestButton, view.TargetButton);
        }

        private void CreateCanvas()
        {
            EnsureEventSystem();

            var canvasObject = new GameObject("Rule Reconstruction Canvas");
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960f, 540f);
            scaler.matchWidthOrHeight = 0.5f;

            var background = canvasObject.AddComponent<Image>();
            background.color = pageColor;
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
            DontDestroyOnLoad(eventSystem);
        }

        private void BuildLayout()
        {
            ClearCanvasChildren();

            var root = ui.CreatePanel("Root", canvas.transform, pageColor);
            RuleReconstructionUiFactory.Stretch(root, Vector2.zero, Vector2.one, new Vector2(18f, 12f), new Vector2(-18f, -12f));

            titleText = ui.CreateText("Title", root, string.Empty, 22, TextAnchor.MiddleLeft);
            RuleReconstructionUiFactory.Anchor(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -34f), new Vector2(0f, 0f));

            prevButton = ui.CreateButton("PrevButton", root, string.Empty, 16, () => MoveStage(-1));
            RuleReconstructionUiFactory.Anchor(prevButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-160f, -34f), new Vector2(-84f, 0f));
            SetButtonIcon(prevButton, ButtonIconKind.Previous, 18f);

            nextButton = ui.CreateButton("NextButton", root, string.Empty, 16, () => MoveStage(1));
            RuleReconstructionUiFactory.Anchor(nextButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-76f, -34f), new Vector2(0f, 0f));
            SetButtonIcon(nextButton, ButtonIconKind.Next, 18f);

            sidebar = ui.CreatePanel("Sidebar", root, panelColor);
            RuleReconstructionUiFactory.Anchor(sidebar, new Vector2(0f, 0f), new Vector2(0.33f, 1f), new Vector2(0f, 0f), new Vector2(-10f, -46f));

            boardPanel = ui.CreatePanel("BoardPanel", root, panelColor);
            RuleReconstructionUiFactory.Anchor(boardPanel, new Vector2(0.33f, 0f), new Vector2(1f, 1f), new Vector2(10f, 0f), new Vector2(0f, -46f));

            boardTitleText = ui.CreateText("BoardTitle", boardPanel, string.Empty, 22, TextAnchor.MiddleLeft);
            RuleReconstructionUiFactory.Anchor(boardTitleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -42f), new Vector2(-120f, -8f));

            resultBannerText = ui.CreateText("ResultBanner", boardPanel, string.Empty, 54, TextAnchor.MiddleCenter);
            RuleReconstructionUiFactory.Anchor(resultBannerText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0f, 26f), new Vector2(0f, -26f));
            resultBannerText.fontStyle = FontStyle.Bold;
            resultBannerText.raycastTarget = false;
            var bannerOutline = resultBannerText.gameObject.AddComponent<Outline>();
            bannerOutline.effectColor = new Color(0f, 0f, 0f, 0.72f);
            bannerOutline.effectDistance = new Vector2(2f, -2f);

            BuildSidebar();
            BuildBoardCells();

            view = canvas.gameObject.AddComponent<RuleReconstructionView>();
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
        }

        private void BuildSidebar()
        {
            sourceSlotImages.Clear();
            previewViews.Clear();
            rangeIconViews.Clear();
            effectIconTexts.Clear();
            rulePanelImages.Clear();
            rulePanelOutlines.Clear();
            directionBlockPreviews.Clear();
            directionBlockOutlines.Clear();
            rangeBlockOutlines.Clear();
            effectBlockOutlines.Clear();

            foreach (Transform child in sidebar)
            {
                Destroy(child.gameObject);
            }

            var content = ui.CreatePanel("SidebarContent", sidebar, Color.clear);
            RuleReconstructionUiFactory.Stretch(content, Vector2.zero, Vector2.one, new Vector2(14f, 198f), new Vector2(-14f, -14f));

            var y = -2f;
            var stageColors = StageRuleAnalyzer.GetStageColors(CurrentStage);
            if (!ContainsColor(stageColors, selectedRuleColor))
            {
                selectedRuleColor = stageColors[0];
            }

            foreach (var color in stageColors)
            {
                AddRuleControls(content, color, y);
                y -= 58f;
            }

            var actionRoot = ui.CreatePanel("Actions", sidebar, Color.clear);
            RuleReconstructionUiFactory.Anchor(actionRoot, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(14f, 12f), new Vector2(-14f, 188f));

            AddBlockTray(actionRoot);

            if (usingSceneView)
            {
                return;
            }

            analysisText = ui.CreateText("StageAnalysis", content, string.Empty, 12, TextAnchor.MiddleLeft);
            RuleReconstructionUiFactory.Anchor(analysisText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, y), new Vector2(0f, y));
            analysisText.gameObject.SetActive(false);

            testButton = ui.CreateButton("TestButton", actionRoot, string.Empty, 16, StartRun);
            RuleReconstructionUiFactory.Anchor(testButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(-4f, -108f));
            SetButtonIcon(testButton, ButtonIconKind.Run, 22f);

            targetButton = ui.CreateButton("TargetButton", actionRoot, string.Empty, 16, ResetDisplay);
            RuleReconstructionUiFactory.Anchor(targetButton.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(4f, -160f), new Vector2(0f, -108f));
            SetButtonIcon(targetButton, ButtonIconKind.Target, 22f);

            statusText = ui.CreateText("Status", actionRoot, string.Empty, 15, TextAnchor.MiddleLeft);
            RuleReconstructionUiFactory.Anchor(statusText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 18f));
        }

        private static void LayoutActionButtons(Button runButton, Button targetButton)
        {
            RuleReconstructionUiFactory.Anchor(runButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(-4f, -108f));
            RuleReconstructionUiFactory.Anchor(targetButton.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(4f, -160f), new Vector2(0f, -108f));
        }

        private void AddRuleControls(RectTransform parent, BoxColor color, float top)
        {
            var panel = ui.CreateButton($"{color}RulePanel", parent, string.Empty, 1, () => SelectRuleColor(color));
            var panelRect = panel.GetComponent<RectTransform>();
            RuleReconstructionUiFactory.Anchor(panelRect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, top - 54f), new Vector2(0f, top));
            rulePanelImages[color] = panel.targetGraphic as Image;
            var outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = selectedRuleOutlineColor;
            outline.effectDistance = new Vector2(1f, -1f);
            outline.enabled = false;
            rulePanelOutlines[color] = outline;

            var sourceButton = ui.CreateIconButton($"{color}Source", panelRect, () => SelectRuleColor(color));
            RuleReconstructionUiFactory.Anchor(sourceButton.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, -18f), new Vector2(44f, 18f));
            AddStaticOutline(sourceButton.gameObject, GetSourceColor(color), 1f);
            var sourceSwatch = ui.CreatePanel($"{color}SourceSwatch", sourceButton.transform, GetSourceColor(color));
            RuleReconstructionUiFactory.Stretch(sourceSwatch, Vector2.zero, Vector2.one, new Vector2(8f, 8f), new Vector2(-8f, -8f));
            sourceSlotImages[color] = sourceSwatch.GetComponent<Image>();

            var directionButton = ui.CreateIconButton($"{color}Direction", panelRect, () => SelectRuleColor(color));
            RuleReconstructionUiFactory.Anchor(directionButton.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(52f, -18f), new Vector2(88f, 18f));
            AddStaticOutline(directionButton.gameObject, directionSlotOutlineColor, 1f);
            var directionPreview = CreateRulePreview($"{color}DirectionIcon", directionButton.transform);
            AnchorIconPreview(directionPreview.Root);
            ConfigureSmallPreview(directionPreview.Root);
            previewViews[color] = directionPreview;

            var rangeButton = ui.CreateIconButton($"{color}Range", panelRect, () => SelectRuleColor(color));
            RuleReconstructionUiFactory.Anchor(rangeButton.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(96f, -18f), new Vector2(132f, 18f));
            AddStaticOutline(rangeButton.gameObject, rangeSlotOutlineColor, 1f);
            rangeIconViews[color] = CreateRangeIcon($"{color}RangeIcon", rangeButton.transform);

            var effectButton = ui.CreateIconButton($"{color}Effect", panelRect, () => SelectRuleColor(color));
            RuleReconstructionUiFactory.Anchor(effectButton.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(140f, -18f), new Vector2(176f, 18f));
            AddStaticOutline(effectButton.gameObject, effectSlotOutlineColor, 1f);
            effectIconTexts[color] = CreateEffectText($"{color}EffectText", effectButton.transform, EffectType.AddNumber, 17);
        }

        private void AddBlockTray(RectTransform parent)
        {
            var directionRoot = ui.CreatePanel("DirectionBlocks", parent, new Color(0.15f, 0.25f, 0.41f));
            RuleReconstructionUiFactory.Anchor(directionRoot, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -40f), new Vector2(0f, -2f));
            AddStaticOutline(directionRoot.gameObject, directionSlotOutlineColor, 1f);
            var directionGrid = directionRoot.gameObject.AddComponent<GridLayoutGroup>();
            directionGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            directionGrid.constraintCount = 5;
            directionGrid.cellSize = new Vector2(38f, 30f);
            directionGrid.spacing = new Vector2(7f, 0f);
            directionGrid.padding = new RectOffset(10, 0, 5, 0);

            AddDirectionBlockButton(directionRoot, DirectionType.Cross, () => ApplyDirectionBlock(DirectionType.Cross));
            AddDirectionBlockButton(directionRoot, DirectionType.Diagonal, () => ApplyDirectionBlock(DirectionType.Diagonal));
            AddDirectionBlockButton(directionRoot, DirectionType.Horizontal, () => ApplyDirectionBlock(DirectionType.Horizontal));
            AddDirectionBlockButton(directionRoot, DirectionType.Vertical, () => ApplyDirectionBlock(DirectionType.Vertical));
            AddDirectionBlockButton(directionRoot, DirectionType.AllAround, () => ApplyDirectionBlock(DirectionType.AllAround));

            var rangeRoot = ui.CreatePanel("RangeBlocks", parent, new Color(0.15f, 0.25f, 0.41f));
            RuleReconstructionUiFactory.Anchor(rangeRoot, new Vector2(0f, 1f), new Vector2(0.52f, 1f), new Vector2(0f, -84f), new Vector2(-4f, -46f));
            AddStaticOutline(rangeRoot.gameObject, rangeSlotOutlineColor, 1f);
            var rangeGrid = rangeRoot.gameObject.AddComponent<GridLayoutGroup>();
            rangeGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            rangeGrid.constraintCount = 2;
            rangeGrid.cellSize = new Vector2(38f, 30f);
            rangeGrid.spacing = new Vector2(7f, 0f);
            rangeGrid.padding = new RectOffset(10, 0, 5, 0);

            AddRangeBlockButton(rangeRoot, RangeType.One, () => ApplyRangeBlock(RangeType.One));
            AddRangeBlockButton(rangeRoot, RangeType.Two, () => ApplyRangeBlock(RangeType.Two));

            var effectRoot = ui.CreatePanel("EffectBlocks", parent, new Color(0.15f, 0.25f, 0.41f));
            RuleReconstructionUiFactory.Anchor(effectRoot, new Vector2(0.52f, 1f), new Vector2(1f, 1f), new Vector2(4f, -84f), new Vector2(0f, -46f));
            AddStaticOutline(effectRoot.gameObject, effectSlotOutlineColor, 1f);
            var effectGrid = effectRoot.gameObject.AddComponent<GridLayoutGroup>();
            effectGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            effectGrid.constraintCount = 2;
            effectGrid.cellSize = new Vector2(38f, 30f);
            effectGrid.spacing = new Vector2(7f, 0f);
            effectGrid.padding = new RectOffset(10, 0, 5, 0);

            AddEffectBlockButton(effectRoot, EffectType.AddNumber, () => ApplyEffectBlock(EffectType.AddNumber));
            AddEffectBlockButton(effectRoot, EffectType.SubtractNumber, () => ApplyEffectBlock(EffectType.SubtractNumber));
        }

        private static Outline AddStaticOutline(GameObject target, Color color, float size)
        {
            var outline = target.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(size, -size);
            return outline;
        }

        private void AddDirectionBlockButton(RectTransform parent, DirectionType direction, UnityEngine.Events.UnityAction onClick)
        {
            var button = ui.CreateIconButton($"Block{direction}", parent, onClick);
            directionBlockOutlines[direction] = AddSelectionOutline(button.gameObject, directionSlotOutlineColor);
            var icon = CreateRulePreview($"{direction}Icon", button.transform);
            directionBlockPreviews[direction] = icon;
            AnchorIconPreview(icon.Root);
            ConfigureSmallPreview(icon.Root);

            var affected = RuleReconstructionPreviewPattern.GetAffectedCells(direction);
            for (var index = 0; index < icon.Cells.Count; index++)
            {
                icon.Cells[index].color = affected.Contains(index) ? affectedTextColor : new Color(0.26f, 0.34f, 0.46f);
            }
        }

        private void AddRangeBlockButton(RectTransform parent, RangeType range, UnityEngine.Events.UnityAction onClick)
        {
            var button = ui.CreateIconButton($"Block{range}", parent, onClick);
            rangeBlockOutlines[range] = AddSelectionOutline(button.gameObject, rangeSlotOutlineColor);
            var icon = CreateRangeIcon($"{range}Icon", button.transform);
            UpdateRangeIcon(icon, range);
        }

        private Outline AddSelectionOutline(GameObject target, Color color)
        {
            var outline = target.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(2f, -2f);
            outline.enabled = false;
            return outline;
        }

        private RangeIconView CreateRangeIcon(string name, Transform parent)
        {
            var root = ui.CreatePanel(name, parent, Color.clear);
            RuleReconstructionUiFactory.Stretch(root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var center = ui.CreatePanel("CenterDot", root, affectedTextColor);
            RuleReconstructionUiFactory.Anchor(center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-3f, -3f), new Vector2(3f, 3f));

            var dots = new List<RectTransform>();
            for (var index = 0; index < 4; index++)
            {
                dots.Add(CreateRangeDot(root));
            }

            return new RangeIconView(root, center, dots);
        }

        private RectTransform CreateRangeDot(Transform parent)
        {
            var dot = ui.CreatePanel("RangeDot", parent, new Color(0.54f, 0.93f, 1f, 0.62f));
            return dot;
        }

        private void UpdateRangeIcon(RangeIconView icon, RangeType range)
        {
            var radius = range == RangeType.One ? 8f : 13f;
            RuleReconstructionUiFactory.Anchor(icon.Dots[0], new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-3f, radius - 3f), new Vector2(3f, radius + 3f));
            RuleReconstructionUiFactory.Anchor(icon.Dots[1], new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(radius - 3f, -3f), new Vector2(radius + 3f, 3f));
            RuleReconstructionUiFactory.Anchor(icon.Dots[2], new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-3f, -radius - 3f), new Vector2(3f, -radius + 3f));
            RuleReconstructionUiFactory.Anchor(icon.Dots[3], new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-radius - 3f, -3f), new Vector2(-radius + 3f, 3f));
        }

        private void AddEffectBlockButton(RectTransform parent, EffectType effect, UnityEngine.Events.UnityAction onClick)
        {
            var button = ui.CreateIconButton($"Block{effect}", parent, onClick);
            effectBlockOutlines[effect] = AddSelectionOutline(button.gameObject, effectSlotOutlineColor);
            CreateEffectText($"{effect}Text", button.transform, effect, 17);
        }

        private Text CreateEffectText(string name, Transform parent, EffectType effect, int fontSize)
        {
            var text = ui.CreateText(name, parent, GetEffectLabel(effect), fontSize, TextAnchor.MiddleCenter);
            text.fontStyle = FontStyle.Bold;
            text.color = affectedTextColor;
            RuleReconstructionUiFactory.Stretch(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return text;
        }

        private static string GetEffectLabel(EffectType effect)
        {
            return effect == EffectType.SubtractNumber ? "-1" : "+1";
        }

        private void AddPlusIcon(Transform parent, Vector2 halfSize)
        {
            var horizontal = ui.CreatePanel("PlusHorizontal", parent, affectedTextColor);
            RuleReconstructionUiFactory.Anchor(horizontal, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-halfSize.x, -halfSize.y), new Vector2(halfSize.x, halfSize.y));

            var vertical = ui.CreatePanel("PlusVertical", parent, affectedTextColor);
            RuleReconstructionUiFactory.Anchor(vertical, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-halfSize.y, -halfSize.x), new Vector2(halfSize.y, halfSize.x));
        }

        private void BuildBoardCells()
        {
            boardCells.Clear();

            if (boardPanel == null)
            {
                return;
            }

            if (boardRoot != null)
            {
                Destroy(boardRoot.gameObject);
            }

            boardRoot = ui.CreatePanel("Board", boardPanel, Color.clear);
            RuleReconstructionUiFactory.Anchor(boardRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-182f, -182f), new Vector2(182f, 182f));
            resultBannerText.rectTransform.SetAsLastSibling();

            var grid = boardRoot.GetComponent<GridLayoutGroup>();
            if (grid == null)
            {
                grid = boardRoot.gameObject.AddComponent<GridLayoutGroup>();
            }

            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            grid.cellSize = new Vector2(68f, 68f);
            grid.spacing = new Vector2(4f, 4f);

            for (var y = CurrentStage.Height - 1; y >= 0; y--)
            {
                for (var x = 0; x < CurrentStage.Width; x++)
                {
                    var cell = ui.CreatePanel($"Cell{x}_{y}", boardRoot, cellColor);
                    var label = ui.CreateText("Value", cell, string.Empty, 25, TextAnchor.MiddleCenter);
                    RuleReconstructionUiFactory.Stretch(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                    boardCells.Add(new BoardCellView(new GridPosition(x, y), cell.GetComponent<Image>(), label));
                }
            }
        }

        private void Update()
        {
            if (session == null || canvas == null)
            {
                EnsureInitialized();
            }

            if (session == null || canvas == null)
            {
                return;
            }

            UpdateTexts();
            UpdateRuleButtons();
            UpdateRulePreviews();
            RenderBoard();
        }

        private void UpdateTexts()
        {
            titleText.text = $"{session.StageIndex + 1:00}/{session.StageCount:00} {GetStageTitleName()}";
            boardTitleText.text = string.Empty;
            statusText.text = string.Empty;
            resultBannerText.text = GetResultBannerText();
            resultBannerText.color = GetResultTextColor();
            resultBannerText.enabled = !string.IsNullOrEmpty(resultBannerText.text);

            analysisText.text = string.Empty;
            UpdateNavigationButtons();
        }

        private void UpdateNavigationButtons()
        {
            if (prevButton != null)
            {
                SetButtonIcon(prevButton, ButtonIconKind.Previous, 18f);
                SetNavigationButtonState(prevButton, !isRunning && !session.IsFirstStage);
            }

            if (nextButton != null)
            {
                var nextAvailable = !isRunning && session.IsCurrentStageCleared && !session.IsLastStage;
                SetButtonIcon(nextButton, ButtonIconKind.Next, 18f);
                SetNavigationButtonState(nextButton, nextAvailable);
            }
        }

        private void SetNavigationButtonState(Button button, bool interactable)
        {
            button.interactable = interactable;

            if (button.targetGraphic != null)
            {
                button.targetGraphic.color = interactable ? buttonColor : new Color(0.16f, 0.24f, 0.36f);
            }

            var label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.color = interactable ? Color.white : new Color(1f, 1f, 1f, 0.42f);
            }

            var icon = button.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null)
            {
                icon.color = interactable ? Color.white : new Color(1f, 1f, 1f, 0.42f);
            }
        }

        private void SetButtonIcon(Button button, ButtonIconKind kind, float size)
        {
            var text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = string.Empty;
            }

            var icon = button.transform.Find("Icon")?.GetComponent<Image>();
            RectTransform iconRect;
            if (icon == null)
            {
                var iconObject = new GameObject("Icon");
                iconObject.transform.SetParent(button.transform, false);
                iconRect = iconObject.AddComponent<RectTransform>();
                icon = iconObject.AddComponent<Image>();
                icon.raycastTarget = false;
            }
            else
            {
                iconRect = icon.GetComponent<RectTransform>();
            }

            icon.sprite = CreateIconSprite(kind);
            icon.color = Color.white;
            icon.preserveAspect = true;
            RuleReconstructionUiFactory.Anchor(
                iconRect,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-size * 0.5f, -size * 0.5f),
                new Vector2(size * 0.5f, size * 0.5f));
        }

        private Sprite CreateIconSprite(ButtonIconKind kind)
        {
            var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave
            };

            var clear = new Color(1f, 1f, 1f, 0f);
            var pixels = texture.GetPixels();
            for (var index = 0; index < pixels.Length; index++)
            {
                pixels[index] = clear;
            }
            texture.SetPixels(pixels);

            switch (kind)
            {
                case ButtonIconKind.Previous:
                    FillTriangle(texture, new Vector2Int(23, 7), new Vector2Int(8, 16), new Vector2Int(23, 25));
                    break;
                case ButtonIconKind.Next:
                case ButtonIconKind.Run:
                    FillTriangle(texture, new Vector2Int(9, 7), new Vector2Int(24, 16), new Vector2Int(9, 25));
                    break;
                case ButtonIconKind.Target:
                    FillTarget(texture);
                    break;
                case ButtonIconKind.Check:
                    FillCheck(texture);
                    break;
                case ButtonIconKind.Cross:
                    FillCross(texture);
                    break;
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 32f, 32f), new Vector2(0.5f, 0.5f), 32f);
        }

        private static void FillTarget(Texture2D texture)
        {
            for (var y = 7; y <= 24; y += 8)
            {
                for (var x = 7; x <= 24; x += 8)
                {
                    FillRect(texture, x, y, 5, 5);
                }
            }
        }

        private static void FillCheck(Texture2D texture)
        {
            FillRect(texture, 7, 14, 5, 5);
            FillRect(texture, 11, 10, 5, 5);
            FillRect(texture, 15, 6, 5, 5);
            FillRect(texture, 19, 18, 5, 5);
            FillRect(texture, 23, 22, 5, 5);
        }

        private static void FillCross(Texture2D texture)
        {
            for (var offset = 0; offset < 16; offset += 4)
            {
                FillRect(texture, 8 + offset, 8 + offset, 5, 5);
                FillRect(texture, 20 - offset, 8 + offset, 5, 5);
            }
        }

        private static void FillTriangle(Texture2D texture, Vector2Int a, Vector2Int b, Vector2Int c)
        {
            for (var y = 0; y < texture.height; y++)
            {
                for (var x = 0; x < texture.width; x++)
                {
                    var point = new Vector2(x + 0.5f, y + 0.5f);
                    if (IsInsideTriangle(point, a, b, c))
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                }
            }
        }

        private static bool IsInsideTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
        {
            var d1 = Sign(point, a, b);
            var d2 = Sign(point, b, c);
            var d3 = Sign(point, c, a);
            return !(d1 < 0f || d2 < 0f || d3 < 0f) || !(d1 > 0f || d2 > 0f || d3 > 0f);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        private static void FillRect(Texture2D texture, int x, int y, int width, int height)
        {
            for (var yy = y; yy < y + height; yy++)
            {
                for (var xx = x; xx < x + width; xx++)
                {
                    texture.SetPixel(xx, yy, Color.white);
                }
            }
        }

        private void UpdateRuleButtons()
        {
            foreach (var color in StageRuleAnalyzer.GetStageColors(CurrentStage))
            {
                rulePanelImages[color].color = rulePanelColor;
                rulePanelOutlines[color].enabled = color == selectedRuleColor;
                UpdateRangeIcon(rangeIconViews[color], session.GetRange(color));
                effectIconTexts[color].text = GetEffectLabel(session.GetEffect(color));
            }

            UpdateBlockSelection();
            UpdateBlockPreviews();
        }

        private void UpdateBlockSelection()
        {
            var selectedDirection = session.GetDirection(selectedRuleColor);
            var selectedRange = session.GetRange(selectedRuleColor);
            var selectedEffect = session.GetEffect(selectedRuleColor);

            foreach (var pair in directionBlockOutlines)
            {
                pair.Value.enabled = pair.Key == selectedDirection;
            }

            foreach (var pair in rangeBlockOutlines)
            {
                pair.Value.enabled = pair.Key == selectedRange;
            }

            foreach (var pair in effectBlockOutlines)
            {
                pair.Value.enabled = pair.Key == selectedEffect;
            }
        }

        private void UpdateBlockPreviews()
        {
            var selectedColor = GetSourceColor(selectedRuleColor);
            foreach (var pair in directionBlockPreviews)
            {
                pair.Value.Cells[4].color = selectedColor;
            }
        }

        private void UpdateRulePreviews()
        {
            foreach (var color in StageRuleAnalyzer.GetStageColors(CurrentStage))
            {
                sourceSlotImages[color].color = GetSourceColor(color);

                var preview = previewViews[color];
                var affected = RuleReconstructionPreviewPattern.GetAffectedCells(session.GetDirection(color));

                for (var index = 0; index < preview.Cells.Count; index++)
                {
                    var cell = preview.Cells[index];
                    if (index == 4)
                    {
                        cell.color = GetSourceColor(color);
                    }
                    else
                    {
                        cell.color = affected.Contains(index) ? affectedTextColor : new Color(0.28f, 0.36f, 0.48f);
                    }
                }
            }
        }

        private void RenderBoard()
        {
            RuleReconstructionBoardRenderer.Render(
                boardCells,
                displayBoard,
                CurrentStage,
                showMismatch,
                activeAffectedCells,
                cellColor,
                wrongTextColor,
                affectedTextColor,
                GetSourceColor);
        }

        private void MoveStage(int delta)
        {
            if (isRunning || delta > 0 && (!session.IsCurrentStageCleared || session.IsLastStage) || delta < 0 && session.IsFirstStage)
            {
                return;
            }

            session.MoveStage(delta);
            selectedRuleColor = StageRuleAnalyzer.GetStageColors(CurrentStage)[0];
            ResetDisplay();
            BuildSidebar();
            BuildBoardCells();
        }

        private void SelectRuleColor(BoxColor color)
        {
            selectedRuleColor = color;
            UpdateRuleButtons();
        }

        private void ApplyDirectionBlock(DirectionType direction)
        {
            session.SetDirection(selectedRuleColor, direction);
            ResetDisplay();
        }

        private void ApplyRangeBlock(RangeType range)
        {
            session.SetRange(selectedRuleColor, range);
            ResetDisplay();
        }

        private void ApplyEffectBlock(EffectType effect)
        {
            session.SetEffect(selectedRuleColor, effect);
            ResetDisplay();
        }

        private void ResetDisplay()
        {
            StopRunRoutine();
            session.ResetSimulation();
            activeAffectedCells.Clear();
            displayBoard = CurrentStage.TargetBoard;
            showResult = false;
            showMismatch = false;
            showResultBanner = false;
        }

        private void StartRun()
        {
            StopRunRoutine();
            runRoutine = StartCoroutine(RunSimulation());
        }

        private IEnumerator RunSimulation()
        {
            isRunning = true;
            showResult = true;
            showMismatch = false;
            showResultBanner = false;
            session.ResetSimulation();
            activeAffectedCells.Clear();
            displayBoard = session.ResultBoard;

            yield return new WaitForSeconds(0.35f);

            while (!IsComplete)
            {
                session.SetActiveSource(session.AppliedSourceCount);
                activeAffectedCells = session.GetAffectedCells(session.ActiveSourceIndex);
                displayBoard = session.ResultBoard;
                yield return new WaitForSeconds(0.25f);

                session.ApplyNextSource();
                displayBoard = session.ResultBoard;
                yield return new WaitForSeconds(0.45f);
            }

            session.ClearActiveSource();
            activeAffectedCells.Clear();

            if (!session.ValidationResult.IsClear)
            {
                showMismatch = true;
                showResultBanner = true;
                yield return new WaitForSeconds(1.1f);
                showResultBanner = false;
            }
            else
            {
                session.MarkCurrentStageCleared();
                showResultBanner = true;
                yield return new WaitForSeconds(1.1f);
                showResultBanner = false;
            }

            isRunning = false;
            runRoutine = null;
        }

        private void StopRunRoutine()
        {
            if (runRoutine == null)
            {
                isRunning = false;
                return;
            }

            StopCoroutine(runRoutine);
            runRoutine = null;
            isRunning = false;
        }

        private string GetStageTitleName()
        {
            var name = CurrentStage.Name;
            return name.Length > 3 && char.IsDigit(name[0]) && char.IsDigit(name[1]) && name[2] == ' '
                ? name.Substring(3)
                : name;
        }

        private string GetResultBannerText()
        {
            if (!showResultBanner)
            {
                return string.Empty;
            }

            if (showMismatch)
            {
                return $"X {session.ValidationResult.WrongCellCount}";
            }

            if (showResult && IsComplete && session.ValidationResult.IsClear)
            {
                return "OK";
            }

            return string.Empty;
        }

        private Color GetResultTextColor()
        {
            if (showMismatch)
            {
                return wrongTextColor;
            }

            if (showResult && session.ValidationResult.IsClear)
            {
                return clearTextColor;
            }

            return Color.white;
        }

        private Color GetSourceColor(BoxColor color)
        {
            return color switch
            {
                BoxColor.Red => new Color(0.82f, 0.18f, 0.16f),
                BoxColor.Blue => new Color(0.14f, 0.36f, 0.88f),
                BoxColor.Green => new Color(0.13f, 0.64f, 0.28f),
                BoxColor.Yellow => new Color(0.92f, 0.74f, 0.16f),
                _ => cellColor
            };
        }

        private static bool ContainsColor(IReadOnlyList<BoxColor> colors, BoxColor target)
        {
            for (var index = 0; index < colors.Count; index++)
            {
                if (colors[index] == target)
                {
                    return true;
                }
            }

            return false;
        }

        private static void AnchorIconPreview(RectTransform rectTransform)
        {
            RuleReconstructionUiFactory.Stretch(rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-12f, -12f), new Vector2(12f, 12f));
        }

        private static void ConfigureSmallPreview(RectTransform rectTransform)
        {
            var grid = rectTransform.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(7f, 7f);
            grid.spacing = new Vector2(1f, 1f);
        }

        private RulePreviewView CreateRulePreview(string name, Transform parent)
        {
            var root = ui.CreatePanel(name, parent, Color.clear);
            var cells = new List<Image>();
            var grid = root.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.cellSize = new Vector2(12f, 12f);
            grid.spacing = new Vector2(2f, 2f);

            for (var index = 0; index < 9; index++)
            {
                var cell = ui.CreatePanel($"PreviewCell{index}", root, new Color(0.28f, 0.36f, 0.48f));
                cells.Add(cell.GetComponent<Image>());
            }

            return new RulePreviewView(root, cells);
        }

        private void ClearCanvasChildren()
        {
            foreach (Transform child in canvas.transform)
            {
                Destroy(child.gameObject);
            }
        }

        private bool IsComplete => session.IsComplete;
        private StageData CurrentStage => session.CurrentStage;

    }
}
