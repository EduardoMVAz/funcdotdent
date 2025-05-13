# Stock Portfolio Optimization

## Summary

The goal of this project is to develop an algorithm that tries to find the best combination of stocks from the [Dow Jones Index](https://en.wikipedia.org/wiki/Dow_Jones_Industrial_Average) and create a wallet comprised of **25 out of the 30 stocks** listed by that index.

Using historical stock data in the following format:
| Date       | Price | Close | High | Low  | Open | Volume    |
|------------|-------|-------|------|------|------|-----------|
| 2024-08-01 | 218   | 224   | 216  | 224  | 62501000 |
| 2024-08-02 | 219   | 225   | 217  | 218  | 105568600 |
| 2024-08-05 | 209   | 213   | 195  | 198  | 119548600 |
| 2024-08-06 | 207   | 209   | 200  | 205  | 69660500 |
| 2024-08-07 | 209   | 213   | 206  | 206  | 63516400 |
| 2024-08-08 | 213   | 213   | 208  | 212  | 47161100 |
| 2024-08-09 | 216   | 216   | 211  | 211  | 42201600 |
| 2024-08-12 | 217   | 219   | 215  | 216  | 38028100 |

And the [Sharpe Ratio](https://en.wikipedia.org/wiki/Sharpe_ratio) measurement, the algorithm creates the best wallet possible following these steps:

- **Generate all possible combinations of 25 stocks** out of the 30 stocks listed by the Dow Jones Index;
- **Create random weight vectors** for each selected portfolio, ensuring all weights are non-negative and sum to 1;
- **Calculate the Sharpe Ratio** for each (combination, weight vector) pair;
- **Select and store** the best-performing portfolio based on the highest Sharpe Ratio.

## Optimization

Simply assigning random weights is unlikely to produce an optimal portfolio. To increase the chances of finding the best Sharpe Ratio, the algorithm performs **1,000 simulations per combination**, each using a different random weight vector.

This, however, demands significant computational resources, given that:

$$
\binom{30}{25} = \frac{30!}{25!(30 - 25)!} = \frac{30!}{25! \cdot 5!} = 142506
$$

And if 1000 weight vectors are calculated for each combination, the algorithm would calculate the sharpe ratio 142 million times.

To optimize this process, the project uses **Parallelization and Functional Programming**.

## Parallelization

The algorithm is built to use **multi-threading** when creating the 1000 weight vectors, for each combination. This means that multiple combinations can be calculated simultaneously, significantly lowering the time necessary to finish calculations. 

To understand why the parallelization was implemented for the **weight vector calculations**, the algorithm's steps must be clearly defined:

1. Creation of the Wallet Combinations 
2. Choosing a Wallet
3. Calculating the Wallet's Sharpe Ratio for a thousand random weight vectors

The step choosen for the parallelization was the Weight Vector and Sharpe Ratio calculation for these main reasons:

- Parallelizing the Wallet Combinations was unnecessary: this step was already very quick;
- Choosing each Wallet in Parallel and then calculating the 1000 weight vectors and Sharpe Ratios resulted in very complex threads, limiting the optimization done by the CPU;
- The Weight Vector and Sharpe Ratio Calculation was the most repeated operation by far.

## Programming Structure

One of the highlights of **Functional Programming** is it's synergy with **Parallelization**, due to the safety of it's **Pure Function Philosophy** and how optimized the Functional Languages are in dealing with high data volume.

For that reason, this project is built using C#, the abstract and impure layer, and F#, the functional and pure layer.

All the data loading is done through C# code, which is more manageable, and all the calculation is done through pure functions created in F#. 

The Project can be found in ```/BestWallet``` directory, whilst the C# main structure can be found in the ```/BestWallet/BestWallet.Cli``` and the F# functions can be found in the modules in ```/BestWallet/BestWallet.Core```.

### List vs Array

A very important detail in the development of this project is **the usage of Arrays** in the Functional part of the code. In the first version, the whole F# code was developed without the help of **Generative AI**. However, the algorithm was slower than expected. With the help of **Generative AI**, several small optimizations were done, but the one that had the biggest impact was switching the Functional code to use **Arrays** instead of **Lists**. Both versions can be found in the repository however, the first version being the one contained in the ```Simulate.fs``` and ```Library.fs``` files, and the array version in the ```SimulateArray.fs``` and ```LibraryArray.fs``` files.
 
## Usage and Installation

### Dependencies

- Have [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download) or higher installed;
- Have python and the yfinance lib installed.

To use the project, first clone the repository:

        git clone https://github.com/EduardoMVAz/funcdotdent

Restore the project's dependencies:

        dotnet restore
    
And then, inside the ```/BestWallet``` directory, use the python script to download the data from the Dow Jones Index:

        python3 readstocks.py <start_date: YY-mm-dd FORMAT> <end_date: YY-mm-dd FORMAT >

This will download data to the ```/BestWallet/dow_data```.

Now, simply run the project passing the arguments to your desire:

        dotnet run -- <startDate> <endDate> <numStocks> <numWeights>

## Results

Several experiments were conducted using different scenarios for comparision. The following is a table with the information of the performance examples (all done in data from 2024-08-01 -> 2024-12-31):

| Number of Weight Vector | Sharpe Obtained  | Time           |
|-------------------------|------------------|----------------|
| 10                      | 2.8656           | 14.11 seconds  |
| 50                      | 2.8168           | 50.13 seconds  |
| 100                     | 2.8698           | 91.83 seconds  |
| 250                     | 2.9410           | 227.31 seconds |
| 500                     | 2.9271           | 405.59 seconds |
| 750                     | 2.9806           | 591.99 seconds |
| 1000                    | 2.9420           | 881.07 seconds |
| 1000                    | 3.0569           | 798.17 seconds |
| 1000                    | 3.1754           | 817.64 seconds |
| 1000                    | 2.9837           | 809.81 seconds |
| 1000                    | 3.1049           | 810.98 seconds |

And the following is an experiment done with data **from the first semester of 2025** (2025-01-01 -> 2025-03-31):

| Number of Weight Vector | Sharpe Obtained  | Time           |
|-------------------------|------------------|----------------|
| 1000                    | 3.6283           | 638.95 seconds |

All these analysis can be found in the ```/BestWallet/outputs``` directory, each with a descriptive txt file. 