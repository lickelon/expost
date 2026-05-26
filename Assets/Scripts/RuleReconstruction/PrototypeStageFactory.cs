using System.Collections.Generic;

namespace Expost.RuleReconstruction
{
    public static class PrototypeStageFactory
    {
        public static List<StageData> CreateStages()
        {
            return new List<StageData>
            {
                Create("01 Overlap Cross", new[] { Source(1, 1, BoxColor.Red), Source(3, 1, BoxColor.Red), Source(2, 3, BoxColor.Blue) }, Rule(DirectionType.Cross, RangeType.Two), Rule(DirectionType.Diagonal, RangeType.One)),
                Create("02 Split Evidence", new[] { Source(0, 2, BoxColor.Red), Source(4, 2, BoxColor.Red), Source(2, 0, BoxColor.Blue), Source(2, 4, BoxColor.Blue) }, Rule(DirectionType.Horizontal, RangeType.Two), Rule(DirectionType.Vertical, RangeType.Two)),
                Create("03 Corner Noise", new[] { Source(0, 0, BoxColor.Red), Source(4, 4, BoxColor.Red), Source(1, 3, BoxColor.Blue) }, Rule(DirectionType.AllAround, RangeType.One), Rule(DirectionType.Cross, RangeType.Two)),
                Create("04 Diagonal Trap", new[] { Source(2, 2, BoxColor.Red), Source(0, 4, BoxColor.Blue), Source(4, 0, BoxColor.Blue) }, Rule(DirectionType.Diagonal, RangeType.Two), Rule(DirectionType.Cross, RangeType.One)),
                Create("05 Axis Mix", new[] { Source(1, 0, BoxColor.Red), Source(3, 4, BoxColor.Red), Source(2, 2, BoxColor.Blue) }, Rule(DirectionType.Vertical, RangeType.Two), Rule(DirectionType.Horizontal, RangeType.Two)),
                Create("06 Dense Middle", new[] { Source(1, 2, BoxColor.Red), Source(3, 2, BoxColor.Red), Source(2, 1, BoxColor.Blue), Source(2, 3, BoxColor.Blue) }, Rule(DirectionType.Cross, RangeType.One), Rule(DirectionType.AllAround, RangeType.One)),
                Create("07 Range Check", new[] { Source(0, 1, BoxColor.Red), Source(4, 3, BoxColor.Red), Source(2, 2, BoxColor.Blue) }, Rule(DirectionType.Diagonal, RangeType.Two), Rule(DirectionType.Cross, RangeType.Two)),
                Create("08 Edge Symmetry", new[] { Source(0, 0, BoxColor.Red), Source(0, 4, BoxColor.Red), Source(4, 1, BoxColor.Blue), Source(4, 3, BoxColor.Blue) }, Rule(DirectionType.Vertical, RangeType.Two), Rule(DirectionType.Diagonal, RangeType.One)),
                Create("09 False Cross", new[] { Source(1, 1, BoxColor.Red), Source(3, 3, BoxColor.Red), Source(1, 3, BoxColor.Blue), Source(3, 1, BoxColor.Blue) }, Rule(DirectionType.AllAround, RangeType.One), Rule(DirectionType.Diagonal, RangeType.Two)),
                Create("10 Final Overlap", new[] { Source(2, 0, BoxColor.Red), Source(0, 2, BoxColor.Red), Source(4, 2, BoxColor.Blue), Source(2, 4, BoxColor.Blue) }, Rule(DirectionType.Cross, RangeType.Two), Rule(DirectionType.AllAround, RangeType.One))
            };
        }

        private static StageData Create(string name, IEnumerable<SourceBoxData> sources, Rule redRule, Rule blueRule)
        {
            var stage = new StageData
            {
                Name = name,
                Width = 5,
                Height = 5,
                AnswerRules = new RuleSet()
            };

            stage.Sources.AddRange(sources);
            stage.AnswerRules.Set(BoxColor.Red, redRule);
            stage.AnswerRules.Set(BoxColor.Blue, blueRule);
            stage.TargetBoard = RuleSimulator.Simulate(stage, stage.AnswerRules);

            return stage;
        }

        private static SourceBoxData Source(int x, int y, BoxColor color)
        {
            return new SourceBoxData(x, y, color);
        }

        private static Rule Rule(DirectionType direction, RangeType range)
        {
            return new Rule(direction, range, EffectType.AddNumber);
        }
    }
}
