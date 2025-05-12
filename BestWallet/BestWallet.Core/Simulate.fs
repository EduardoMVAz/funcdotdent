namespace BestWallet.Core

module Simulate = 
    let rec generateWalletCombinations (k: int) (stocks: string list) : string list list = 
        match (k, stocks) with
        | (0, _) -> [ [] ]
        | (_, []) -> []
        | (_, _) when k = stocks.Length -> [ stocks ]
        | (_, h :: t) -> 
            let withoutCurr = generateWalletCombinations k t in
            let withCurr = 
                generateWalletCombinations (k-1) t 
                |> List.map (fun combo -> h :: combo) in
            withoutCurr @ withCurr

    let generateWeights (k: int) : float list =
        let rng = System.Random() in
        let randomWeights = List.init k (fun _ -> rng.NextDouble()) in
        let total = List.sum randomWeights in
            List.map(fun w -> w / total) randomWeights
