namespace Shared;

public static class Extensions
{
    public static List<T[]> SplitIntoBatches<T>(this T[] array, int batchSize)
    {
        if (batchSize <= 0) throw new ArgumentException("Размер части должен быть больше нуля.", nameof(batchSize));

        var result = new List<T[]>();

        for (var i = 0; i < array.Length; i += batchSize)
        {
            var currentBatchSize = Math.Min(batchSize, array.Length - i);
            var batch = new T[currentBatchSize];
            Array.Copy(array, i, batch, 0, currentBatchSize);
            result.Add(batch);
        }

        return result;
    }
}