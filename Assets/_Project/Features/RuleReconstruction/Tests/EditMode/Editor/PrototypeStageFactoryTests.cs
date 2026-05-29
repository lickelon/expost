using NUnit.Framework;

namespace Expost.RuleReconstruction.Tests
{
    public sealed class PrototypeStageFactoryTests
    {
        [Test]
        public void CreateStages_OpeningStagesIntroduceColorDirectionThenRange()
        {
            var stages = PrototypeStageFactory.CreateStages();

            Assert.That(stages[0].Name, Is.EqualTo("01 Color Basics"));
            AssertRule(stages[0], BoxColor.Red, DirectionType.Cross, RangeType.One);
            AssertRule(stages[0], BoxColor.Blue, DirectionType.Cross, RangeType.One);
            AssertRule(stages[0], BoxColor.Green, DirectionType.Cross, RangeType.One);

            Assert.That(stages[1].Name, Is.EqualTo("02 Direction Basics"));
            AssertRule(stages[1], BoxColor.Red, DirectionType.Cross, RangeType.One);
            AssertRule(stages[1], BoxColor.Blue, DirectionType.Diagonal, RangeType.One);
            AssertRule(stages[1], BoxColor.Green, DirectionType.Horizontal, RangeType.One);

            Assert.That(stages[2].Name, Is.EqualTo("03 Range Basics"));
            AssertRule(stages[2], BoxColor.Red, DirectionType.Cross, RangeType.Two);
            AssertRule(stages[2], BoxColor.Blue, DirectionType.Diagonal, RangeType.One);
            AssertRule(stages[2], BoxColor.Green, DirectionType.Vertical, RangeType.Two);
        }

        [Test]
        public void CreateStages_OpeningStagesHaveUniqueSolutions()
        {
            var stages = PrototypeStageFactory.CreateStages();

            for (var index = 0; index < 3; index++)
            {
                var stage = stages[index];
                var analysis = StageRuleAnalyzer.Analyze(stage);

                Assert.That(analysis.HasUniqueSolution, Is.True, stage.Name);
            }
        }

        private static void AssertRule(StageData stage, BoxColor color, DirectionType direction, RangeType range)
        {
            Assert.That(stage.AnswerRules.TryGet(color, out var rule), Is.True);
            Assert.That(rule.Direction, Is.EqualTo(direction));
            Assert.That(rule.Range, Is.EqualTo(range));
        }
    }
}
