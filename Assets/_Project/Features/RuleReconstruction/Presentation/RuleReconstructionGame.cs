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
        private readonly Dictionary<BoxColor, Image> rulePanelImages = new();
        private readonly Dictionary<BoxColor, Outline> rulePanelOutlines = new();
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
            return true;
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

            prevButton = ui.CreateButton("PrevButton", root, "Prev", 16, () => MoveStage(-1));
            RuleReconstructionUiFactory.Anchor(prevButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-160f, -34f), new Vector2(-84f, 0f));

            nextButton = ui.CreateButton("NextButton", root, "Next", 16, () => MoveStage(1));
            RuleReconstructionUiFactory.Anchor(nextButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-76f, -34f), new Vector2(0f, 0f));

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
            rulePanelImages.Clear();
            rulePanelOutlines.Clear();

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

            testButton = ui.CreateButton("TestButton", actionRoot, "Test", 16, StartRun);
            RuleReconstructionUiFactory.Anchor(testButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -132f), new Vector2(0f, -108f));

            targetButton = ui.CreateButton("TargetButton", actionRoot, "Target", 16, ResetDisplay);
            RuleReconstructionUiFactory.Anchor(targetButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -160f), new Vector2(0f, -136f));

            statusText = ui.CreateText("Status", actionRoot, string.Empty, 15, TextAnchor.MiddleLeft);
            RuleReconstructionUiFactory.Anchor(statusText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 18f));
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
            var sourceSwatch = ui.CreatePanel($"{color}SourceSwatch", sourceButton.transform, GetSourceColor(color));
            RuleReconstructionUiFactory.Stretch(sourceSwatch, Vector2.zero, Vector2.one, new Vector2(8f, 8f), new Vector2(-8f, -8f));
            sourceSlotImages[color] = sourceSwatch.GetComponent<Image>();

            var directionButton = ui.CreateIconButton($"{color}Direction", panelRect, () => SelectRuleColor(color));
            RuleReconstructionUiFactory.Anchor(directionButton.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(52f, -18f), new Vector2(88f, 18f));
            var directionPreview = CreateRulePreview($"{color}DirectionIcon", directionButton.transform);
            AnchorIconPreview(directionPreview.Root);
            ConfigureSmallPreview(directionPreview.Root);
            previewViews[color] = directionPreview;

            var rangeButton = ui.CreateIconButton($"{color}Range", panelRect, () => SelectRuleColor(color));
            RuleReconstructionUiFactory.Anchor(rangeButton.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(96f, -18f), new Vector2(132f, 18f));
            rangeIconViews[color] = CreateRangeIcon($"{color}RangeIcon", rangeButton.transform);

            var effectButton = ui.CreateIconButton($"{color}Effect", panelRect, () => SelectRuleColor(color));
            RuleReconstructionUiFactory.Anchor(effectButton.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(140f, -18f), new Vector2(176f, 18f));
            AddPlusIcon(effectButton.transform, new Vector2(10f, 2f));
        }

        private void AddBlockTray(RectTransform parent)
        {
            var directionRoot = ui.CreatePanel("DirectionBlocks", parent, new Color(0.15f, 0.25f, 0.41f));
            RuleReconstructionUiFactory.Anchor(directionRoot, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -40f), new Vector2(0f, -2f));
            AddSectionAccent(directionRoot, new Color(0.54f, 0.93f, 1f));
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
            AddSectionAccent(rangeRoot, new Color(0.35f, 0.95f, 0.56f));
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
            AddSectionAccent(effectRoot, new Color(1f, 0.86f, 0.20f));
            AddEffectBlockButton(effectRoot, () => SelectRuleColor(selectedRuleColor));
        }

        private void AddSectionAccent(RectTransform parent, Color color)
        {
            var outline = parent.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(1f, -1f);
        }

        private void AddDirectionBlockButton(RectTransform parent, DirectionType direction, UnityEngine.Events.UnityAction onClick)
        {
            var button = ui.CreateIconButton($"Block{direction}", parent, onClick);
            var icon = CreateRulePreview($"{direction}Icon", button.transform);
            AnchorIconPreview(icon.Root);
            ConfigureSmallPreview(icon.Root);

            var affected = RuleReconstructionPreviewPattern.GetAffectedCells(direction);
            for (var index = 0; index < icon.Cells.Count; index++)
            {
                if (index == 4)
                {
                    icon.Cells[index].color = new Color(0.88f, 0.22f, 0.18f);
                }
                else
                {
                    icon.Cells[index].color = affected.Contains(index) ? affectedTextColor : new Color(0.26f, 0.34f, 0.46f);
                }
            }
        }

        private void AddRangeBlockButton(RectTransform parent, RangeType range, UnityEngine.Events.UnityAction onClick)
        {
            var button = ui.CreateIconButton($"Block{range}", parent, onClick);
            var icon = CreateRangeIcon($"{range}Icon", button.transform);
            UpdateRangeIcon(icon, range);
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

        private void AddEffectBlockButton(RectTransform parent, UnityEngine.Events.UnityAction onClick)
        {
            var button = ui.CreateIconButton("BlockEffect", parent, onClick);
            RuleReconstructionUiFactory.Anchor(button.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, -15f), new Vector2(48f, 15f));

            AddPlusIcon(button.transform, new Vector2(10f, 2f));
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
            titleText.text = $"Rule Reconstruction / {CurrentStage.Name}";
            boardTitleText.text = GetMainBoardTitle();
            statusText.text = GetStatusText();
            statusText.color = GetResultTextColor();
            resultBannerText.text = GetResultBannerText();
            resultBannerText.color = GetResultTextColor();
            resultBannerText.enabled = !string.IsNullOrEmpty(resultBannerText.text);

            analysisText.text = string.Empty;
        }

        private void UpdateRuleButtons()
        {
            foreach (var color in StageRuleAnalyzer.GetStageColors(CurrentStage))
            {
                rulePanelImages[color].color = rulePanelColor;
                rulePanelOutlines[color].enabled = color == selectedRuleColor;
                UpdateRangeIcon(rangeIconViews[color], session.GetRange(color));
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
            session.MoveStage(delta);
            selectedRuleColor = StageRuleAnalyzer.GetStageColors(CurrentStage)[0];
            ResetDisplay();
            BuildSidebar();
            BuildBoardCells();
        }

        private void SelectRuleColor(BoxColor color)
        {
            selectedRuleColor = color;
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

        private string GetMainBoardTitle()
        {
            if (showMismatch)
            {
                return $"Wrong {session.ValidationResult.WrongCellCount}";
            }

            if (showResult)
            {
                return session.ValidationResult.IsClear && IsComplete ? "Clear" : "Test Result";
            }

            return "Target";
        }

        private string GetStatusText()
        {
            if (isRunning)
            {
                return GetRunningStatusText();
            }

            if (!showResult)
            {
                return "TEST READY";
            }

            return session.ValidationResult.IsClear ? "CLEAR" : GetMismatchSummaryText();
        }

        private string GetResultBannerText()
        {
            if (!showResultBanner)
            {
                return string.Empty;
            }

            if (showMismatch)
            {
                return $"WRONG {session.ValidationResult.WrongCellCount}";
            }

            if (showResult && IsComplete && session.ValidationResult.IsClear)
            {
                return "CLEAR";
            }

            return string.Empty;
        }

        private string GetMismatchSummaryText()
        {
            var summary = RuleReconstructionMismatchAnalyzer.GetSummary(session.ResultBoard, CurrentStage.TargetBoard);
            return $"WRONG {summary.Total} | NEED {summary.NeedMore} | OVER {summary.Excess}";
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

        private string GetRunningStatusText()
        {
            if (session.ActiveSourceIndex < 0 || session.ActiveSourceIndex >= CurrentStage.Sources.Count)
            {
                return "TESTING";
            }

            var source = CurrentStage.Sources[session.ActiveSourceIndex];
            return $"Applying {source.Color}...";
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
