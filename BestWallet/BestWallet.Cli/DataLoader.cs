using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

public static class DataLoader
{
    // Read the second column ("Close") from custom CSV format
    public static List<float> ReadAdjustedCloses(string path)
    {
        return File.ReadLines(path)
            .Skip(3)
            .Select(line => line.Split(',')[1]) // Close column
            .Select(s => float.Parse(s, CultureInfo.InvariantCulture))
            .ToList();
    }


    // Read all files in the folder and get prices in a consistent order
    public static (List<string> Stocks, List<List<float>> PriceMatrix) ReadWithStocks(string folderPath)
    {
        var files = Directory.GetFiles(folderPath, "*.csv").OrderBy(f => f).ToList();
        var stocks = files.Select(f => Path.GetFileNameWithoutExtension(f)!).ToList();
        var prices = files.Select(ReadAdjustedCloses).ToList();

        return (stocks, prices);
    }
}
