using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    public sealed class RuleReconstructionSidebarBuilder
    {
        private readonly RuleReconstructionUiFactory ui;
        private readonly Func<BoxColor, Color> getSourceColor;
        private readonly UnityAction<BoxColor> selectRuleColor;
        private readonly UnityAction<DirectionType> applyDirection;
        private readonly UnityAction<RangeType> applyRange;
        private readonly UnityAction<EffectType> applyEffect;
        private readonly Color buttonColor;
        private readonly Color selectedRuleOutlineColor;
        private readonly Color directionSlotOutlineColor;
        private readonly Color rangeSlotOutlineColor;
        private readonly Color effectSlotOutlineColor;
        private readonly Color affectedTextColor;

        public RuleReconstructionSidebarBuilder(
            RuleReconstructionUiFactory ui,
            Func<BoxColor, Color> getSourceColor,
            UnityAction<BoxColor> selectRuleColor,
            UnityAction<DirectionType> applyDirection,
            UnityAction<RangeType> applyRange,
            UnityAction<EffectType> applyEffect,
            Color buttonColor,
            Color selectedRuleOutlineColor,
            Color directionSlotOutlineColor,
            Color rangeSlotOutlineColor,
            Color effectSlotOutlineColor,
            Color affectedTextColor)
        {
            this.ui = ui;
            this.getSourceColor = getSourceColor;
            this.selectRuleColor = selectRuleColor;
            this.applyDirection = applyDirection;
            this.applyRange = applyRange;
            this.applyEffect = applyEffect;
            this.buttonColor = buttonColor;
            this.selectedRuleOutlineColor = selectedRuleOutlineColor;
            this.directionSlotOutlineColor = directionSlotOutlineColor;
            this.rangeSlotOutlineColor = rangeSlotOutlineColor;
            this.effectSlotOutlineColor = effectSlotOutlineColor;
            this.affectedTextColor = affectedTextColor;
        }

        public RuleReconstructionSidebarView Build(RectTransform sidebar, IReadOnlyList<BoxColor> stageColors)
        {
            foreach (Transform child in sidebar)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }

            var view = new RuleReconstructionSidebarView();
            var content = ui.CreatePanel("SidebarContent", sidebar, Color.clear);
            RuleReconstructionUiFactory.Stretch(content, Vector2.zero, Vector2.one, new Vector2(14f, 198f), new Vector2(-14f, -14f));

            var y = -2f;
            foreach (var color in stageColors)
            {
                AddRuleControls(view, content, color, y);
                y -= 58f;
            }

            var actionRoot = ui.CreatePanel("Actions", sidebar, Color.clear);
            RuleReconstructionUiFactory.Anchor(actionRoot, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(14f, 12f), new Vector2(-14f, 188f));
            AddBlockTray(view, actionRoot);

            return view;
        }

        public Text CreateFallbackAnalysisText(RectTransform parent, float top)
        {
            var text = ui.CreateText("StageAnalysis", parent, string.Empty, 12, TextAnchor.MiddleLeft);
            RuleReconstructionUiFactory.Anchor(text.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, top), new Vector2(0f, top));
            text.gameObject.SetActive(false);
            return text;
        }

        public Button CreateFallbackRunButton(RectTransform actionRoot, UnityAction onClick)
        {
            return ui.CreateButton("TestButton", actionRoot, string.Empty, 16, onClick);
        }

        public Button CreateFallbackTargetButton(RectTransform actionRoot, UnityAction onClick)
        {
            return ui.CreateButton("TargetButton", actionRoot, string.Empty, 16, onClick);
        }

        public Text CreateFallbackStatusText(RectTransform actionRoot)
        {
            var status = ui.CreateText("Status", actionRoot, string.Empty, 15, TextAnchor.MiddleLeft);
            RuleReconstructionUiFactory.Anchor(status.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 18f));
            return status;
        }

        private void AddRuleControls(RuleReconstructionSidebarView view, RectTransform parent, BoxColor color, float top)
        {
            var panel = ui.CreateButton($"{color}RulePanel", parent, string.Empty, 1, () => selectRuleColor(color));
            var panelRect = panel.GetComponent<RectTransform>();
            RuleReconstructionUiFactory.Anchor(panelRect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, top - 54f), new Vector2(0f, top));
            view.RulePanelImages[color] = panel.targetGraphic as Image;
            var outline = AddOutline(panel.gameObject, selectedRuleOutlineColor, 1f);
            outline.enabled = false;
            view.RulePanelOutlines[color] = outline;

            var sourceButton = ui.CreateIconButton($"{color}Source", panelRect, () => selectRuleColor(color));
            RuleReconstructionUiFactory.Anchor(sourceButton.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, -18f), new Vector2(44f, 18f));
            AddOutline(sourceButton.gameObject, getSourceColor(color), 1f);
            var sourceSwatch = ui.CreatePanel($"{color}SourceSwatch", sourceButton.transform, getSourceColor(color));
            RuleReconstructionUiFactory.Stretch(sourceSwatch, Vector2.zero, Vector2.one, new Vector2(8f, 8f), new Vector2(-8f, -8f));
            view.SourceSlotImages[color] = sourceSwatch.GetComponent<Image>();

            var directionButton = ui.CreateIconButton($"{color}Direction", panelRect, () => selectRuleColor(color));
            RuleReconstructionUiFactory.Anchor(directionButton.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(52f, -18f), new Vector2(88f, 18f));
            AddOutline(directionButton.gameObject, directionSlotOutlineColor, 1f);
            var directionPreview = CreateRulePreview($"{color}DirectionIcon", directionButton.transform);
            AnchorIconPreview(directionPreview.Root);
            ConfigureSmallPreview(directionPreview.Root);
            view.PreviewViews[color] = directionPreview;

            var rangeButton = ui.CreateIconButton($"{color}Range", panelRect, () => selectRuleColor(color));
            RuleReconstructionUiFactory.Anchor(rangeButton.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(96f, -18f), new Vector2(132f, 18f));
            AddOutline(rangeButton.gameObject, rangeSlotOutlineColor, 1f);
            view.RangeIconViews[color] = CreateRangeIcon($"{color}RangeIcon", rangeButton.transform);

            var effectButton = ui.CreateIconButton($"{color}Effect", panelRect, () => selectRuleColor(color));
            RuleReconstructionUiFactory.Anchor(effectButton.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(140f, -18f), new Vector2(176f, 18f));
            AddOutline(effectButton.gameObject, effectSlotOutlineColor, 1f);
            view.EffectIconTexts[color] = CreateEffectText($"{color}EffectText", effectButton.transform, EffectType.AddNumber, 17);
        }

        private void AddBlockTray(RuleReconstructionSidebarView view, RectTransform parent)
        {
            var directionRoot = CreateBlockRoot(parent, "DirectionBlocks", directionSlotOutlineColor, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -40f), new Vector2(0f, -2f), 5);
            AddDirectionBlockButton(view, directionRoot, DirectionType.Cross);
            AddDirectionBlockButton(view, directionRoot, DirectionType.Diagonal);
            AddDirectionBlockButton(view, directionRoot, DirectionType.Horizontal);
            AddDirectionBlockButton(view, directionRoot, DirectionType.Vertical);
            AddDirectionBlockButton(view, directionRoot, DirectionType.AllAround);

            var rangeRoot = CreateBlockRoot(parent, "RangeBlocks", rangeSlotOutlineColor, new Vector2(0f, 1f), new Vector2(0.52f, 1f), new Vector2(0f, -84f), new Vector2(-4f, -46f), 2);
            AddRangeBlockButton(view, rangeRoot, RangeType.One);
            AddRangeBlockButton(view, rangeRoot, RangeType.Two);

            var effectRoot = CreateBlockRoot(parent, "EffectBlocks", effectSlotOutlineColor, new Vector2(0.52f, 1f), new Vector2(1f, 1f), new Vector2(4f, -84f), new Vector2(0f, -46f), 2);
            AddEffectBlockButton(view, effectRoot, EffectType.AddNumber);
            AddEffectBlockButton(view, effectRoot, EffectType.SubtractNumber);
        }

        private RectTransform CreateBlockRoot(RectTransform parent, string name, Color outlineColor, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, int columns)
        {
            var root = ui.CreatePanel(name, parent, new Color(0.15f, 0.25f, 0.41f));
            RuleReconstructionUiFactory.Anchor(root, anchorMin, anchorMax, offsetMin, offsetMax);
            AddOutline(root.gameObject, outlineColor, 1f);
            var grid = root.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            grid.cellSize = new Vector2(38f, 30f);
            grid.spacing = new Vector2(7f, 0f);
            grid.padding = new RectOffset(10, 0, 5, 0);
            return root;
        }

        private void AddDirectionBlockButton(RuleReconstructionSidebarView view, RectTransform parent, DirectionType direction)
        {
            var button = ui.CreateIconButton($"Block{direction}", parent, () => applyDirection(direction));
            view.DirectionBlockOutlines[direction] = AddSelectionOutline(button.gameObject, directionSlotOutlineColor);
            var icon = CreateRulePreview($"{direction}Icon", button.transform);
            view.DirectionBlockPreviews[direction] = icon;
            AnchorIconPreview(icon.Root);
            ConfigureSmallPreview(icon.Root);

            var affected = RuleReconstructionPreviewPattern.GetAffectedCells(direction);
            for (var index = 0; index < icon.Cells.Count; index++)
            {
                icon.Cells[index].color = affected.Contains(index) ? affectedTextColor : new Color(0.26f, 0.34f, 0.46f);
            }
        }

        private void AddRangeBlockButton(RuleReconstructionSidebarView view, RectTransform parent, RangeType range)
        {
            var button = ui.CreateIconButton($"Block{range}", parent, () => applyRange(range));
            view.RangeBlockOutlines[range] = AddSelectionOutline(button.gameObject, rangeSlotOutlineColor);
            var icon = CreateRangeIcon($"{range}Icon", button.transform);
            UpdateRangeIcon(icon, range);
        }

        private void AddEffectBlockButton(RuleReconstructionSidebarView view, RectTransform parent, EffectType effect)
        {
            var button = ui.CreateIconButton($"Block{effect}", parent, () => applyEffect(effect));
            view.EffectBlockOutlines[effect] = AddSelectionOutline(button.gameObject, effectSlotOutlineColor);
            CreateEffectText($"{effect}Text", button.transform, effect, 17);
        }

        private Outline AddSelectionOutline(GameObject target, Color color)
        {
            var outline = AddOutline(target, color, 2f);
            outline.enabled = false;
            return outline;
        }

        private static Outline AddOutline(GameObject target, Color color, float size)
        {
            var outline = target.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(size, -size);
            return outline;
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
                dots.Add(ui.CreatePanel("RangeDot", root, new Color(0.54f, 0.93f, 1f, 0.62f)));
            }

            return new RangeIconView(root, center, dots);
        }

        public static void UpdateRangeIcon(RangeIconView icon, RangeType range)
        {
            var radius = range == RangeType.One ? 8f : 13f;
            RuleReconstructionUiFactory.Anchor(icon.Dots[0], new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-3f, radius - 3f), new Vector2(3f, radius + 3f));
            RuleReconstructionUiFactory.Anchor(icon.Dots[1], new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(radius - 3f, -3f), new Vector2(radius + 3f, 3f));
            RuleReconstructionUiFactory.Anchor(icon.Dots[2], new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-3f, -radius - 3f), new Vector2(3f, -radius + 3f));
            RuleReconstructionUiFactory.Anchor(icon.Dots[3], new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-radius - 3f, -3f), new Vector2(-radius + 3f, 3f));
        }

        private Text CreateEffectText(string name, Transform parent, EffectType effect, int fontSize)
        {
            var text = ui.CreateText(name, parent, effect == EffectType.SubtractNumber ? "-1" : "+1", fontSize, TextAnchor.MiddleCenter);
            text.fontStyle = FontStyle.Bold;
            text.color = affectedTextColor;
            RuleReconstructionUiFactory.Stretch(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return text;
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
    }
}
