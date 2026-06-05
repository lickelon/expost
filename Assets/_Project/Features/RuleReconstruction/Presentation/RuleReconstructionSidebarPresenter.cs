using System;
using System.Collections.Generic;
using UnityEngine;

namespace Expost.RuleReconstruction
{
    public sealed class RuleReconstructionSidebarPresenter
    {
        private readonly RuleReconstructionView view;
        private readonly RuleReconstructionSession session;
        private readonly Action resetAttemptState;

        private RuleReconstructionSidebarView sidebarView;
        private BoxColor selectedRuleColor;

        public RuleReconstructionSidebarPresenter(RuleReconstructionView view, RuleReconstructionSession session, Action resetAttemptState)
        {
            this.view = view;
            this.session = session;
            this.resetAttemptState = resetAttemptState;
            selectedRuleColor = StageRuleAnalyzer.GetStageColors(session.CurrentStage)[0];
        }

        public void Rebuild()
        {
            var stageColors = StageRuleAnalyzer.GetStageColors(session.CurrentStage);
            if (!ContainsColor(stageColors, selectedRuleColor))
            {
                selectedRuleColor = stageColors[0];
            }

            var builder = new RuleReconstructionSidebarBuilder(
                view.GetSourceColor,
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
            sidebarView = builder.Build(view.RuleListRoot, view.DirectionBlockRoot, view.RangeBlockRoot, view.EffectBlockRoot, stageColors);
        }

        public void Render()
        {
            if (sidebarView == null)
            {
                return;
            }

            UpdateRuleButtons();
            UpdateRulePreviews();
        }

        private void SelectRuleColor(BoxColor color)
        {
            selectedRuleColor = color;
            UpdateRuleButtons();
        }

        private void ApplyDirectionBlock(DirectionType direction)
        {
            session.SetDirection(selectedRuleColor, direction);
            resetAttemptState();
        }

        private void ApplyRangeBlock(RangeType range)
        {
            session.SetRange(selectedRuleColor, range);
            resetAttemptState();
        }

        private void ApplyEffectBlock(EffectType effect)
        {
            session.SetEffect(selectedRuleColor, effect);
            resetAttemptState();
        }

        private void UpdateRuleButtons()
        {
            foreach (var color in StageRuleAnalyzer.GetStageColors(session.CurrentStage))
            {
                var isSelected = color == selectedRuleColor;
                sidebarView.RulePanelOutlines[color].enabled = false;
                sidebarView.RuleSelectionIndicators[color].color = view.GetSourceColor(color);
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
            var selectedColor = view.GetSourceColor(selectedRuleColor);
            foreach (var pair in sidebarView.DirectionBlockPreviews)
            {
                pair.Value.Cells[4].color = selectedColor;
            }
        }

        private void UpdateRulePreviews()
        {
            foreach (var color in StageRuleAnalyzer.GetStageColors(session.CurrentStage))
            {
                sidebarView.SourceSlotImages[color].color = view.GetSourceColor(color);

                var preview = sidebarView.PreviewViews[color];
                var affected = RuleReconstructionPreviewPattern.GetAffectedCells(session.GetDirection(color));

                for (var index = 0; index < preview.Cells.Count; index++)
                {
                    var cell = preview.Cells[index];
                    if (index == 4)
                    {
                        cell.color = view.GetSourceColor(color);
                    }
                    else
                    {
                        cell.color = affected.Contains(index) ? view.AffectedTextColor : view.PreviewIdleColor;
                    }
                }
            }
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
    }
}
