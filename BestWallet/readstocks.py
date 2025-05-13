import yfinance as yf
import os
import argparse
from datetime import datetime

def main(startDate, endDate):
    TICKERS = [
        "AAPL", "AMGN", "AXP", "BA", "CAT", "CRM", "CSCO", "CVX", "DIS",
        "GS", "HD", "HON", "IBM", "INTC", "JNJ", "JPM", "KO", "MCD",
        "MMM", "MRK", "MSFT", "NKE", "PG", "TRV", "UNH", "V", "VZ", "WBA",
        "WMT", "XOM"
    ]

    START = startDate or "2024-08-01"
    END = endDate or "2024-12-31"
    FOLDER = f"dow_data/{START}_to_{END}"
    os.makedirs(FOLDER, exist_ok=True)

    for ticker in TICKERS:
        print(f"Downloading {ticker}...")
        df = yf.download(ticker, start=START, end=END)
        df.to_csv(f"{FOLDER}/{ticker}.csv")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Download Dow 30 stock data using yfinance.")
    parser.add_argument("startDate", help="Start date in format YYYY-MM-DD")
    parser.add_argument("endDate", help="End date in format YYYY-MM-DD")

    args = parser.parse_args()

    try:
        datetime.strptime(args.startDate, "%Y-%m-%d")
        datetime.strptime(args.endDate, "%Y-%m-%d")
    except ValueError:
        print("Usage: python readstocks.py <start_date: YY-mm-dd FORMAT> <end_date: YY-mm-dd FORMAT >")
        exit(1)

    main(args.startDate, args.endDate)
