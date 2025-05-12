namespace BestWallet.Core

module SimulateArray =

    /// Recursively generates all combinations of k elements from a list.
    let rec generateWalletCombinations (k: int) (stocks: string list) : string list list =
        match (k, stocks) with
        | (0, _) -> [ [] ]
        | (_, []) -> []
        | (_, _) when k = stocks.Length -> [ stocks ]
        | (_, h :: t) ->
            let without = generateWalletCombinations k t
            let withCurr = generateWalletCombinations (k - 1) t |> List.map (fun c -> h :: c)
            without @ withCurr

    /// Generates a normalized random weight vector of length k.
    let generateWeights (k: int) : double[] =
        let rng = System.Random()
        let raw = Array.init k (fun _ -> rng.NextDouble())
        let total = Array.sum raw
        raw |> Array.map (fun x -> x / total)
