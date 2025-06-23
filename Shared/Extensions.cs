namespace Shared;

public static class Extensions
{
    public static List<List<T>> SplitIntoBatches<T>(this ICollection<T> collection, int batchSize)
    {
        if (batchSize <= 0) throw new ArgumentException("Размер части должен быть больше нуля.", nameof(batchSize));

        var result = new List<List<T>>();
        var list = collection as IList<T> ?? collection.ToList();
        var count = list.Count;

        for (var i = 0; i < count; i += batchSize)
        {
            var currentBatchSize = Math.Min(batchSize, count - i);
            var batch = new List<T>(currentBatchSize);
            for (var j = 0; j < currentBatchSize; j++)
            {
                batch.Add(list[i + j]);
            }
            result.Add(batch);
        }

        return result;
    }
}