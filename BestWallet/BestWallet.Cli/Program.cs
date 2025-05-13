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
        if (args.Length != 4)
        {
            Console.WriteLine("Usage: dotnet run --project BestWallet.Cli -- <startDate> <endDate> <numStocks> <numWeights>");
            Console.WriteLine("Example: dotnet run --project BestWallet.Cli -- 2024-08-01 2024-12-31 25 1000");
            return;
        }

        // Validate date formats
        if (!DateTime.TryParseExact(args[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate) ||
            !DateTime.TryParseExact(args[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
        {
            Console.WriteLine("Error: Dates must be in format YYYY-MM-DD.");
            return;
        }

        // Validate numeric args
        if (!int.TryParse(args[2], out int numStocks) || numStocks <= 0 ||
            !int.TryParse(args[3], out int numWeights) || numWeights <= 0)
        {
            Console.WriteLine("Error: numStocks and numWeights must be positive integers.");
            return;
        }


        string folder = $"dow_data/{args[0]}_to_{args[1]}";
        Console.WriteLine($"Analyzing data from {args[0]} to {args[1]}");
        var (stocks, prices) = DataLoader.ReadWithStocks(folder);

        double[][] priceMatrix = [.. prices.Select(row => row.Select(x => (double)x).ToArray())];
        double[][] dailyReturns = CalculatorArray.allDailyReturns(priceMatrix);
        double[][] dailyReturnsTransposed = CalculatorArray.transposeStockXDays(dailyReturns);

        var combinations = SimulateArray.generateWalletCombinations(
            numStocks,
            Microsoft.FSharp.Collections.ListModule.OfSeq(stocks)
        ).Select(fsharpList => fsharpList.ToList()).ToList();

        Console.WriteLine($"Evaluating {combinations.Count} combinations x {numWeights} weights each.");

        double globalBestSharpe = double.NegativeInfinity;
        List<string> globalBestStocks = [];
        double[] globalBestWeights = [];
        var sw = Stopwatch.StartNew();

        foreach (var combo in combinations)
        {
            var (sharpe, weights) = Simulate.BestSharpeForCombination(combo.ToList(), stocks, dailyReturnsTransposed, numWeights);

            if (sharpe > globalBestSharpe)
            {
                globalBestSharpe = sharpe;
                globalBestStocks = [.. combo];
                globalBestWeights = weights;
            }
        }

        sw.Stop();

        Console.WriteLine($"\nBest Sharpe Ratio: {globalBestSharpe:F4}");
        for (int i = 0; i < globalBestStocks.Count; i++)
        {
            Console.WriteLine($"• {globalBestStocks[i]}: {globalBestWeights[i]:P2}");
        }

        Console.WriteLine($"\nCompleted in {sw.Elapsed.TotalSeconds:F2} seconds");
    }
}