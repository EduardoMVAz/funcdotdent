using BestWallet.Core;

public static class Simulate
{
    public static (double sharpe, double[] weights) BestSharpeForCombination(
        List<string> combo,
        List<string> allTickers,
        double[][] dailyReturnsTransposed,
        int numWeights)
    {
        var indices = combo.Select(t => allTickers.IndexOf(t)).ToArray();
        var selectedReturns = dailyReturnsTransposed
            .Select(dayReturns => indices.Select(i => dayReturns[i]).ToArray())
            .ToArray();

        double bestSharpe = double.NegativeInfinity;
        double[] bestWeights = [];
        object locker = new();

        Parallel.For(0, numWeights, i =>
        {
            var weights = SimulateArray.generateWeights(combo.Count);
            var walletReturns = CalculatorArray.walletDailyReturns(selectedReturns, weights);
            var sharpe = CalculatorArray.sharpeRatio(walletReturns);

            lock (locker)
            {
                if (sharpe > bestSharpe)
                {
                    bestSharpe = sharpe;
                    bestWeights = weights;
                }
            }
        });

        return (bestSharpe, bestWeights);
    }
}