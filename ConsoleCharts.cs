namespace NorthwindConsole
{
    public static class ConsoleCharts
    {
        public static void PrintBarChart(Dictionary<string, int> data, int maxBarWidth = 40)
        {
            if (data.Count == 0)
                return;
            int maxValue = data.Values.Max();
            int labelWidth = data.Keys.Max(k => k.Length);

            foreach (var kvp in data)
            {
                string label = kvp.Key.PadRight(labelWidth);
                int barLength =
                    maxValue > 0 ? (int)(kvp.Value / (double)maxValue * maxBarWidth) : 0;
                Console.WriteLine($"{label} | {new string('█', barLength)} {kvp.Value}");
            }
        }

        public static void PrintHistogram(
            IEnumerable<int> values,
            int binCount = 10,
            int maxBarWidth = 40
        )
        {
            var data = values.ToList();
            if (data.Count == 0)
                return;
            int min = data.Min();
            int max = data.Max();
            if (min == max)
                max++;
            double binSize = (max - min) / (double)binCount;
            var bins = new int[binCount];
            foreach (var v in data)
            {
                int bin = (int)((v - min) / binSize);
                if (bin == binCount)
                    bin--;
                bins[bin]++;
            }
            for (int i = 0; i < binCount; i++)
            {
                int barLength =
                    bins.Max() > 0 ? (int)(bins[i] / (double)bins.Max() * maxBarWidth) : 0;
                int binStart = (int)(min + i * binSize);
                int binEnd = (int)(min + (i + 1) * binSize);
                Console.WriteLine(
                    $"{binStart, 5}-{binEnd, 5} | {new string('█', barLength)} {bins[i]}"
                );
            }
        }
    }
}
