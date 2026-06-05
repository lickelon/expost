namespace Expost.RuleReconstruction
{
    public static class MismatchAnalyzer
    {
        public static MismatchSummary GetSummary(BoardState resultBoard, BoardState targetBoard)
        {
            var needMore = 0;
            var excess = 0;

            for (var y = 0; y < targetBoard.Height; y++)
            {
                for (var x = 0; x < targetBoard.Width; x++)
                {
                    var targetCell = targetBoard.GetCell(x, y);
                    if (targetCell.HasSource)
                    {
                        continue;
                    }

                    var difference = targetCell.Number - resultBoard.GetCell(x, y).Number;
                    if (difference > 0)
                    {
                        needMore++;
                    }
                    else if (difference < 0)
                    {
                        excess++;
                    }
                }
            }

            return new MismatchSummary(needMore, excess);
        }
    }
}
