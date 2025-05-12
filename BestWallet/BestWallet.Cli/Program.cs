using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BestWallet.Core;

class Program
{
    static void Main(string[] args)
    {

        string folder = "dow_data/2024-08-01_to_2024-12-31";
        var (stocks, prices) = DataLoader.ReadWithStocks(folder);

        double[][] priceMatrix = prices
            .Select(row => row.Select(x => (double)x).ToArray())
            .ToArray();

        double[][] dailyReturns = CalculatorArray.allDailyReturns(priceMatrix);
        double[][] dailyReturnsTransposed = CalculatorArray.transposeStockXDays(dailyReturns);

        var combinations = SimulateArray.generateWalletCombinations(
            25,
            Microsoft.FSharp.Collections.ListModule.OfSeq(stocks)
        ).Select(fsharpList => fsharpList.ToList()).ToList();

        Console.WriteLine($"🔄 Evaluating {combinations.Count} combinations × 30 weights each...");

        double globalBestSharpe = double.NegativeInfinity;
        List<string> globalBestStocks = null;
        double[] globalBestWeights = null;

        var sw = Stopwatch.StartNew();

        foreach (var combo in combinations)
        {
            var (sharpe, weights) = BestSharpeForCombination(combo.ToList(), stocks, dailyReturnsTransposed);

            if (sharpe > globalBestSharpe)
            {
                globalBestSharpe = sharpe;
                globalBestStocks = combo.ToList();
                globalBestWeights = weights;
            }
        }

        sw.Stop();

        Console.WriteLine($"\n🏆 Best Sharpe Ratio: {globalBestSharpe:F4}");
        for (int i = 0; i < globalBestStocks.Count; i++)
        {
            Console.WriteLine($"• {globalBestStocks[i]}: {globalBestWeights[i]:P2}");
        }

        Console.WriteLine($"\n⏱️ Completed in {sw.Elapsed.TotalSeconds:F2} seconds");
    }

    static (double sharpe, double[] weights) BestSharpeForCombination(
        List<string> combo,
        List<string> allTickers,
        double[][] dailyReturnsTransposed)
    {
        var indices = combo.Select(t => allTickers.IndexOf(t)).ToArray();
        var selectedReturns = dailyReturnsTransposed
            .Select(dayReturns => indices.Select(i => dayReturns[i]).ToArray())
            .ToArray();

        double bestSharpe = double.NegativeInfinity;
        double[] bestWeights = null;
        object locker = new();

        Parallel.For(0, 30, i =>
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