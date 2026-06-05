using System.Collections.Generic;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    public sealed class RuleReconstructionSidebarView
    {
        public readonly Dictionary<BoxColor, Image> SourceSlotImages = new();
        public readonly Dictionary<BoxColor, RulePreviewView> PreviewViews = new();
        public readonly Dictionary<BoxColor, RangeIconView> RangeIconViews = new();
        public readonly Dictionary<BoxColor, Image> EffectIconImages = new();
        public readonly Dictionary<BoxColor, Image> RulePanelImages = new();
        public readonly Dictionary<BoxColor, Image> RuleSelectionIndicators = new();
        public readonly Dictionary<BoxColor, Outline> RulePanelOutlines = new();
        public readonly Dictionary<DirectionType, RulePreviewView> DirectionBlockPreviews = new();
        public readonly Dictionary<DirectionType, Outline> DirectionBlockOutlines = new();
        public readonly Dictionary<RangeType, Outline> RangeBlockOutlines = new();
        public readonly Dictionary<EffectType, Outline> EffectBlockOutlines = new();
    }
}
