using System.Collections.Generic;

namespace Expost.RuleReconstruction
{
    public static class StageRuleAnalyzer
    {
        public static List<BoxColor> GetStageColors(StageData stage)
        {
            var colors = new List<BoxColor>();

            foreach (var source in stage.Sources)
            {
                if (!colors.Contains(source.Color))
                {
                    colors.Add(source.Color);
                }
            }

            return colors;
        }
    }
}
