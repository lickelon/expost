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
                    if (!result.GetCell(x, y).Matches(target.GetCell(x, y)))
                    {
                        wrongCount++;
                    }
                }
            }

            return new ValidationResult(wrongCount == 0, wrongCount);
        }
    }
}
