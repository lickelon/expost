namespace Expost.RuleReconstruction
{
    public static class Validator
    {
        public static ValidationResult Validate(BoardState result, BoardState target)
        {
            var wrongCount = 0;

            for (var y = 0; y < target.Height; y++)
            {
                for (var x = 0; x < target.Width; x++)
                {
                    if (!IsCellCorrect(result.GetCell(x, y), target.GetCell(x, y)))
                    {
                        wrongCount++;
                    }
                }
            }

            return new ValidationResult(wrongCount == 0, wrongCount);
        }

        public static bool IsCellCorrect(CellState result, CellState target)
        {
            return target.HasSource || result.Number == target.Number;
        }
    }
}
