using System.Collections.Generic;

namespace Expost.RuleReconstruction
{
    public static class PrototypeStageFactory
    {
        public static List<StageData> CreateStages()
        {
            return new List<StageData>
            {
                Create("01 Three Color Cross",
                    new[] { Source(1, 1, BoxColor.Red), Source(3, 1, BoxColor.Red), Source(2, 3, BoxColor.Blue), Source(4, 4, BoxColor.Green) },
                    Answer(BoxColor.Red, DirectionType.Cross, RangeType.Two),
                    Answer(BoxColor.Blue, DirectionType.Diagonal, RangeType.One),
                    Answer(BoxColor.Green, DirectionType.Vertical, RangeType.Two)),
                Create("02 Four Color Split",
                    new[] { Source(0, 2, BoxColor.Red), Source(4, 2, BoxColor.Blue), Source(2, 0, BoxColor.Green), Source(2, 4, BoxColor.Yellow) },
                    Answer(BoxColor.Red, DirectionType.Horizontal, RangeType.Two),
                    Answer(BoxColor.Blue, DirectionType.Vertical, RangeType.One),
                    Answer(BoxColor.Green, DirectionType.Diagonal, RangeType.Two),
                    Answer(BoxColor.Yellow, DirectionType.Cross, RangeType.One)),
                Create("03 Corner Noise",
                    new[] { Source(0, 0, BoxColor.Red), Source(4, 4, BoxColor.Red), Source(1, 3, BoxColor.Blue), Source(3, 1, BoxColor.Green) },
                    Answer(BoxColor.Red, DirectionType.AllAround, RangeType.One),
                    Answer(BoxColor.Blue, DirectionType.Cross, RangeType.Two),
                    Answer(BoxColor.Green, DirectionType.Horizontal, RangeType.Two)),
                Create("04 Diagonal Trap",
                    new[] { Source(2, 2, BoxColor.Red), Source(0, 4, BoxColor.Blue), Source(4, 0, BoxColor.Green), Source(1, 0, BoxColor.Yellow) },
                    Answer(BoxColor.Red, DirectionType.Diagonal, RangeType.Two),
                    Answer(BoxColor.Blue, DirectionType.Cross, RangeType.One),
                    Answer(BoxColor.Green, DirectionType.Vertical, RangeType.Two),
                    Answer(BoxColor.Yellow, DirectionType.Horizontal, RangeType.One)),
                Create("05 Axis Mix",
                    new[] { Source(1, 0, BoxColor.Red), Source(3, 4, BoxColor.Red), Source(2, 2, BoxColor.Blue), Source(4, 1, BoxColor.Green) },
                    Answer(BoxColor.Red, DirectionType.Vertical, RangeType.Two),
                    Answer(BoxColor.Blue, DirectionType.Horizontal, RangeType.Two),
                    Answer(BoxColor.Green, DirectionType.AllAround, RangeType.One)),
                Create("06 Dense Middle",
                    new[] { Source(1, 2, BoxColor.Red), Source(3, 2, BoxColor.Blue), Source(2, 1, BoxColor.Green), Source(2, 3, BoxColor.Yellow) },
                    Answer(BoxColor.Red, DirectionType.Cross, RangeType.One),
                    Answer(BoxColor.Blue, DirectionType.AllAround, RangeType.One),
                    Answer(BoxColor.Green, DirectionType.Vertical, RangeType.Two),
                    Answer(BoxColor.Yellow, DirectionType.Diagonal, RangeType.Two)),
                Create("07 Range Check",
                    new[] { Source(0, 1, BoxColor.Red), Source(4, 3, BoxColor.Blue), Source(2, 2, BoxColor.Green) },
                    Answer(BoxColor.Red, DirectionType.Diagonal, RangeType.Two),
                    Answer(BoxColor.Blue, DirectionType.Cross, RangeType.Two),
                    Answer(BoxColor.Green, DirectionType.AllAround, RangeType.One)),
                Create("08 Edge Symmetry",
                    new[] { Source(0, 0, BoxColor.Red), Source(0, 4, BoxColor.Blue), Source(4, 1, BoxColor.Green), Source(4, 3, BoxColor.Yellow) },
                    Answer(BoxColor.Red, DirectionType.Vertical, RangeType.Two),
                    Answer(BoxColor.Blue, DirectionType.Diagonal, RangeType.One),
                    Answer(BoxColor.Green, DirectionType.Horizontal, RangeType.Two),
                    Answer(BoxColor.Yellow, DirectionType.Cross, RangeType.One)),
                Create("09 False Cross",
                    new[] { Source(1, 1, BoxColor.Red), Source(3, 3, BoxColor.Blue), Source(1, 3, BoxColor.Green), Source(3, 1, BoxColor.Yellow) },
                    Answer(BoxColor.Red, DirectionType.AllAround, RangeType.One),
                    Answer(BoxColor.Blue, DirectionType.Diagonal, RangeType.Two),
                    Answer(BoxColor.Green, DirectionType.Cross, RangeType.One),
                    Answer(BoxColor.Yellow, DirectionType.Horizontal, RangeType.Two)),
                Create("10 Final Overlap",
                    new[] { Source(2, 0, BoxColor.Red), Source(0, 2, BoxColor.Blue), Source(4, 2, BoxColor.Green), Source(2, 4, BoxColor.Yellow) },
                    Answer(BoxColor.Red, DirectionType.Cross, RangeType.Two),
                    Answer(BoxColor.Blue, DirectionType.AllAround, RangeType.One),
                    Answer(BoxColor.Green, DirectionType.Diagonal, RangeType.Two),
                    Answer(BoxColor.Yellow, DirectionType.Vertical, RangeType.Two))
            };
        }

        private static StageData Create(string name, IEnumerable<SourceBoxData> sources, params ColorRuleAnswer[] answers)
        {
            var stage = new StageData
            {
                Name = name,
                Width = 5,
                Height = 5,
                AnswerRules = new RuleSet()
            };

            stage.Sources.AddRange(sources);

            foreach (var answer in answers)
            {
                stage.AnswerRules.Set(answer.Color, answer.Rule);
            }

            stage.TargetBoard = RuleSimulator.Simulate(stage, stage.AnswerRules);

            return stage;
        }

        private static SourceBoxData Source(int x, int y, BoxColor color)
        {
            return new SourceBoxData(x, y, color);
        }

        private static ColorRuleAnswer Answer(BoxColor color, DirectionType direction, RangeType range)
        {
            return new ColorRuleAnswer(color, new Rule(direction, range, EffectType.AddNumber));
        }

        private readonly struct ColorRuleAnswer
        {
            public readonly BoxColor Color;
            public readonly Rule Rule;

            public ColorRuleAnswer(BoxColor color, Rule rule)
            {
                Color = color;
                Rule = rule;
            }
        }
    }
}
