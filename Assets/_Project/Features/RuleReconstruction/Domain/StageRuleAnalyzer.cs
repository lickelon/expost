using System.Collections.Generic;

namespace Expost.RuleReconstruction
{
    public readonly struct StageAnalysisResult
    {
        public readonly int TestedRuleCount;
        public readonly int MatchingRuleCount;

        public StageAnalysisResult(int testedRuleCount, int matchingRuleCount)
        {
            TestedRuleCount = testedRuleCount;
            MatchingRuleCount = matchingRuleCount;
        }

        public bool HasUniqueSolution => MatchingRuleCount == 1;
    }

    public static class StageRuleAnalyzer
    {
        private static readonly DirectionType[] DirectionCandidates =
        {
            DirectionType.Cross,
            DirectionType.Diagonal,
            DirectionType.Horizontal,
            DirectionType.Vertical,
            DirectionType.AllAround
        };

        private static readonly RangeType[] RangeCandidates =
        {
            RangeType.One,
            RangeType.Two
        };

        public static StageAnalysisResult Analyze(StageData stage)
        {
            var colors = GetStageColors(stage);
            var rules = new RuleSet();
            var testedCount = 0;
            var matchingCount = 0;

            Search(stage, colors, 0, rules, ref testedCount, ref matchingCount);

            return new StageAnalysisResult(testedCount, matchingCount);
        }

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

        private static void Search(StageData stage, IReadOnlyList<BoxColor> colors, int colorIndex, RuleSet rules, ref int testedCount, ref int matchingCount)
        {
            if (colorIndex >= colors.Count)
            {
                testedCount++;
                var result = RuleSimulator.Simulate(stage, rules);

                if (Validator.Validate(result, stage.TargetBoard).IsClear)
                {
                    matchingCount++;
                }

                return;
            }

            var color = colors[colorIndex];

            foreach (var direction in DirectionCandidates)
            {
                foreach (var range in RangeCandidates)
                {
                    rules.Set(color, new Rule(direction, range, EffectType.AddNumber));
                    Search(stage, colors, colorIndex + 1, rules, ref testedCount, ref matchingCount);
                }
            }
        }
    }
}
