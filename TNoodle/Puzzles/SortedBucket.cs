using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNoodle.Puzzles
{
    public class SortedBuckets<H>
    {
        SortedSet<Bucket<H>> buckets;

        public SortedBuckets()
        {
            buckets = new SortedSet<Bucket<H>>();
        }

        public void add(H element, int value)
        {
            Bucket<H> bucket;
            Bucket<H> searchBucket = new Bucket<H>(value);
            if (!buckets.Contains(searchBucket))
            {
                // There is no bucket yet for value, so we create one.
                bucket = searchBucket;
                buckets.Add(bucket);
            }
            else
            {
                bucket = buckets.Where(b => b.CompareTo(searchBucket) >= 0).First();
            }
            bucket.push(element);
        }

        public int smallestValue()
        {
            return buckets.First().getValue();
        }

        public bool isEmpty()
        {
            return buckets.Count == 0;
        }

        public H pop()
        {
            Bucket<H> bucket = buckets.First();
            H h = bucket.pop();
            if (bucket.isEmpty())
            {
                // We just removed the last element from this bucket,
                // so we can trash the bucket now.
                buckets.Remove(bucket);
            }
            return h;
        }

        public override string ToString()
        {
            return buckets.ToString();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object o)
        {
            throw new NotImplementedException();
        }
    }
}
