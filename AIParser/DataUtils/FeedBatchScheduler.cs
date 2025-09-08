using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIParser.DataUtils
{
    public class FeedBatchScheduler
    {
        private readonly int _maxTokensPerBatch;

        public FeedBatchScheduler(int maxTokensPerBatch = 2500)
        {
            _maxTokensPerBatch = maxTokensPerBatch;
        }
        public IEnumerable<List<List<string>>> SplitIntoBatches(RawFeedData rawData)
        {
            var currentBatch = new List<List<string>>();
            int currentTokens = EstimateTokens(rawData.Headers);

            foreach (var row in rawData.Rows)
            {
                int rowTokens = EstimateTokens(row);

                if (currentTokens + rowTokens > _maxTokensPerBatch && currentBatch.Count > 0)
                {
                    yield return currentBatch;
                    currentBatch = new List<List<string>>();
                    currentTokens = EstimateTokens(rawData.Headers);
                }

                currentBatch.Add(row);
                currentTokens += rowTokens;
            }

            if (currentBatch.Count > 0)
            {
                yield return currentBatch;
            }
        }
      
        private int EstimateTokens(IEnumerable<string> values)
        {
            int length = values.Sum(v => v?.Length ?? 0);
            return length / 4;
        }
    }
}
