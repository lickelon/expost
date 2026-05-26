using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Expost.RuleReconstruction
{
    public sealed class RuleReconstructionPrototype : MonoBehaviour
    {
        private static readonly string[] DirectionLabels = { "Cross", "Diagonal", "Horizontal", "Vertical", "AllAround" };
        private static readonly string[] RangeLabels = { "One", "Two" };
        private static readonly BoxColor[] Colors = { BoxColor.Red, BoxColor.Blue };

        private readonly Dictionary<BoxColor, DirectionType> selectedDirections = new();
        private readonly Dictionary<BoxColor, RangeType> selectedRanges = new();
        private List<StageData> stages;
        private int stageIndex;
        private int appliedSourceCount;
        private int activeSourceIndex = -1;
        private BoardState resultBoard;
        private BoardState displayBoard;
        private HashSet<GridPosition> activeAffectedCells = new();
        private ValidationResult validationResult;
        private float sidebarWidth;
        private float targetCellSize;
        private float panelGap;
        private float buttonHeight;
        private Vector2 sidebarScroll;
        private Coroutine runRoutine;
        private bool isRunning;
        private bool showResult;
        private bool showMismatch;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle buttonStyle;
        private GUIStyle cellStyle;
        private GUIStyle redSourceStyle;
        private GUIStyle blueSourceStyle;
        private GUIStyle affectedStyle;
        private GUIStyle wrongStyle;
        private GUIStyle clearStyle;
        private GUIStyle panelStyle;

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

            stages = PrototypeStageFactory.CreateStages();

            foreach (var color in Colors)
            {
                selectedDirections[color] = DirectionType.Cross;
                selectedRanges[color] = RangeType.One;
            }

            ResetDisplay();
        }

        private void OnGUI()
        {
            EnsureInitialized();
            UpdateLayoutMetrics();
            EnsureStyles();

            GUILayout.BeginArea(new Rect(18f, 12f, Screen.width - 36f, Screen.height - 24f));
            DrawHeader();
            GUILayout.Space(8f);
            GUILayout.BeginHorizontal();
            DrawSidebar();
            GUILayout.Space(panelGap);
            DrawTargetPanel();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Rule Reconstruction / {CurrentStage.Name}", titleStyle);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Prev", buttonStyle, GUILayout.Width(90f), GUILayout.Height(buttonHeight)))
            {
                MoveStage(-1);
            }

            if (GUILayout.Button("Next", buttonStyle, GUILayout.Width(90f), GUILayout.Height(buttonHeight)))
            {
                MoveStage(1);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawSidebar()
        {
            GUILayout.BeginVertical(panelStyle, GUILayout.Width(sidebarWidth), GUILayout.Height(Screen.height - 82f));
            sidebarScroll = GUILayout.BeginScrollView(sidebarScroll, false, true);

            foreach (var color in Colors)
            {
                DrawColorControls(color);
                GUILayout.Space(10f);
            }

            GUILayout.EndScrollView();
            GUILayout.Space(8f);
            DrawActionPanel();
            GUILayout.EndVertical();
        }

        private void DrawTargetPanel()
        {
            GUILayout.BeginVertical(panelStyle, GUILayout.ExpandWidth(true), GUILayout.Height(Screen.height - 82f));
            GUILayout.BeginHorizontal();
            GUILayout.Label(GetMainBoardTitle(), titleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(GetProgressText(), bodyStyle);
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            DrawBoard(displayBoard, showMismatch ? CurrentStage.TargetBoard : null, targetCellSize);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void DrawBoard(BoardState board, BoardState comparisonTarget, float size)
        {
            for (var y = board.Height - 1; y >= 0; y--)
            {
                GUILayout.BeginHorizontal();

                for (var x = 0; x < board.Width; x++)
                {
                    var style = GetCellStyle(board, comparisonTarget, x, y);
                    GUILayout.Label(GetCellText(board.GetCell(x, y)), style, GUILayout.Width(size), GUILayout.Height(size));
                }

                GUILayout.EndHorizontal();
            }
        }

        private void DrawColorControls(BoxColor color)
        {
            GUILayout.Label($"{color} Rule", titleStyle);

            var directionIndex = (int)selectedDirections[color];
            var nextDirection = GUILayout.SelectionGrid(directionIndex, DirectionLabels, 2, buttonStyle, GUILayout.Height(buttonHeight * 3f));
            if (nextDirection != directionIndex)
            {
                selectedDirections[color] = (DirectionType)nextDirection;
                ResetDisplay();
            }

            GUILayout.Space(4f);

            var rangeIndex = (int)selectedRanges[color];
            var nextRange = GUILayout.SelectionGrid(rangeIndex, RangeLabels, 2, buttonStyle, GUILayout.Height(buttonHeight));
            if (nextRange != rangeIndex)
            {
                selectedRanges[color] = (RangeType)nextRange;
                ResetDisplay();
            }
        }

        private void DrawActionPanel()
        {
            GUI.enabled = !isRunning;

            if (GUILayout.Button("Run", buttonStyle, GUILayout.Height(buttonHeight)))
            {
                StartRun();
            }

            if (GUILayout.Button("Reset", buttonStyle, GUILayout.Height(buttonHeight)))
            {
                ResetDisplay();
            }

            GUI.enabled = true;

            GUILayout.Label(GetStatusText(), validationResult.IsClear && showResult ? clearStyle : titleStyle);
        }

        private void UpdateLayoutMetrics()
        {
            panelGap = Mathf.Clamp(Screen.width * 0.018f, 10f, 22f);
            sidebarWidth = Mathf.Clamp(Screen.width * 0.34f, 300f, 440f);
            buttonHeight = Mathf.Clamp(Screen.height * 0.065f, 30f, 48f);
            var targetAreaWidth = Screen.width - 36f - sidebarWidth - panelGap - 48f;
            var targetAreaHeight = Screen.height - 116f;
            targetCellSize = Mathf.Clamp(Mathf.Min(targetAreaWidth / 5f, targetAreaHeight / 5f), 54f, 112f);
        }

        private void MoveStage(int delta)
        {
            stageIndex = (stageIndex + delta + stages.Count) % stages.Count;
            ResetDisplay();
        }

        private void ResetSimulationState()
        {
            appliedSourceCount = 0;
            activeSourceIndex = -1;
            activeAffectedCells.Clear();
            resultBoard = RuleSimulator.Simulate(CurrentStage, BuildSelectedRules(), appliedSourceCount);
            validationResult = Validator.Validate(resultBoard, CurrentStage.TargetBoard);
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

            foreach (var color in Colors)
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

        private string GetProgressText()
        {
            return showResult ? $"{appliedSourceCount}/{CurrentStage.Sources.Count}" : "Target";
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

        private GUIStyle GetCellStyle(BoardState board, BoardState comparisonTarget, int x, int y)
        {
            var cell = board.GetCell(x, y);
            var position = new GridPosition(x, y);

            if (comparisonTarget != null && !cell.Matches(comparisonTarget.GetCell(x, y)))
            {
                return wrongStyle;
            }

            if (!cell.HasSource && activeAffectedCells.Contains(position))
            {
                return affectedStyle;
            }

            if (!cell.HasSource)
            {
                return cellStyle;
            }

            return cell.SourceColor == BoxColor.Red ? redSourceStyle : blueSourceStyle;
        }

        private string GetCellText(CellState cell)
        {
            if (cell.HasSource)
            {
                return string.Empty;
            }

            return cell.Number.ToString();
        }

        private void EnsureStyles()
        {
            var titleSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height * 0.045f, 18f, 28f));
            var bodySize = Mathf.RoundToInt(Mathf.Clamp(Screen.height * 0.032f, 14f, 20f));
            var cellFontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height * 0.052f, 20f, 34f));

            if (titleStyle != null && titleStyle.fontSize == titleSize && cellStyle.fontSize == cellFontSize)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = titleSize,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = bodySize,
                normal = { textColor = Color.white }
            };
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = bodySize,
                fontStyle = FontStyle.Bold
            };
            cellStyle = CreateCellStyle(new Color(0.16f, 0.17f, 0.19f), Color.white, cellFontSize);
            redSourceStyle = CreateCellStyle(new Color(0.82f, 0.18f, 0.16f), Color.white, cellFontSize);
            blueSourceStyle = CreateCellStyle(new Color(0.14f, 0.36f, 0.88f), Color.white, cellFontSize);
            affectedStyle = CreateCellStyle(new Color(0.16f, 0.17f, 0.19f), new Color(0.54f, 0.93f, 1f), cellFontSize);
            wrongStyle = CreateCellStyle(new Color(0.16f, 0.17f, 0.19f), new Color(1f, 0.86f, 0.20f), cellFontSize);
            clearStyle = new GUIStyle(titleStyle)
            {
                normal = { textColor = new Color(0.35f, 0.95f, 0.56f) }
            };
            panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(12, 12, 12, 12)
            };
        }

        private static GUIStyle CreateCellStyle(Color background, Color text, int fontSize)
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = fontSize,
                fontStyle = FontStyle.Bold
            };
            style.normal.background = MakeTexture(background);
            style.normal.textColor = text;
            return style;
        }

        private static Texture2D MakeTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private bool IsComplete => appliedSourceCount >= CurrentStage.Sources.Count;
        private StageData CurrentStage => stages[stageIndex];
    }
}
