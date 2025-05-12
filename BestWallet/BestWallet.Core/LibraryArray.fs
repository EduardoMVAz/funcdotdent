namespace BestWallet.Core

module CalculatorArray =

    /// Computes the daily return given two consecutive close prices.
    let dailyReturn (priceTminOne: double, priceT: double) : double =
        priceT / priceTminOne - 1.0

    /// Computes daily returns from a sequence of close prices.
    let dailyReturns (prices: double[]) : double[] =
        prices
        |> Array.pairwise
        |> Array.map (fun (pPrev, pNow) -> pNow / pPrev - 1.0)

    /// Applies dailyReturns to each stock’s price history.
    let allDailyReturns (priceMatrix: double[][]) : double[][] =
        priceMatrix |> Array.map dailyReturns

    /// Transposes a 2D matrix (stock × day) into (day × stock).
    let transposeStockXDays (stockXday: double[][]) : double[][] =
        if stockXday.Length = 0 then [||]
        else
            let numDays = stockXday[0].Length
            Array.init numDays (fun i ->
                stockXday |> Array.map (fun row -> row[i])
            )

    /// Computes the weighted return of a portfolio for a single day.
    let walletDailyReturn (weights: double[]) (dailyReturns: double[]) : double =
        Array.map2 ( * ) weights dailyReturns |> Array.sum

    /// Computes the daily returns of the portfolio over time.
    let walletDailyReturns (dailyReturnsTransposed: double[][]) (weights: double[]) : double[] =
        dailyReturnsTransposed |> Array.map (walletDailyReturn weights)

    /// Computes the annualized average return from daily returns.
    let annualizedAverageReturn (walletDailyReturns: double[]) : double =
        Array.average walletDailyReturns * 252.0

    /// Computes the standard deviation (volatility) of daily returns.
    let standardDeviation (dailyReturns: double[]) : double =
        let avg = Array.average dailyReturns
        let squaredDiffs = dailyReturns |> Array.map (fun x -> (x - avg) ** 2.0)
        let variance = (Array.sum squaredDiffs) / double (dailyReturns.Length - 1)
        sqrt variance


    /// Computes the Sharpe ratio using annualized return and volatility.
    let sharpeRatio (dailyReturns: double[]) : double =
        let annualizedReturn = annualizedAverageReturn dailyReturns
        let volatility = standardDeviation dailyReturns * sqrt 252.0
        annualizedReturn / volatility
