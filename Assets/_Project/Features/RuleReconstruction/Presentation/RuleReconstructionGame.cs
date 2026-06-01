using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        private readonly List<BoardCellView> boardCells = new();

        private RuleReconstructionSession session;
        private BoardState displayBoard;
        private HashSet<GridPosition> activeAffectedCells = new();
        private Coroutine runRoutine;
        [SerializeField] private RuleReconstructionView view;
        private BoxColor selectedRuleColor;
        private bool isRunning;
        private bool showResult;
        private bool showMismatch;
        private bool showResultBanner;
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
        private RuleReconstructionSidebarView sidebarView;

        private readonly Color cellColor = new(0.16f, 0.17f, 0.19f);
        private readonly Color buttonColor = new(0.28f, 0.37f, 0.50f);
        private readonly Color rulePanelColor = new(0.17f, 0.29f, 0.48f);
        private readonly Color selectedRuleOutlineColor = new(0.54f, 0.93f, 1f);
        private readonly Color directionSlotOutlineColor = new(0.54f, 0.93f, 1f);
        private readonly Color rangeSlotOutlineColor = new(1f, 0.86f, 0.20f);
        private readonly Color effectSlotOutlineColor = new(0.35f, 0.95f, 0.56f);
        private readonly Color affectedTextColor = new(0.54f, 0.93f, 1f);
        private readonly Color wrongTextColor = new(1f, 0.45f, 0.18f);
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

            if (!TryBindSceneView())
            {
                Debug.LogError("RuleReconstructionGame requires scene-defined RuleReconstructionView references.", this);
                return;
            }

            BuildSidebar();
            BuildBoardCells();
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
            return true;
        }

        private void WireStaticButtons()
        {
            if (view == null || !view.HasRequiredReferences())
            {
                return;
            }

            DisableRaycastTarget(view.TestButton.transform.parent);
            view.PrevButton.onClick.RemoveAllListeners();
            view.PrevButton.onClick.AddListener(() => MoveStage(-1));
            view.NextButton.onClick.RemoveAllListeners();
            view.NextButton.onClick.AddListener(() => MoveStage(1));
            view.TestButton.onClick.RemoveAllListeners();
            view.TestButton.onClick.AddListener(StartRun);
            view.TargetButton.onClick.RemoveAllListeners();
            view.TargetButton.onClick.AddListener(ResetDisplay);
            view.RefreshStaticButtonVisuals();
        }

        private static void DisableRaycastTarget(Transform target)
        {
            if (target == null)
            {
                return;
            }

            var graphic = target.GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.raycastTarget = false;
            }
        }

        private void BuildSidebar()
        {
            var stageColors = StageRuleAnalyzer.GetStageColors(CurrentStage);
            if (!ContainsColor(stageColors, selectedRuleColor))
            {
                selectedRuleColor = stageColors[0];
            }

            var builder = new RuleReconstructionSidebarBuilder(
                ui,
                GetSourceColor,
                SelectRuleColor,
                ApplyDirectionBlock,
                ApplyRangeBlock,
                ApplyEffectBlock,
                buttonColor,
                selectedRuleOutlineColor,
                directionSlotOutlineColor,
                rangeSlotOutlineColor,
                effectSlotOutlineColor,
                affectedTextColor);
            sidebarView = builder.Build(sidebar, stageColors);

        }

        private static string GetEffectLabel(EffectType effect)
        {
            return effect == EffectType.SubtractNumber ? "-1" : "+1";
        }

        private void BuildBoardCells()
        {
            boardCells.Clear();

            if (boardPanel == null)
            {
                return;
            }

            var boardView = RuleReconstructionBoardBuilder.Build(ui, boardPanel, boardRoot, resultBannerText, CurrentStage, cellColor);
            boardRoot = boardView.Root;
            boardCells.AddRange(boardView.Cells);
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
                RuleReconstructionIconFactory.ApplyToButton(prevButton, ButtonIconKind.Previous, 18f);
                SetNavigationButtonState(prevButton, !isRunning && !session.IsFirstStage);
            }

            if (nextButton != null)
            {
                var nextAvailable = !isRunning && session.IsCurrentStageCleared && !session.IsLastStage;
                RuleReconstructionIconFactory.ApplyToButton(nextButton, ButtonIconKind.Next, 18f);
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

        private void UpdateRuleButtons()
        {
            foreach (var color in StageRuleAnalyzer.GetStageColors(CurrentStage))
            {
                sidebarView.RulePanelImages[color].color = rulePanelColor;
                sidebarView.RulePanelOutlines[color].enabled = color == selectedRuleColor;
                RuleReconstructionSidebarBuilder.UpdateRangeIcon(sidebarView.RangeIconViews[color], session.GetRange(color));
                sidebarView.EffectIconTexts[color].text = GetEffectLabel(session.GetEffect(color));
            }

            UpdateBlockSelection();
            UpdateBlockPreviews();
        }

        private void UpdateBlockSelection()
        {
            var selectedDirection = session.GetDirection(selectedRuleColor);
            var selectedRange = session.GetRange(selectedRuleColor);
            var selectedEffect = session.GetEffect(selectedRuleColor);

            foreach (var pair in sidebarView.DirectionBlockOutlines)
            {
                pair.Value.enabled = pair.Key == selectedDirection;
            }

            foreach (var pair in sidebarView.RangeBlockOutlines)
            {
                pair.Value.enabled = pair.Key == selectedRange;
            }

            foreach (var pair in sidebarView.EffectBlockOutlines)
            {
                pair.Value.enabled = pair.Key == selectedEffect;
            }
        }

        private void UpdateBlockPreviews()
        {
            var selectedColor = GetSourceColor(selectedRuleColor);
            foreach (var pair in sidebarView.DirectionBlockPreviews)
            {
                pair.Value.Cells[4].color = selectedColor;
            }
        }

        private void UpdateRulePreviews()
        {
            foreach (var color in StageRuleAnalyzer.GetStageColors(CurrentStage))
            {
                sidebarView.SourceSlotImages[color].color = GetSourceColor(color);

                var preview = sidebarView.PreviewViews[color];
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
                return string.Empty;
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

        private bool IsComplete => session.IsComplete;
        private StageData CurrentStage => session.CurrentStage;

    }
}
