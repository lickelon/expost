using System.Collections.Generic;
using NUnit.Framework;

namespace Expost.RuleReconstruction.Tests
{
    public sealed class RuleSimulatorTests
    {
        [Test]
        public void Simulate_DoesNotApplyNumbersToSourceCells()
        {
            var stage = CreateStage(
                new SourceBoxData(2, 2, BoxColor.Red),
                new SourceBoxData(3, 2, BoxColor.Blue));
            var rules = CreateRules(new ColorRuleData(BoxColor.Red, DirectionType.Cross, RangeType.One));

            var result = RuleSimulator.Simulate(stage, rules);

            Assert.That(result.GetCell(3, 2).HasSource, Is.True);
            Assert.That(result.GetCell(3, 2).Number, Is.EqualTo(0));
            Assert.That(result.GetCell(2, 3).Number, Is.EqualTo(1));
            Assert.That(result.GetCell(2, 1).Number, Is.EqualTo(1));
            Assert.That(result.GetCell(1, 2).Number, Is.EqualTo(1));
        }

        [Test]
        public void GetAffectedPositions_ExcludesEverySourceCell()
        {
            var stage = CreateStage(
                new SourceBoxData(2, 2, BoxColor.Red),
                new SourceBoxData(3, 2, BoxColor.Blue));
            var rule = new Rule(DirectionType.Cross, RangeType.One, EffectType.AddNumber);

            var positions = RuleSimulator.GetAffectedPositions(stage, stage.Sources[0], rule);

            Assert.That(positions.Contains(new GridPosition(3, 2)), Is.False);
            Assert.That(positions.Contains(new GridPosition(2, 3)), Is.True);
            Assert.That(positions.Contains(new GridPosition(2, 1)), Is.True);
            Assert.That(positions.Contains(new GridPosition(1, 2)), Is.True);
        }

        private static StageData CreateStage(params SourceBoxData[] sources)
        {
            return new StageData
            {
                Name = "Test Stage",
                Width = 5,
                Height = 5,
                Sources = new List<SourceBoxData>(sources),
                AnswerRules = new RuleSet()
            };
        }

        private static RuleSet CreateRules(params ColorRuleData[] rules)
        {
            var ruleSet = new RuleSet();

            foreach (var rule in rules)
            {
                ruleSet.Set(rule.Color, new Rule(rule.Direction, rule.Range, EffectType.AddNumber));
            }

            return ruleSet;
        }
    }
}
