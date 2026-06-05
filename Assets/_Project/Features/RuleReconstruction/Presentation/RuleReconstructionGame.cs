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

        private readonly Color backgroundColor = new(0.96f, 0.96f, 0.92f);
        private readonly Color cellColor = new(0.97f, 0.96f, 0.89f);
        private readonly Color numberTextColor = new(0.18f, 0.20f, 0.19f);
        private readonly Color buttonColor = new(0.89f, 0.91f, 0.84f);
        private readonly Color rulePanelColor = new(0.93f, 0.94f, 0.89f, 0.22f);
        private readonly Color selectedRuleOutlineColor = new(0.20f, 0.52f, 0.72f, 0.82f);
        private readonly Color directionSlotOutlineColor = new(0.18f, 0.66f, 0.80f);
        private readonly Color rangeSlotOutlineColor = new(0.88f, 0.70f, 0.16f);
        private readonly Color effectSlotOutlineColor = new(0.24f, 0.68f, 0.38f);
        private readonly Color affectedTextColor = new(0.12f, 0.58f, 0.70f);
        private readonly Color wrongTextColor = new(0.94f, 0.36f, 0.18f);
        private readonly Color clearTextColor = new(0.18f, 0.62f, 0.34f);

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

            ApplyTheme();
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

        private void ApplyTheme()
        {
            SetImageColor(canvas.transform, backgroundColor);
            SetImageColor(canvas.transform.Find("Root"), backgroundColor);
            SetImageColor(sidebar, Color.clear);
            SetImageColor(boardPanel, Color.clear);
            SetImageColor(testButton.transform.parent, Color.clear);

            titleText.color = new Color(numberTextColor.r, numberTextColor.g, numberTextColor.b, 0.76f);
            titleText.fontSize = 20;
            boardTitleText.color = numberTextColor;
            statusText.color = numberTextColor;
            analysisText.color = numberTextColor;
            resultBannerText.color = clearTextColor;
            resultBannerText.fontSize = 22;
            resultBannerText.alignment = TextAnchor.MiddleCenter;

            ArrangeNavigationButtons();
            ArrangeActionButtons();
            ArrangeResultBanner();
            DisableButtonTransition(prevButton);
            DisableButtonTransition(nextButton);
            DisableButtonTransition(testButton);
            DisableButtonTransition(targetButton);
        }

        private static void SetImageColor(Transform target, Color color)
        {
            if (target == null)
            {
                return;
            }

            var image = target.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }
        }

        private static void DisableButtonTransition(Button button)
        {
            if (button != null)
            {
                button.transition = Selectable.Transition.None;
            }
        }

        private void ArrangeActionButtons()
        {
            ArrangeActionButton(testButton, new Vector2(-26f, 0f));
            ArrangeActionButton(targetButton, new Vector2(26f, 0f));
        }

        private void ArrangeNavigationButtons()
        {
            ArrangeNavigationButton(prevButton, new Vector2(-55f, -18f));
            ArrangeNavigationButton(nextButton, new Vector2(-17f, -18f));
        }

        private static void ArrangeNavigationButton(Button button, Vector2 center)
        {
            if (button == null)
            {
                return;
            }

            var rect = button.GetComponent<RectTransform>();
            RuleReconstructionUiFactory.Anchor(rect, new Vector2(1f, 1f), new Vector2(1f, 1f), center + new Vector2(-16f, -16f), center + new Vector2(16f, 16f));
        }

        private static void ArrangeActionButton(Button button, Vector2 center)
        {
            if (button == null)
            {
                return;
            }

            var rect = button.GetComponent<RectTransform>();
            RuleReconstructionUiFactory.Anchor(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), center + new Vector2(-22f, -18f), center + new Vector2(22f, 18f));
        }

        private void ArrangeResultBanner()
        {
            if (resultBannerText == null)
            {
                return;
            }

            RuleReconstructionUiFactory.Anchor(
                resultBannerText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(136f, 154f),
                new Vector2(220f, 194f));
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
                selectedRuleOutlineColor,
                directionSlotOutlineColor,
                rangeSlotOutlineColor,
                effectSlotOutlineColor,
                affectedTextColor,
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

            var boardView = RuleReconstructionBoardBuilder.Build(boardRoot, resultBannerText, CurrentStage, cellColor, view.BoardCellPrefab);
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
                button.targetGraphic.color = interactable ? new Color(0.89f, 0.91f, 0.84f, 0.42f) : new Color(0.89f, 0.91f, 0.84f, 0.08f);
            }

            var label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.color = interactable ? numberTextColor : new Color(0.17f, 0.20f, 0.22f, 0.38f);
            }

            var icon = button.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null)
            {
                icon.color = interactable ? numberTextColor : new Color(0.17f, 0.20f, 0.22f, 0.24f);
            }
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

            if (button.targetGraphic != null)
            {
                button.targetGraphic.color = interactable ? new Color(0.89f, 0.91f, 0.84f, 0.42f) : new Color(0.89f, 0.91f, 0.84f, 0.12f);
            }

            var icon = button.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null)
            {
                icon.color = interactable ? numberTextColor : new Color(0.17f, 0.20f, 0.22f, 0.28f);
            }
        }

        private void UpdateRuleButtons()
        {
            foreach (var color in StageRuleAnalyzer.GetStageColors(CurrentStage))
            {
                var isSelected = color == selectedRuleColor;
                sidebarView.RulePanelImages[color].color = rulePanelColor;
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
                        cell.color = affected.Contains(index) ? affectedTextColor : new Color(0.78f, 0.82f, 0.78f, 0.52f);
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
                numberTextColor,
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
            resultBannerIcon.color = clearTextColor;
        }

        private Color GetSourceColor(BoxColor color)
        {
            return color switch
            {
                BoxColor.Red => new Color(0.88f, 0.18f, 0.16f),
                BoxColor.Blue => new Color(0.16f, 0.40f, 0.86f),
                BoxColor.Green => new Color(0.16f, 0.66f, 0.34f),
                BoxColor.Yellow => new Color(0.94f, 0.76f, 0.18f),
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
