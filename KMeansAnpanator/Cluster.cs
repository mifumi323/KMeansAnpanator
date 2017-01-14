using System.Collections.Generic;
using System.Linq;

namespace KMeansAnpanator
{
    public class Cluster
    {
        public int ID { get; private set; }
        public ICollection<Member> Members { get; private set; }
        public IDictionary<int, double> Scores { get; private set; }

        public Cluster(int id)
        {
            ID = id;
            Members = new List<Member>();
            Scores = new Dictionary<int, double>();
        }

        public bool UpdateScores()
        {
            var dirty = false;
            var scores = new Dictionary<int, double>();
            var counts = new Dictionary<int, int>();
            foreach (var member in Members)
            {
                foreach (var skv in member.Scores)
                {
                    if (scores.ContainsKey(skv.Key))
                    {
                        scores[skv.Key] += skv.Value;
                        counts[skv.Key]++;
                    }
                    else
                    {
                        scores[skv.Key] = skv.Value;
                        counts[skv.Key] = 1;
                    }
                }
            }
            foreach (var ckv in counts)
            {
                scores[ckv.Key] /= ckv.Value;
            }
            dirty = scores.Any(skv => !Scores.ContainsKey(skv.Key) || skv.Value != Scores[skv.Key])
                || Scores.Any(skv => !scores.ContainsKey(skv.Key));
            Scores = scores;
            return dirty;
        }
    }
}
