using System.Collections.Generic;
using System.Linq;

namespace KMeansAnpanator
{
    public class Member
    {
        public int ID { get; private set; }
        public string Name { get; set; }
        public Cluster Cluster { get; set; }
        public IDictionary<int, double> Scores { get; private set; }

        public Member(int id)
        {
            ID = id;
            Scores = new Dictionary<int, double>();
        }

        public double GetDistanceSquare(Cluster cluster)
        {
            return Scores
                .Where(kv => cluster.Scores.ContainsKey(kv.Key))
                .Select(kv => cluster.Scores[kv.Key] - kv.Value)
                .Sum(d => d * d);
        }

        public double GetWeightedDistanceSquare(Cluster cluster, IDictionary<int, double> weight)
        {
            return Scores
                .Where(kv => cluster.Scores.ContainsKey(kv.Key) && weight.ContainsKey(kv.Key))
                .Select(kv => (cluster.Scores[kv.Key] - kv.Value) * weight[kv.Key])
                .Sum(d => d * d);
        }
    }
}
