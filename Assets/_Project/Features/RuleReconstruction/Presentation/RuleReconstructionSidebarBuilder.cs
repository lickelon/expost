using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    public sealed class RuleReconstructionSidebarBuilder
    {
        private static readonly Color RulePanelSurfaceColor = new(0.93f, 0.94f, 0.89f, 0.22f);
        private static readonly Color PreviewIdleColor = new(0.80f, 0.84f, 0.80f, 0.48f);

        private readonly Func<BoxColor, Color> getSourceColor;
        private readonly UnityAction<BoxColor> selectRuleColor;
        private readonly UnityAction<DirectionType> applyDirection;
        private readonly UnityAction<RangeType> applyRange;
        private readonly UnityAction<EffectType> applyEffect;
        private readonly Color selectedRuleOutlineColor;
        private readonly Color directionSlotOutlineColor;
        private readonly Color rangeSlotOutlineColor;
        private readonly Color effectSlotOutlineColor;
        private readonly Color affectedTextColor;
        private readonly RuleReconstructionRuleCard ruleCardPrefab;
        private readonly RuleReconstructionBlockButton directionBlockPrefab;
        private readonly RuleReconstructionBlockButton rangeBlockPrefab;
        private readonly RuleReconstructionBlockButton effectBlockPrefab;

        public RuleReconstructionSidebarBuilder(
            Func<BoxColor, Color> getSourceColor,
            UnityAction<BoxColor> selectRuleColor,
            UnityAction<DirectionType> applyDirection,
            UnityAction<RangeType> applyRange,
            UnityAction<EffectType> applyEffect,
            Color selectedRuleOutlineColor,
            Color directionSlotOutlineColor,
            Color rangeSlotOutlineColor,
            Color effectSlotOutlineColor,
            Color affectedTextColor,
            RuleReconstructionRuleCard ruleCardPrefab,
            RuleReconstructionBlockButton directionBlockPrefab,
            RuleReconstructionBlockButton rangeBlockPrefab,
            RuleReconstructionBlockButton effectBlockPrefab)
        {
            this.getSourceColor = getSourceColor;
            this.selectRuleColor = selectRuleColor;
            this.applyDirection = applyDirection;
            this.applyRange = applyRange;
            this.applyEffect = applyEffect;
            this.selectedRuleOutlineColor = selectedRuleOutlineColor;
            this.directionSlotOutlineColor = directionSlotOutlineColor;
            this.rangeSlotOutlineColor = rangeSlotOutlineColor;
            this.effectSlotOutlineColor = effectSlotOutlineColor;
            this.affectedTextColor = affectedTextColor;
            this.ruleCardPrefab = ruleCardPrefab;
            this.directionBlockPrefab = directionBlockPrefab;
            this.rangeBlockPrefab = rangeBlockPrefab;
            this.effectBlockPrefab = effectBlockPrefab;
        }

        public RuleReconstructionSidebarView Build(
            RectTransform ruleListRoot,
            RectTransform directionBlockRoot,
            RectTransform rangeBlockRoot,
            RectTransform effectBlockRoot,
            IReadOnlyList<BoxColor> stageColors)
        {
            var view = new RuleReconstructionSidebarView();
            var ruleCards = EnsureChildren(ruleListRoot, ruleCardPrefab, stageColors.Count);
            for (var index = 0; index < stageColors.Count; index++)
            {
                AddRuleControls(view, ruleCards[index], stageColors[index]);
            }

            var directions = new[]
            {
                DirectionType.Cross,
                DirectionType.Diagonal,
                DirectionType.Horizontal,
                DirectionType.Vertical,
                DirectionType.AllAround
            };
            var directionBlocks = EnsureChildren(directionBlockRoot, directionBlockPrefab, directions.Length);
            for (var index = 0; index < directions.Length; index++)
            {
                AddDirectionBlockButton(view, directionBlocks[index], directions[index]);
            }

            var ranges = new[] { RangeType.One, RangeType.Two };
            var rangeBlocks = EnsureChildren(rangeBlockRoot, rangeBlockPrefab, ranges.Length);
            for (var index = 0; index < ranges.Length; index++)
            {
                AddRangeBlockButton(view, rangeBlocks[index], ranges[index]);
            }

            var effects = new[] { EffectType.AddNumber, EffectType.SubtractNumber };
            var effectBlocks = EnsureChildren(effectBlockRoot, effectBlockPrefab, effects.Length);
            for (var index = 0; index < effects.Length; index++)
            {
                AddEffectBlockButton(view, effectBlocks[index], effects[index]);
            }

            return view;
        }

        private static List<T> EnsureChildren<T>(RectTransform root, T prefab, int requiredCount)
            where T : Component
        {
            var children = new List<T>();
            foreach (Transform child in root)
            {
                if (child.TryGetComponent<T>(out var component))
                {
                    children.Add(component);
                }
            }

            while (children.Count < requiredCount)
            {
                children.Add(UnityEngine.Object.Instantiate(prefab, root));
            }

            for (var index = 0; index < children.Count; index++)
            {
                children[index].gameObject.SetActive(index < requiredCount);
            }

            return children;
        }

        private void AddRuleControls(RuleReconstructionSidebarView view, RuleReconstructionRuleCard card, BoxColor color)
        {
            card.name = $"{color}RuleCard";
            card.Button.onClick.RemoveAllListeners();
            card.Button.onClick.AddListener(() => selectRuleColor(color));
            card.PanelImage.color = RulePanelSurfaceColor;
            card.PanelOutline.effectColor = selectedRuleOutlineColor;
            card.PanelOutline.enabled = false;
            card.SelectionIndicator.color = getSourceColor(color);
            card.SelectionIndicator.enabled = false;
            card.SourceSlotImage.color = getSourceColor(color);
            card.EffectIconImage.sprite = RuleReconstructionIconFactory.Get(ButtonIconKind.Plus);
            card.EffectIconImage.color = affectedTextColor;

            view.RulePanelImages[color] = card.PanelImage;
            view.RulePanelOutlines[color] = card.PanelOutline;
            view.RuleSelectionIndicators[color] = card.SelectionIndicator;
            view.SourceSlotImages[color] = card.SourceSlotImage;
            view.PreviewViews[color] = card.DirectionPreview;
            view.RangeIconViews[color] = card.RangeIcon;
            view.EffectIconImages[color] = card.EffectIconImage;
        }

        private void AddDirectionBlockButton(RuleReconstructionSidebarView view, RuleReconstructionBlockButton block, DirectionType direction)
        {
            block.name = $"Block{direction}";
            block.Button.onClick.RemoveAllListeners();
            block.Button.onClick.AddListener(() => applyDirection(direction));
            block.SelectionOutline.effectColor = directionSlotOutlineColor;
            block.SelectionOutline.enabled = false;
            view.DirectionBlockOutlines[direction] = block.SelectionOutline;
            view.DirectionBlockPreviews[direction] = block.DirectionPreview.ToView();

            var affected = RuleReconstructionPreviewPattern.GetAffectedCells(direction);
            var cells = block.DirectionPreview.Cells;
            for (var index = 0; index < cells.Count; index++)
            {
                cells[index].color = affected.Contains(index) ? affectedTextColor : PreviewIdleColor;
            }
        }

        private void AddRangeBlockButton(RuleReconstructionSidebarView view, RuleReconstructionBlockButton block, RangeType range)
        {
            block.name = $"Block{range}";
            block.Button.onClick.RemoveAllListeners();
            block.Button.onClick.AddListener(() => applyRange(range));
            block.SelectionOutline.effectColor = rangeSlotOutlineColor;
            block.SelectionOutline.enabled = false;
            view.RangeBlockOutlines[range] = block.SelectionOutline;
            UpdateRangeIcon(block.RangeIcon.ToView(), range);
        }

        private void AddEffectBlockButton(RuleReconstructionSidebarView view, RuleReconstructionBlockButton block, EffectType effect)
        {
            block.name = $"Block{effect}";
            block.Button.onClick.RemoveAllListeners();
            block.Button.onClick.AddListener(() => applyEffect(effect));
            block.SelectionOutline.effectColor = effectSlotOutlineColor;
            block.SelectionOutline.enabled = false;
            block.EffectIconImage.sprite = RuleReconstructionIconFactory.Get(GetEffectIconKind(effect));
            block.EffectIconImage.color = affectedTextColor;
            view.EffectBlockOutlines[effect] = block.SelectionOutline;
        }

        public static void UpdateRangeIcon(RangeIconView icon, RangeType range)
        {
            var radius = range == RangeType.One ? 8f : 13f;
            RuleReconstructionUiFactory.Anchor(icon.Dots[0], new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-3f, radius - 3f), new Vector2(3f, radius + 3f));
            RuleReconstructionUiFactory.Anchor(icon.Dots[1], new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(radius - 3f, -3f), new Vector2(radius + 3f, 3f));
            RuleReconstructionUiFactory.Anchor(icon.Dots[2], new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-3f, -radius - 3f), new Vector2(3f, -radius + 3f));
            RuleReconstructionUiFactory.Anchor(icon.Dots[3], new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-radius - 3f, -3f), new Vector2(-radius + 3f, 3f));
        }

        public static ButtonIconKind GetEffectIconKind(EffectType effect)
        {
            return effect == EffectType.SubtractNumber ? ButtonIconKind.Minus : ButtonIconKind.Plus;
        }
    }
}
