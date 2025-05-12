namespace BestWallet.Core

module Calculator = 

    /// Computes the daily return given two consecutive close prices of a stock.
    /// 
    /// Formula: (price on day T / price on day T-1) - 1
    ///
    /// Parameters:
    ///  - priceTminOne: The closing price of the previous day
    ///  - priceT: The closing price of the current day
    ///
    /// Returns:
    ///  - The relative daily return as a float (e.g., 0.02 means +2%)
    let dailyReturn (priceTminOne: float, priceT: float) : float =
        priceT / priceTminOne - 1.0

    /// Computes the daily returns for a list of close prices.
    ///
    /// Parameters:
    ///  - prices: List of sequential close prices
    ///
    /// Returns:
    ///  - A list of daily returns for each pair of consecutive days
    let dailyReturns (prices: float list) : float list = 
        prices
        |> List.toArray
        |> Array.pairwise
        |> Array.map (fun (pPrev, pNow) -> pNow / pPrev - 1.0)
        |> Array.toList

    /// Applies dailyReturns to each stock’s price history in a portfolio.
    ///
    /// Parameters:
    ///  - prices: A list of price lists (one per stock)
    ///
    /// Returns:
    ///  - A list of daily return lists (one per stock)
    let allDailyReturns (prices: float list list) : float list list = 
        List.map dailyReturns prices

    /// Transposes a list-of-lists of stock × day returns into day × stock format.
    ///
    /// Parameters:
    ///  - dailyReturns: Each sublist is the return series for a single stock
    ///
    /// Returns:
    ///  - A list of lists where each sublist represents returns of all stocks on a given day
    let rec transposeStockXDays (dailyReturns: float list list) : float list list = 
        if List.forall List.isEmpty dailyReturns then
            []
        else 
            let currDay = List.map List.head dailyReturns
            let nextDay = List.map List.tail dailyReturns
            currDay :: transposeStockXDays nextDay

    /// Computes the total return of a portfolio on a single day.
    ///
    /// Parameters:
    ///  - dailyReturnWithWeights: A list of (weight, return) tuples for each stock
    ///
    /// Returns:
    ///  - The weighted return for the portfolio that day
    let walletDailyReturn (dailyReturnWithWeights: (float * float) list) : float =
        List.map (fun (x, y) -> x * y) dailyReturnWithWeights
        |> List.sum

    /// Computes the portfolio’s return for each day, given weights.
    ///
    /// Parameters:
    ///  - dailyReturnsTransposed: Daily returns for all stocks, grouped by day
    ///  - weights: Portfolio weights for each stock
    ///
    /// Returns:
    ///  - A list of daily portfolio returns
    let walletDailyReturns (dailyReturnsTransposed: float list list) (weights: float list) : float list = 
        dailyReturnsTransposed
        |> List.map (fun dailyReturns -> List.zip weights dailyReturns)
        |> List.map walletDailyReturn

    /// Computes the annualized average return for a portfolio.
    ///
    /// Parameters:
    ///  - walletDailyReturnsValue: Daily returns of the portfolio
    ///
    /// Returns:
    ///  - Annualized average return based on 252 trading days
    let annualizedAverageReturn (walletDailyReturnsValue: float list) : float =
        List.average walletDailyReturnsValue * 252.0

    /// Computes the standard deviation (volatility) of daily returns.
    ///
    /// Parameters:
    ///  - dailyReturns: A list of daily portfolio returns
    ///
    /// Returns:
    ///  - The standard deviation (volatility) of the input returns
    let standardDeviation (dailyReturns: float list) : float = 
        let averageReturn = List.average dailyReturns
        let squaredDifferenceSum = 
            List.fold (fun acc x -> acc + (x - averageReturn) ** 2.0) 0.0 dailyReturns
        let variance = squaredDifferenceSum / float(dailyReturns.Length - 1) 
        sqrt variance

    /// Computes the Sharpe Ratio for a portfolio.
    ///
    /// Parameters:
    ///  - dailyReturnsValue: A list of daily portfolio returns
    ///
    /// Returns:
    ///  - The annualized Sharpe Ratio (assumes risk-free rate is 0)
    let sharpeRatio (dailyReturnsValue: float list) : float =
        let annualizedReturn = annualizedAverageReturn dailyReturnsValue
        let volatility = standardDeviation dailyReturnsValue * sqrt 252.0
        annualizedReturn / volatility
