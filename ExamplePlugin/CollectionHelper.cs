using System;
using System.Collections.Generic;
using System.Text;

namespace RiskOfCodePlugin
{
    internal static class CollectionHelper
    {
        public static List<List<T>> SplitIntoChunks<T>(this List<T> list, int numberOfChunks)
        {
            if (numberOfChunks <= 0)
                throw new ArgumentException("Number of chunks must be greater than zero.");

            var chunks = new List<List<T>>();
            int totalItems = list.Count;

            // Calculate the base size of each chunk
            int baseChunkSize = totalItems / numberOfChunks;
            // Calculate how many chunks need to be larger by one item
            int remainder = totalItems % numberOfChunks;

            int start = 0;
            for (int i = 0; i < numberOfChunks; i++)
            {
                int chunkSize = baseChunkSize + (i < remainder ? 1 : 0);
                var chunk = list.GetRange(start, chunkSize);
                chunks.Add(chunk);
                start += chunkSize;
            }

            return chunks;
        }
    }
}
