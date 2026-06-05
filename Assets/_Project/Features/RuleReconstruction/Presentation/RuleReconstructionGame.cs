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
        private RectTransform ruleListRoot;
        private RectTransform directionBlockRoot;
        private RectTransform rangeBlockRoot;
        private RectTransform effectBlockRoot;
        private Text titleText;
        private Text boardTitleText;
        private Text statusText;
        private Text analysisText;
        private Text resultBannerText;
        private Image resultBannerIcon;
        private Button prevButton;
        private Button nextButton;
        private Button testButton;
        private Button targetButton;
        private RuleReconstructionSidebarView sidebarView;

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
            boardRoot = view.BoardRoot;
            ruleListRoot = view.RuleListRoot;
            directionBlockRoot = view.DirectionBlockRoot;
            rangeBlockRoot = view.RangeBlockRoot;
            effectBlockRoot = view.EffectBlockRoot;
            titleText = view.TitleText;
            boardTitleText = view.BoardTitleText;
            statusText = view.StatusText;
            analysisText = view.AnalysisText;
            resultBannerText = view.ResultBannerText;
            resultBannerIcon = view.ResultBannerIcon;
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
            view.TargetButton.onClick.AddListener(ShowTarget);
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
                GetSourceColor,
                SelectRuleColor,
                ApplyDirectionBlock,
                ApplyRangeBlock,
                ApplyEffectBlock,
                view.AffectedTextColor,
                view.PreviewIdleColor,
                view.RuleCardPrefab,
                view.DirectionBlockPrefab,
                view.RangeBlockPrefab,
                view.EffectBlockPrefab);
            sidebarView = builder.Build(ruleListRoot, directionBlockRoot, rangeBlockRoot, effectBlockRoot, stageColors);

        }

        private void BuildBoardCells()
        {
            boardCells.Clear();

            if (boardPanel == null)
            {
                return;
            }

            var boardView = RuleReconstructionBoardBuilder.Build(boardRoot, resultBannerText, CurrentStage, view.BoardCellPrefab);
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
            titleText.text = $"{session.StageIndex + 1:00}/{session.StageCount:00}";
            boardTitleText.text = string.Empty;
            statusText.text = string.Empty;
            resultBannerText.text = string.Empty;
            resultBannerText.enabled = false;
            UpdateResultBannerIcon();

            analysisText.text = string.Empty;
            UpdateNavigationButtons();
            UpdateActionButtons();
        }

        private void UpdateNavigationButtons()
        {
            if (prevButton != null)
            {
                RuleReconstructionIconFactory.ApplyToButton(prevButton, ButtonIconKind.Previous);
                SetNavigationButtonState(prevButton, !isRunning && !session.IsFirstStage);
            }

            if (nextButton != null)
            {
                var nextAvailable = !isRunning && session.IsCurrentStageCleared && !session.IsLastStage;
                RuleReconstructionIconFactory.ApplyToButton(nextButton, ButtonIconKind.Next);
                SetNavigationButtonState(nextButton, nextAvailable);
            }
        }

        private void SetNavigationButtonState(Button button, bool interactable)
        {
            button.interactable = interactable;
        }

        private void UpdateActionButtons()
        {
            SetActionButtonState(testButton, !showResult || showMismatch);
            SetActionButtonState(targetButton, showResult && !showMismatch);
        }

        private void SetActionButtonState(Button button, bool active)
        {
            if (button == null)
            {
                return;
            }

            var interactable = active && !isRunning;
            button.interactable = interactable;
        }

        private void UpdateRuleButtons()
        {
            foreach (var color in StageRuleAnalyzer.GetStageColors(CurrentStage))
            {
                var isSelected = color == selectedRuleColor;
                sidebarView.RulePanelOutlines[color].enabled = false;
                sidebarView.RuleSelectionIndicators[color].color = GetSourceColor(color);
                sidebarView.RuleSelectionIndicators[color].enabled = isSelected;
                RuleReconstructionSidebarBuilder.UpdateRangeIcon(sidebarView.RangeIconViews[color], session.GetRange(color));
                sidebarView.EffectIconImages[color].sprite = RuleReconstructionIconFactory.Get(RuleReconstructionSidebarBuilder.GetEffectIconKind(session.GetEffect(color)));
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
                        cell.color = affected.Contains(index) ? view.AffectedTextColor : view.PreviewIdleColor;
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
                view.WrongTextColor,
                view.NeedMoreTextColor,
                view.AffectedTextColor,
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
            ResetAttemptState();
        }

        private void ApplyRangeBlock(RangeType range)
        {
            session.SetRange(selectedRuleColor, range);
            ResetAttemptState();
        }

        private void ApplyEffectBlock(EffectType effect)
        {
            session.SetEffect(selectedRuleColor, effect);
            ResetAttemptState();
        }

        private void ResetDisplay()
        {
            ShowTarget();
        }

        private void ShowTarget()
        {
            StopRunRoutine();
            session.ResetSimulation();
            activeAffectedCells.Clear();
            displayBoard = CurrentStage.TargetBoard;
            showResult = false;
            showMismatch = false;
            showResultBanner = false;
        }

        private void ResetAttemptState()
        {
            StopRunRoutine();
            session.ResetSimulation();
            activeAffectedCells.Clear();
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

        private void UpdateResultBannerIcon()
        {
            if (resultBannerIcon == null)
            {
                return;
            }

            var showClear = showResultBanner && !showMismatch && showResult && IsComplete && session.ValidationResult.IsClear;
            resultBannerIcon.enabled = showClear;
            resultBannerIcon.color = view.ClearTextColor;
        }

        private Color GetSourceColor(BoxColor color)
        {
            return view.GetSourceColor(color);
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
