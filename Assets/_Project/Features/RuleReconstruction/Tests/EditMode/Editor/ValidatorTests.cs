using NUnit.Framework;

namespace Expost.RuleReconstruction.Tests
{
    public sealed class ValidatorTests
    {
        [Test]
        public void Validate_ReturnsClear_WhenBoardsMatch()
        {
            var target = new BoardState(2, 2);
            target.AddNumber(0, 0, 1);
            var result = new BoardState(2, 2);
            result.AddNumber(0, 0, 1);

            var validation = Validator.Validate(result, target);

            Assert.That(validation.IsClear, Is.True);
            Assert.That(validation.WrongCellCount, Is.EqualTo(0));
        }

        [Test]
        public void Validate_CountsWrongNumberCells()
        {
            var target = new BoardState(2, 2);
            target.AddNumber(0, 0, 1);
            target.AddNumber(1, 1, 1);
            var result = new BoardState(2, 2);
            result.AddNumber(0, 0, 1);

            var validation = Validator.Validate(result, target);

            Assert.That(validation.IsClear, Is.False);
            Assert.That(validation.WrongCellCount, Is.EqualTo(1));
        }

        [Test]
        public void Validate_IgnoresTargetSourceCells()
        {
            var target = new BoardState(2, 2);
            target.PlaceSource(new SourceBoxData(0, 0, BoxColor.Red));
            var result = new BoardState(2, 2);
            result.PlaceSource(new SourceBoxData(0, 0, BoxColor.Blue));
            result.AddNumber(0, 0, 1);

            var validation = Validator.Validate(result, target);

            Assert.That(validation.IsClear, Is.True);
            Assert.That(validation.WrongCellCount, Is.EqualTo(0));
        }

        [Test]
        public void IsCellCorrect_ComparesOnlyNumberForNumberCells()
        {
            var target = new CellState
            {
                Number = 2
            };
            var result = new CellState
            {
                HasSource = true,
                SourceColor = BoxColor.Green,
                Number = 1
            };

            Assert.That(Validator.IsCellCorrect(result, target), Is.False);
        }
    }
}
