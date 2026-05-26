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
        private static readonly DirectionType[] DirectionOptions =
        {
            DirectionType.Cross,
            DirectionType.Diagonal,
            DirectionType.Horizontal,
            DirectionType.Vertical,
            DirectionType.AllAround
        };

        private static readonly RangeType[] RangeOptions =
        {
            RangeType.One,
            RangeType.Two
        };

        private static readonly BoxColor[] AllColors =
        {
            BoxColor.Red,
            BoxColor.Blue,
            BoxColor.Green,
            BoxColor.Yellow
        };

        private readonly Dictionary<BoxColor, DirectionType> selectedDirections = new();
        private readonly Dictionary<BoxColor, RangeType> selectedRanges = new();
        private readonly Dictionary<BoxColor, List<Button>> directionButtons = new();
        private readonly Dictionary<BoxColor, List<Button>> rangeButtons = new();
        private readonly List<BoardCellView> boardCells = new();

        private List<StageData> stages;
        private int stageIndex;
        private int appliedSourceCount;
        private int activeSourceIndex = -1;
        private BoardState resultBoard;
        private BoardState displayBoard;
        private HashSet<GridPosition> activeAffectedCells = new();
        private ValidationResult validationResult;
        private StageAnalysisResult stageAnalysis;
        private Coroutine runRoutine;
        private bool isRunning;
        private bool showResult;
        private bool showMismatch;
        private Canvas canvas;
        private RectTransform sidebar;
        private RectTransform boardRoot;
        private Text titleText;
        private Text boardTitleText;
        private Text progressText;
        private Text statusText;
        private Text analysisText;
        private Font uiFont;

        private readonly Color pageColor = new(0.18f, 0.29f, 0.47f);
        private readonly Color panelColor = new(0.13f, 0.23f, 0.39f);
        private readonly Color cellColor = new(0.16f, 0.17f, 0.19f);
        private readonly Color selectedButtonColor = new(0.82f, 0.86f, 0.92f);
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
            if (stages != null)
            {
                return;
            }

            stages = StageRepository.LoadStages();
            uiFont = Font.CreateDynamicFontFromOSFont(new[] { "SF Pro", "Arial", "Helvetica" }, 18);

            foreach (var color in AllColors)
            {
                selectedDirections[color] = DirectionType.Cross;
                selectedRanges[color] = RangeType.One;
            }

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
            Anchor(sidebar, new Vector2(0f, 0f), new Vector2(0.36f, 1f), new Vector2(0f, 0f), new Vector2(-10f, -46f));

            var boardPanel = CreatePanel("BoardPanel", root, panelColor);
            Anchor(boardPanel, new Vector2(0.36f, 0f), new Vector2(1f, 1f), new Vector2(10f, 0f), new Vector2(0f, -46f));

            boardTitleText = CreateText("BoardTitle", boardPanel, string.Empty, 22, TextAnchor.MiddleLeft);
            Anchor(boardTitleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -42f), new Vector2(-120f, -8f));

            progressText = CreateText("Progress", boardPanel, string.Empty, 16, TextAnchor.MiddleRight);
            Anchor(progressText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-120f, -42f), new Vector2(-16f, -8f));

            boardRoot = CreatePanel("Board", boardPanel, Color.clear);
            Anchor(boardRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-230f, -230f), new Vector2(230f, 230f));

            BuildSidebar();
            BuildBoardCells();
        }

        private void BuildSidebar()
        {
            directionButtons.Clear();
            rangeButtons.Clear();

            foreach (Transform child in sidebar)
            {
                Destroy(child.gameObject);
            }

            var content = CreatePanel("SidebarContent", sidebar, Color.clear);
            Stretch(content, Vector2.zero, Vector2.one, new Vector2(14f, 14f), new Vector2(-14f, -92f));

            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            foreach (var color in StageRuleAnalyzer.GetStageColors(CurrentStage))
            {
                AddRuleControls(content, color);
            }

            analysisText = CreateText("StageAnalysis", content, string.Empty, 15, TextAnchor.MiddleLeft);
            analysisText.rectTransform.sizeDelta = new Vector2(0f, 38f);

            var actionRoot = CreatePanel("Actions", sidebar, Color.clear);
            Anchor(actionRoot, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(14f, 14f), new Vector2(-14f, 82f));

            var actionLayout = actionRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            actionLayout.spacing = 6f;
            actionLayout.childControlHeight = true;
            actionLayout.childControlWidth = true;
            actionLayout.childForceExpandHeight = false;
            actionLayout.childForceExpandWidth = true;

            CreateButton("RunButton", actionRoot, "Run", 17, StartRun).GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 36f);
            CreateButton("ResetButton", actionRoot, "Reset", 17, ResetDisplay).GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 36f);
            statusText = CreateText("Status", actionRoot, string.Empty, 17, TextAnchor.MiddleLeft);
            statusText.rectTransform.sizeDelta = new Vector2(0f, 28f);
        }

        private void AddRuleControls(Transform parent, BoxColor color)
        {
            var label = CreateText($"{color}Label", parent, $"{color} Rule", 18, TextAnchor.MiddleLeft);
            label.rectTransform.sizeDelta = new Vector2(0f, 24f);

            var directionGrid = CreateGrid($"{color}Directions", parent, 2, new Vector2(116f, 30f), 6f);
            directionButtons[color] = new List<Button>();
            foreach (var direction in DirectionOptions)
            {
                var capturedDirection = direction;
                var button = CreateButton($"{color}{direction}", directionGrid.transform, direction.ToString(), 13, () =>
                {
                    selectedDirections[color] = capturedDirection;
                    ResetDisplay();
                });
                directionButtons[color].Add(button);
            }

            var rangeGrid = CreateGrid($"{color}Ranges", parent, 2, new Vector2(116f, 30f), 6f);
            rangeButtons[color] = new List<Button>();
            foreach (var range in RangeOptions)
            {
                var capturedRange = range;
                var button = CreateButton($"{color}{range}", rangeGrid.transform, range.ToString(), 13, () =>
                {
                    selectedRanges[color] = capturedRange;
                    ResetDisplay();
                });
                rangeButtons[color].Add(button);
            }
        }

        private RectTransform CreateGrid(string name, Transform parent, int columns, Vector2 cellSize, float spacing)
        {
            var grid = CreatePanel(name, parent, Color.clear);
            grid.sizeDelta = new Vector2(0f, Mathf.Ceil(5f / columns) * (cellSize.y + spacing));

            var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = columns;
            layout.cellSize = cellSize;
            layout.spacing = new Vector2(spacing, spacing);
            return grid;
        }

        private void BuildBoardCells()
        {
            boardCells.Clear();

            foreach (Transform child in boardRoot)
            {
                Destroy(child.gameObject);
            }

            var grid = boardRoot.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            grid.cellSize = new Vector2(88f, 88f);
            grid.spacing = new Vector2(4f, 4f);

            for (var y = CurrentStage.Height - 1; y >= 0; y--)
            {
                for (var x = 0; x < CurrentStage.Width; x++)
                {
                    var cell = CreatePanel($"Cell{x}_{y}", boardRoot, cellColor);
                    var label = CreateText("Value", cell, string.Empty, 30, TextAnchor.MiddleCenter);
                    Stretch(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                    boardCells.Add(new BoardCellView(new GridPosition(x, y), cell.GetComponent<Image>(), label));
                }
            }
        }

        private void Update()
        {
            if (canvas == null)
            {
                return;
            }

            UpdateTexts();
            UpdateRuleButtons();
            RenderBoard();
        }

        private void UpdateTexts()
        {
            titleText.text = $"Rule Reconstruction / {CurrentStage.Name}";
            boardTitleText.text = GetMainBoardTitle();
            progressText.text = showResult ? $"{appliedSourceCount}/{CurrentStage.Sources.Count}" : "Target";
            statusText.text = GetStatusText();
            statusText.color = validationResult.IsClear && showResult ? clearTextColor : Color.white;

            var analysisLabel = stageAnalysis.HasUniqueSolution ? "Unique" : $"{stageAnalysis.MatchingRuleCount} Solutions";
            analysisText.text = $"Stage Check\n{analysisLabel} / Tested {stageAnalysis.TestedRuleCount}";
        }

        private void UpdateRuleButtons()
        {
            foreach (var color in StageRuleAnalyzer.GetStageColors(CurrentStage))
            {
                for (var index = 0; index < DirectionOptions.Length; index++)
                {
                    SetButtonSelected(directionButtons[color][index], selectedDirections[color] == DirectionOptions[index]);
                }

                for (var index = 0; index < RangeOptions.Length; index++)
                {
                    SetButtonSelected(rangeButtons[color][index], selectedRanges[color] == RangeOptions[index]);
                }
            }
        }

        private void RenderBoard()
        {
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
            stageIndex = (stageIndex + delta + stages.Count) % stages.Count;
            BuildSidebar();
            BuildBoardCells();
            ResetDisplay();
        }

        private void ResetSimulationState()
        {
            appliedSourceCount = 0;
            activeSourceIndex = -1;
            activeAffectedCells.Clear();
            resultBoard = RuleSimulator.Simulate(CurrentStage, BuildSelectedRules(), appliedSourceCount);
            validationResult = Validator.Validate(resultBoard, CurrentStage.TargetBoard);
            stageAnalysis = StageRuleAnalyzer.Analyze(CurrentStage);
        }

        private void ResetDisplay()
        {
            StopRunRoutine();
            ResetSimulationState();
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
            ResetSimulationState();
            displayBoard = resultBoard;

            yield return new WaitForSeconds(0.35f);

            while (!IsComplete)
            {
                activeSourceIndex = appliedSourceCount;
                activeAffectedCells = GetActiveAffectedCells(activeSourceIndex);
                displayBoard = resultBoard;
                yield return new WaitForSeconds(0.25f);

                appliedSourceCount++;
                resultBoard = RuleSimulator.Simulate(CurrentStage, BuildSelectedRules(), appliedSourceCount);
                validationResult = Validator.Validate(resultBoard, CurrentStage.TargetBoard);
                displayBoard = resultBoard;
                yield return new WaitForSeconds(0.45f);
            }

            activeSourceIndex = -1;
            activeAffectedCells.Clear();

            if (!validationResult.IsClear)
            {
                showMismatch = true;
                yield return new WaitForSeconds(1.6f);
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

        private RuleSet BuildSelectedRules()
        {
            var ruleSet = new RuleSet();

            foreach (var color in StageRuleAnalyzer.GetStageColors(CurrentStage))
            {
                ruleSet.Set(color, new Rule(selectedDirections[color], selectedRanges[color], EffectType.AddNumber));
            }

            return ruleSet;
        }

        private HashSet<GridPosition> GetActiveAffectedCells(int sourceIndex)
        {
            var cells = new HashSet<GridPosition>();

            if (sourceIndex < 0 || sourceIndex >= CurrentStage.Sources.Count)
            {
                return cells;
            }

            var source = CurrentStage.Sources[sourceIndex];
            var rules = BuildSelectedRules();

            if (!rules.TryGet(source.Color, out var rule))
            {
                return cells;
            }

            foreach (var position in RuleSimulator.GetAffectedPositions(CurrentStage, source, rule))
            {
                cells.Add(position);
            }

            return cells;
        }

        private string GetMainBoardTitle()
        {
            if (showMismatch)
            {
                return "Result";
            }

            if (showResult)
            {
                return validationResult.IsClear && IsComplete ? "Clear" : "Simulation";
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

            return validationResult.IsClear ? "CLEAR" : $"WRONG {validationResult.WrongCellCount}";
        }

        private string GetRunningStatusText()
        {
            if (activeSourceIndex < 0 || activeSourceIndex >= CurrentStage.Sources.Count)
            {
                return $"RUNNING {appliedSourceCount}/{CurrentStage.Sources.Count}";
            }

            var source = CurrentStage.Sources[activeSourceIndex];
            return $"APPLYING {source.Color} {activeSourceIndex + 1}/{CurrentStage.Sources.Count}";
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

        private void SetButtonSelected(Button button, bool selected)
        {
            var image = button.targetGraphic as Image;
            if (image == null)
            {
                return;
            }

            image.color = selected ? selectedButtonColor : buttonColor;
            var label = button.GetComponentInChildren<Text>();
            label.color = selected ? Color.black : Color.white;
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

        private bool IsComplete => appliedSourceCount >= CurrentStage.Sources.Count;
        private StageData CurrentStage => stages[stageIndex];

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
    }
}
