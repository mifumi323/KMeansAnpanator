using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KMeansAnpanator
{
    public partial class FormMain : Form
    {
        List<Member> Members;
        List<Cluster> Clusters;
        string[] Questions;
        Dictionary<int, double> Weight;

        public FormMain()
        {
            InitializeComponent();
        }

        private void dataGridView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.All;
        }

        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var lines = File.ReadAllLines(files[0], Encoding.UTF8);
                Members = new List<Member>();
                Clusters = null;
                Questions = lines[0].Split(',');
                Weight = new Dictionary<int, double>();
                for (int i = 1; i < lines.Length; i++)
                {
                    var items = lines[i].Split(',');
                    var member = new Member(i)
                    {
                        Name = items[0],
                    };
                    for (int j = 1; j < items.Length; j++)
                    {
                        double score = 0.0;
                        if (!string.IsNullOrEmpty(items[j]) && double.TryParse(items[j], out score))
                        {
                            member.Scores[j] = score;
                            if (Weight.ContainsKey(j))
                            {
                                Weight[j]++;
                            }
                            else
                            {
                                Weight[j] = 1.0;
                            }
                        }
                    }
                    Members.Add(member);
                }
                DoClustering(10, 100);
                dataGridView1.DataSource = GenerateDataTable();
            }
        }

        private DataTable GenerateDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("名前");
            dt.Columns.Add("クラスタ");
            for (int q = 1; q < Questions.Length; q++)
            {
                dt.Columns.Add(Questions[q]);
            }
            foreach (var member in Members)
            {
                var dr = dt.NewRow();
                dr["名前"] = member.Name;
                dr["クラスタ"] = member.Cluster.ID;
                foreach (var kv in member.Scores)
                {
                    dr[Questions[kv.Key]] = kv.Value;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        private void DoClustering(int clusterCount, int maxTrial)
        {
            // 初期化
            Clusters = new List<Cluster>(clusterCount);
            var random = new Random();
            for (int i = 0; i < clusterCount; i++)
            {
                var cluster = new Cluster(i);
                cluster.Members.Add(Members[random.Next(Members.Count)]);
                Clusters.Add(cluster);
            }

            // クラスタリング
            for (int t = 0; t < maxTrial; t++)
            {
                // クラスタ位置計算
                var dirty = false;
                Parallel.ForEach(Clusters, cluster =>
                {
                    dirty |= cluster.UpdateScores();
                });

                // メンバー所属計算
                Parallel.ForEach(Members, member =>
                {
                    member.Cluster = Clusters
                        .OrderBy(cluster => member.GetWeightedDistanceSquare(cluster, Weight))
                        .ThenBy(cluster => cluster.ID)
                        .First();
                });
                Parallel.ForEach(Clusters, cluster =>
                {
                    cluster.Members.Clear();
                });
                foreach (var member in Members)
                {
                    member.Cluster.Members.Add(member);
                }
                Clusters = Clusters.Where(cluster => cluster.Members.Count > 0).ToList();

                if (!dirty) continue;
            }
        }
    }
}
