using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BestWallet.Core;
using Microsoft.FSharp.Collections;

class Program
{
    static void Main(string[] args)
    {
        string folder = "dow_data/2024-08-01_to_2024-12-31";
        var (stocks, prices) = DataLoader.ReadWithStocks(folder);
        var fsharpDailyReturns = Calculator.allDailyReturns(ToFSharpListOfLists(prices));
        var dailyReturns = fsharpDailyReturns
        .Select(sub => sub.ToList().Select(x => (float)x).ToList())
        .ToList();

        var combinations = Simulate.generateWalletCombinations(25, ListModule.OfSeq(stocks)).ToList();

        Console.WriteLine($"🔄 Evaluating {combinations.Count} combinations × 1000 weights each...");

        object locker = new();
        double globalBestSharpe = double.NegativeInfinity;
        List<string> globalBestStocks = null;
        List<double> globalBestWeights = null;

        var sw = Stopwatch.StartNew();

        foreach (var combo in combinations)
        {
            var (bestSharpe, bestWeights) = BestSharpeForCombination(combo.ToList(), stocks, dailyReturns);

            lock (locker)
            {
                if (bestSharpe > globalBestSharpe)
                {
                    globalBestSharpe = bestSharpe;
                    globalBestStocks = combo.ToList();
                    globalBestWeights = bestWeights;
                }
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

    static (double sharpe, List<double> weights) BestSharpeForCombination(
        List<string> combo,
        List<string> allTickers,
        List<List<float>> dailyReturns)
    {
        var indices = combo.Select(t => allTickers.IndexOf(t)).ToList();
        var comboReturns = indices.Select(i => dailyReturns[i]).ToList();
        var fsharpReturns = ToFSharpListOfLists(comboReturns);
        var transposed = Calculator.transposeStockXDays(fsharpReturns);

        object locker = new();
        double bestSharpe = double.NegativeInfinity;
        List<double> bestWeights = null;

        Parallel.For(0, 30, i =>
        {
            var weights = Simulate.generateWeights(combo.Count).Select(Convert.ToDouble).ToList();
            var walletReturns = Calculator.walletDailyReturns(transposed, ListModule.OfSeq(weights));
            var sharpe = Calculator.sharpeRatio(walletReturns);

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


    static FSharpList<FSharpList<double>> ToFSharpListOfLists<T>(List<List<T>> lists)
    {
        return ListModule.OfSeq(
            lists.Select(sub => ListModule.OfSeq(sub.Select(x => Convert.ToDouble(x))))
        );
    }
}
