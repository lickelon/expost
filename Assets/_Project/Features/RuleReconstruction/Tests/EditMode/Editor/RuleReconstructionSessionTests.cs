using System.Collections.Generic;
using NUnit.Framework;

namespace Expost.RuleReconstruction.Tests
{
    public sealed class RuleReconstructionSessionTests
    {
        private static readonly BoxColor[] TestColors =
        {
            BoxColor.Red,
            BoxColor.Blue,
            BoxColor.Green,
            BoxColor.Yellow
        };

        [Test]
        public void ApplyNextSource_AdvancesSimulationAndUpdatesValidation()
        {
            var stage = CreateSingleSourceStage("Single Source");
            var session = new RuleReconstructionSession(new List<StageData> { stage }, TestColors);

            Assert.That(session.AppliedSourceCount, Is.EqualTo(0));
            Assert.That(session.IsComplete, Is.False);
            Assert.That(session.ValidationResult.IsClear, Is.False);

            session.ApplyNextSource();

            Assert.That(session.AppliedSourceCount, Is.EqualTo(1));
            Assert.That(session.IsComplete, Is.True);
            Assert.That(session.ValidationResult.IsClear, Is.True);
        }

        [Test]
        public void CycleDirection_ChangesRuleAndResetsSimulation()
        {
            var stage = CreateSingleSourceStage("Single Source");
            var session = new RuleReconstructionSession(new List<StageData> { stage }, TestColors);
            session.ApplyNextSource();

            session.CycleDirection(BoxColor.Red);

            Assert.That(session.GetDirection(BoxColor.Red), Is.EqualTo(DirectionType.Diagonal));
            Assert.That(session.AppliedSourceCount, Is.EqualTo(0));
            Assert.That(session.IsComplete, Is.False);
        }

        [Test]
        public void MoveStage_WrapsAndResetsSimulation()
        {
            var first = CreateSingleSourceStage("01 First");
            var second = CreateSingleSourceStage("02 Second");
            var session = new RuleReconstructionSession(new List<StageData> { first, second }, TestColors);
            session.ApplyNextSource();

            session.MoveStage(1);

            Assert.That(session.CurrentStage.Name, Is.EqualTo("02 Second"));
            Assert.That(session.AppliedSourceCount, Is.EqualTo(0));

            session.MoveStage(1);

            Assert.That(session.CurrentStage.Name, Is.EqualTo("01 First"));
        }

        private static StageData CreateSingleSourceStage(string name)
        {
            var stage = new StageData
            {
                Name = name,
                Width = 3,
                Height = 3,
                Sources = new List<SourceBoxData>
                {
                    new(1, 1, BoxColor.Red)
                },
                AnswerRules = CreateRules(new ColorRuleData(BoxColor.Red, DirectionType.Cross, RangeType.One))
            };

            stage.TargetBoard = RuleSimulator.Simulate(stage, stage.AnswerRules);
            return stage;
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
