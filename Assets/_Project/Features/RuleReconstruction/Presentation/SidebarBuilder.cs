using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    public sealed class SidebarBuilder
    {
        private readonly Func<BoxColor, Color> getSourceColor;
        private readonly UnityAction<BoxColor> selectRuleColor;
        private readonly UnityAction<DirectionType> applyDirection;
        private readonly UnityAction<RangeType> applyRange;
        private readonly UnityAction<EffectType> applyEffect;
        private readonly Color affectedTextColor;
        private readonly Color previewIdleColor;
        private readonly RuleCard ruleCardPrefab;
        private readonly BlockButton directionBlockPrefab;
        private readonly BlockButton rangeBlockPrefab;
        private readonly BlockButton effectBlockPrefab;

        public SidebarBuilder(
            Func<BoxColor, Color> getSourceColor,
            UnityAction<BoxColor> selectRuleColor,
            UnityAction<DirectionType> applyDirection,
            UnityAction<RangeType> applyRange,
            UnityAction<EffectType> applyEffect,
            Color affectedTextColor,
            Color previewIdleColor,
            RuleCard ruleCardPrefab,
            BlockButton directionBlockPrefab,
            BlockButton rangeBlockPrefab,
            BlockButton effectBlockPrefab)
        {
            this.getSourceColor = getSourceColor;
            this.selectRuleColor = selectRuleColor;
            this.applyDirection = applyDirection;
            this.applyRange = applyRange;
            this.applyEffect = applyEffect;
            this.affectedTextColor = affectedTextColor;
            this.previewIdleColor = previewIdleColor;
            this.ruleCardPrefab = ruleCardPrefab;
            this.directionBlockPrefab = directionBlockPrefab;
            this.rangeBlockPrefab = rangeBlockPrefab;
            this.effectBlockPrefab = effectBlockPrefab;
        }

        public SidebarView Build(
            RectTransform ruleListRoot,
            RectTransform directionBlockRoot,
            RectTransform rangeBlockRoot,
            RectTransform effectBlockRoot,
            IReadOnlyList<BoxColor> stageColors)
        {
            var view = new SidebarView();
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

        private void AddRuleControls(SidebarView view, RuleCard card, BoxColor color)
        {
            card.name = $"{color}RuleCard";
            card.Button.onClick.RemoveAllListeners();
            card.Button.onClick.AddListener(() => selectRuleColor(color));
            card.PanelOutline.enabled = false;
            card.SelectionIndicator.color = getSourceColor(color);
            card.SelectionIndicator.enabled = false;
            card.SourceSlotImage.color = getSourceColor(color);
            card.EffectIconImage.sprite = IconFactory.Get(ButtonIconKind.Plus);

            view.RulePanelImages[color] = card.PanelImage;
            view.RulePanelOutlines[color] = card.PanelOutline;
            view.RuleSelectionIndicators[color] = card.SelectionIndicator;
            view.SourceSlotImages[color] = card.SourceSlotImage;
            view.PreviewViews[color] = card.DirectionPreview;
            view.RangeIconViews[color] = card.RangeIcon;
            view.EffectIconImages[color] = card.EffectIconImage;
        }

        private void AddDirectionBlockButton(SidebarView view, BlockButton block, DirectionType direction)
        {
            block.name = $"Block{direction}";
            block.Button.onClick.RemoveAllListeners();
            block.Button.onClick.AddListener(() => applyDirection(direction));
            block.SelectionOutline.enabled = false;
            view.DirectionBlockOutlines[direction] = block.SelectionOutline;
            view.DirectionBlockPreviews[direction] = block.DirectionPreview.ToView();

            var affected = PreviewPattern.GetAffectedCells(direction);
            var cells = block.DirectionPreview.Cells;
            for (var index = 0; index < cells.Count; index++)
            {
                cells[index].color = affected.Contains(index) ? affectedTextColor : previewIdleColor;
            }
        }

        private void AddRangeBlockButton(SidebarView view, BlockButton block, RangeType range)
        {
            block.name = $"Block{range}";
            block.Button.onClick.RemoveAllListeners();
            block.Button.onClick.AddListener(() => applyRange(range));
            block.SelectionOutline.enabled = false;
            view.RangeBlockOutlines[range] = block.SelectionOutline;
            UpdateRangeIcon(block.RangeIcon.ToView(), range);
        }

        private void AddEffectBlockButton(SidebarView view, BlockButton block, EffectType effect)
        {
            block.name = $"Block{effect}";
            block.Button.onClick.RemoveAllListeners();
            block.Button.onClick.AddListener(() => applyEffect(effect));
            block.SelectionOutline.enabled = false;
            block.EffectIconImage.sprite = IconFactory.Get(GetEffectIconKind(effect));
            view.EffectBlockOutlines[effect] = block.SelectionOutline;
        }

        public static void UpdateRangeIcon(RangeIconView icon, RangeType range)
        {
            var radius = range == RangeType.One ? 8f : 13f;
            UiFactory.Anchor(icon.Dots[0], new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-3f, radius - 3f), new Vector2(3f, radius + 3f));
            UiFactory.Anchor(icon.Dots[1], new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(radius - 3f, -3f), new Vector2(radius + 3f, 3f));
            UiFactory.Anchor(icon.Dots[2], new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-3f, -radius - 3f), new Vector2(3f, -radius + 3f));
            UiFactory.Anchor(icon.Dots[3], new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-radius - 3f, -3f), new Vector2(-radius + 3f, 3f));
        }

        public static ButtonIconKind GetEffectIconKind(EffectType effect)
        {
            return effect == EffectType.SubtractNumber ? ButtonIconKind.Minus : ButtonIconKind.Plus;
        }
    }
}
