using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    public sealed class RuleReconstructionPrototype : MonoBehaviour
    {
        private static readonly BoxColor[] AllColors =
        {
            BoxColor.Red,
            BoxColor.Blue,
            BoxColor.Green,
            BoxColor.Yellow
        };

        private readonly Dictionary<BoxColor, Text> directionValueTexts = new();
        private readonly Dictionary<BoxColor, Text> rangeValueTexts = new();
        private readonly Dictionary<BoxColor, RulePreviewView> previewViews = new();
        private readonly List<BoardCellView> boardCells = new();

        private RuleReconstructionSession session;
        private BoardState displayBoard;
        private HashSet<GridPosition> activeAffectedCells = new();
        private Coroutine runRoutine;
        private bool isRunning;
        private bool showResult;
        private bool showMismatch;
        private Canvas canvas;
        private RectTransform sidebar;
        private RectTransform boardPanel;
        private RectTransform boardRoot;
        private Text titleText;
        private Text boardTitleText;
        private Text progressText;
        private Text statusText;
        private Text analysisText;
        private Text resultBannerText;
        private Font uiFont;

        private readonly Color pageColor = new(0.18f, 0.29f, 0.47f);
        private readonly Color panelColor = new(0.13f, 0.23f, 0.39f);
        private readonly Color cellColor = new(0.16f, 0.17f, 0.19f);
        private readonly Color buttonColor = new(0.38f, 0.43f, 0.50f);
        private readonly Color affectedTextColor = new(0.54f, 0.93f, 1f);
        private readonly Color wrongTextColor = new(1f, 0.86f, 0.20f);
        private readonly Color clearTextColor = new(0.35f, 0.95f, 0.56f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindAnyObjectByType<RuleReconstructionPrototype>() != null)
            {
                return;
            }

            var gameObject = new GameObject("Rule Reconstruction Prototype");
            gameObject.AddComponent<RuleReconstructionPrototype>();
            DontDestroyOnLoad(gameObject);
        }

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
            uiFont = Font.CreateDynamicFontFromOSFont(new[] { "SF Pro", "Arial", "Helvetica" }, 18);

            CreateCanvas();
            BuildLayout();
            ResetDisplay();
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

            var root = CreatePanel("Root", canvas.transform, pageColor);
            Stretch(root, Vector2.zero, Vector2.one, new Vector2(18f, 12f), new Vector2(-18f, -12f));

            titleText = CreateText("Title", root, string.Empty, 22, TextAnchor.MiddleLeft);
            Anchor(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -34f), new Vector2(0f, 0f));

            var prevButton = CreateButton("PrevButton", root, "Prev", 16, () => MoveStage(-1));
            Anchor(prevButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-160f, -34f), new Vector2(-84f, 0f));

            var nextButton = CreateButton("NextButton", root, "Next", 16, () => MoveStage(1));
            Anchor(nextButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-76f, -34f), new Vector2(0f, 0f));

            sidebar = CreatePanel("Sidebar", root, panelColor);
            Anchor(sidebar, new Vector2(0f, 0f), new Vector2(0.33f, 1f), new Vector2(0f, 0f), new Vector2(-10f, -46f));

            boardPanel = CreatePanel("BoardPanel", root, panelColor);
            Anchor(boardPanel, new Vector2(0.33f, 0f), new Vector2(1f, 1f), new Vector2(10f, 0f), new Vector2(0f, -46f));

            boardTitleText = CreateText("BoardTitle", boardPanel, string.Empty, 22, TextAnchor.MiddleLeft);
            Anchor(boardTitleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -42f), new Vector2(-120f, -8f));

            progressText = CreateText("Progress", boardPanel, string.Empty, 16, TextAnchor.MiddleRight);
            Anchor(progressText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-120f, -42f), new Vector2(-16f, -8f));

            resultBannerText = CreateText("ResultBanner", boardPanel, string.Empty, 54, TextAnchor.MiddleCenter);
            Anchor(resultBannerText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0f, 26f), new Vector2(0f, -26f));
            resultBannerText.fontStyle = FontStyle.Bold;
            resultBannerText.raycastTarget = false;
            var bannerOutline = resultBannerText.gameObject.AddComponent<Outline>();
            bannerOutline.effectColor = new Color(0f, 0f, 0f, 0.72f);
            bannerOutline.effectDistance = new Vector2(2f, -2f);

            BuildSidebar();
            BuildBoardCells();
        }

        private void BuildSidebar()
        {
            directionValueTexts.Clear();
            rangeValueTexts.Clear();
            previewViews.Clear();

            foreach (Transform child in sidebar)
            {
                Destroy(child.gameObject);
            }

            var content = CreatePanel("SidebarContent", sidebar, Color.clear);
            Stretch(content, Vector2.zero, Vector2.one, new Vector2(14f, 118f), new Vector2(-14f, -14f));

            var y = -2f;
            foreach (var color in StageRuleAnalyzer.GetStageColors(CurrentStage))
            {
                AddRuleControls(content, color, y);
                y -= 70f;
            }

            analysisText = CreateText("StageAnalysis", content, string.Empty, 12, TextAnchor.MiddleLeft);
            Anchor(analysisText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, y - 34f), new Vector2(0f, y));

            var actionRoot = CreatePanel("Actions", sidebar, Color.clear);
            Anchor(actionRoot, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(14f, 12f), new Vector2(-14f, 112f));

            var runButton = CreateButton("RunButton", actionRoot, "Run", 16, StartRun);
            Anchor(runButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -30f), Vector2.zero);

            var resetButton = CreateButton("ResetButton", actionRoot, "Reset", 16, ResetDisplay);
            Anchor(resetButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -66f), new Vector2(0f, -36f));

            statusText = CreateText("Status", actionRoot, string.Empty, 15, TextAnchor.MiddleLeft);
            Anchor(statusText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 26f));
        }

        private void AddRuleControls(RectTransform parent, BoxColor color, float top)
        {
            var label = CreateText($"{color}Label", parent, $"{color} Rule", 14, TextAnchor.MiddleLeft);
            Anchor(label.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, top - 18f), new Vector2(0f, top));

            var preview = CreateRulePreview($"{color}Preview", parent);
            Anchor(preview.Root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, top - 62f), new Vector2(52f, top - 20f));
            previewViews[color] = preview;

            var directionButton = CreateButton($"{color}Direction", parent, string.Empty, 11, () => CycleDirection(color));
            Anchor(directionButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(58f, top - 44f), new Vector2(0f, top - 20f));
            directionValueTexts[color] = directionButton.GetComponentInChildren<Text>();

            var rangeButton = CreateButton($"{color}Range", parent, string.Empty, 11, () => CycleRange(color));
            Anchor(rangeButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(58f, top - 68f), new Vector2(0f, top - 46f));
            rangeValueTexts[color] = rangeButton.GetComponentInChildren<Text>();
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

            boardRoot = CreatePanel("Board", boardPanel, Color.clear);
            Anchor(boardRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-182f, -182f), new Vector2(182f, 182f));
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
                    var cell = CreatePanel($"Cell{x}_{y}", boardRoot, cellColor);
                    var label = CreateText("Value", cell, string.Empty, 25, TextAnchor.MiddleCenter);
                    Stretch(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
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
            progressText.text = showResult ? $"{session.AppliedSourceCount}/{CurrentStage.Sources.Count}" : "Target";
            statusText.text = GetStatusText();
            statusText.color = GetResultTextColor();
            resultBannerText.text = GetResultBannerText();
            resultBannerText.color = GetResultTextColor();
            resultBannerText.enabled = !string.IsNullOrEmpty(resultBannerText.text);

            var analysis = session.StageAnalysis;
            var analysisLabel = analysis.HasUniqueSolution ? "Unique" : $"{analysis.MatchingRuleCount} Solutions";
            analysisText.text = $"Stage Check\n{analysisLabel} / Tested {analysis.TestedRuleCount}";
        }

        private void UpdateRuleButtons()
        {
            foreach (var color in StageRuleAnalyzer.GetStageColors(CurrentStage))
            {
                directionValueTexts[color].text = session.GetDirection(color).ToString();
                rangeValueTexts[color].text = session.GetRange(color).ToString();
            }
        }

        private void UpdateRulePreviews()
        {
            foreach (var color in StageRuleAnalyzer.GetStageColors(CurrentStage))
            {
                var preview = previewViews[color];
                var affected = GetPreviewAffectedCells(session.GetDirection(color));

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
            if (displayBoard == null || boardCells.Count != displayBoard.Width * displayBoard.Height)
            {
                return;
            }

            foreach (var view in boardCells)
            {
                var cell = displayBoard.GetCell(view.Position.X, view.Position.Y);
                var isWrong = showMismatch && !cell.Matches(CurrentStage.TargetBoard.GetCell(view.Position.X, view.Position.Y));
                var isAffected = !cell.HasSource && activeAffectedCells.Contains(view.Position);

                view.Background.color = cell.HasSource ? GetSourceColor(cell.SourceColor) : cellColor;
                view.Label.text = cell.HasSource ? string.Empty : cell.Number.ToString();
                view.Label.color = isWrong ? wrongTextColor : isAffected ? affectedTextColor : Color.white;
            }
        }

        private void MoveStage(int delta)
        {
            session.MoveStage(delta);
            ResetDisplay();
            BuildSidebar();
            BuildBoardCells();
        }

        private void CycleDirection(BoxColor color)
        {
            session.CycleDirection(color);
            ResetDisplay();
        }

        private void CycleRange(BoxColor color)
        {
            session.CycleRange(color);
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
                yield return new WaitForSeconds(2.4f);
                displayBoard = CurrentStage.TargetBoard;
                showResult = false;
                showMismatch = false;
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
                return session.ValidationResult.IsClear && IsComplete ? "Clear" : "Simulation";
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
                return "READY";
            }

            return session.ValidationResult.IsClear ? "CLEAR" : $"WRONG {session.ValidationResult.WrongCellCount}";
        }

        private string GetResultBannerText()
        {
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
                return $"RUNNING {session.AppliedSourceCount}/{CurrentStage.Sources.Count}";
            }

            var source = CurrentStage.Sources[session.ActiveSourceIndex];
            return $"APPLYING {source.Color} {session.ActiveSourceIndex + 1}/{CurrentStage.Sources.Count}";
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

        private RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            var rectTransform = gameObject.AddComponent<RectTransform>();
            var image = gameObject.AddComponent<Image>();
            image.color = color;
            return rectTransform;
        }

        private Text CreateText(string name, Transform parent, string text, int fontSize, TextAnchor alignment)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            var label = gameObject.AddComponent<Text>();
            label.font = uiFont;
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        private Button CreateButton(string name, Transform parent, string label, int fontSize, UnityEngine.Events.UnityAction onClick)
        {
            var rectTransform = CreatePanel(name, parent, buttonColor);
            var button = rectTransform.gameObject.AddComponent<Button>();
            button.targetGraphic = rectTransform.GetComponent<Image>();
            button.onClick.AddListener(onClick);

            var text = CreateText("Text", rectTransform, label, fontSize, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private RulePreviewView CreateRulePreview(string name, Transform parent)
        {
            var root = CreatePanel(name, parent, Color.clear);
            var cells = new List<Image>();
            var grid = root.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.cellSize = new Vector2(12f, 12f);
            grid.spacing = new Vector2(2f, 2f);

            for (var index = 0; index < 9; index++)
            {
                var cell = CreatePanel($"PreviewCell{index}", root, new Color(0.28f, 0.36f, 0.48f));
                cells.Add(cell.GetComponent<Image>());
            }

            return new RulePreviewView(root, cells);
        }

        private static HashSet<int> GetPreviewAffectedCells(DirectionType direction)
        {
            var cells = new HashSet<int>();

            switch (direction)
            {
                case DirectionType.Cross:
                    cells.Add(1);
                    cells.Add(3);
                    cells.Add(5);
                    cells.Add(7);
                    break;
                case DirectionType.Diagonal:
                    cells.Add(0);
                    cells.Add(2);
                    cells.Add(6);
                    cells.Add(8);
                    break;
                case DirectionType.Horizontal:
                    cells.Add(3);
                    cells.Add(5);
                    break;
                case DirectionType.Vertical:
                    cells.Add(1);
                    cells.Add(7);
                    break;
                case DirectionType.AllAround:
                    for (var index = 0; index < 9; index++)
                    {
                        if (index != 4)
                        {
                            cells.Add(index);
                        }
                    }
                    break;
            }

            return cells;
        }

        private void ClearCanvasChildren()
        {
            foreach (Transform child in canvas.transform)
            {
                Destroy(child.gameObject);
            }
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

        private bool IsComplete => session.IsComplete;
        private StageData CurrentStage => session.CurrentStage;

        private readonly struct BoardCellView
        {
            public readonly GridPosition Position;
            public readonly Image Background;
            public readonly Text Label;

            public BoardCellView(GridPosition position, Image background, Text label)
            {
                Position = position;
                Background = background;
                Label = label;
            }
        }

        private readonly struct RulePreviewView
        {
            public readonly RectTransform Root;
            public readonly List<Image> Cells;

            public RulePreviewView(RectTransform root, List<Image> cells)
            {
                Root = root;
                Cells = cells;
            }
        }
    }
}
