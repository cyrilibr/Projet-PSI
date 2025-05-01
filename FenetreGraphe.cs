using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Projet_PSI
{
    /// <summary>
    /// Formulaire de visualisation du graphe de stations avec positions géographiques,
    /// affichage des arêtes orientées, pondérées, et coloration.
    /// </summary>
    public class FenetreGraphe<T> : Form
    {
        private Graphe<T> graphe;
        private Dictionary<int, PointF> positions;
        private const int rayon = 10;
        private const int margeX = 50;
        private const int margeY = 50;

        // Boutons
        private Button btnReset;
        private Button btnListeAdj;
        private Button btnMatrice;
        private Button btnBFS;
        private Button btnDFS;
        private Button btnConnexe;
        private Button btnCycle;
        private Button btnDijkstra;
        private Button btnBellman;
        private Button btnFloyd;
        private Button btnComparer;
        private Button btnColoration;

        // Parcours et coloration
        private HashSet<int> bfsVisites = new HashSet<int>();
        private HashSet<int> dfsVisites = new HashSet<int>();
        private HashSet<int> dijkstraVisites = new HashSet<int>();
        private HashSet<int> bellmanVisites = new HashSet<int>();
        private HashSet<int> floydVisites = new HashSet<int>();
        private Dictionary<int, Brush> coloration = new Dictionary<int, Brush>();

        public FenetreGraphe(Graphe<T> graphe)
        {
            this.graphe = graphe;
            Text = "Visualisation Géographique du Graphe";
            WindowState = FormWindowState.Maximized;

            InitializeComponents();
            CalculatePositions();
            Paint += DessinerGraphe;
        }

        private void InitializeComponents()
        {
            int x = margeX, y = margeY;
            int espace = 10;

            Button Create(string text, EventHandler handler)
            {
                var btn = new Button { Text = text, AutoSize = true, Location = new Point(x, y) };
                btn.Click += handler;
                Controls.Add(btn);
                x += btn.Width + espace;
                return btn;
            }

            btnReset = Create("Réinitialiser", (s, e) => { ClearAll(); Invalidate(); });
            btnListeAdj = Create("Liste Adjacence", BoutonListeAdj_Click);
            btnMatrice = Create("Matrice Adjacence", BoutonMatriceAdj_Click);
            btnBFS = Create("BFS (1->tous)", BoutonBFS_Click);
            btnDFS = Create("DFS (1->tous)", BoutonDFS_Click);
            btnConnexe = Create("Est connexe ?", BoutonConnexe_Click);
            btnCycle = Create("Contient cycle ?", BoutonCycle_Click);
            btnDijkstra = Create("Dijkstra (1->3)", BoutonDijkstra_Click);
            btnBellman = Create("Bellman (11->30)", BoutonBellman_Click);
            btnFloyd = Create("Floyd (6->22)", BoutonFloyd_Click);
            btnComparer = Create("Comparer Algos", BoutonComparer_Click);
            btnColoration = Create("Coloration Graphe", BoutonColoration_Click);
        }

        private void CalculatePositions()
        {
            var stations = graphe.Noeuds.Values
                .Select(n => n.Data as Station)
                .Where(s => s != null)
                .ToList();

            if (!stations.Any()) return;

            double minLat = stations.Min(s => s.Latitude);
            double maxLat = stations.Max(s => s.Latitude);
            double minLon = stations.Min(s => s.Longitude);
            double maxLon = stations.Max(s => s.Longitude);

            float w = ClientSize.Width - 2 * margeX;
            float h = ClientSize.Height - 2 * margeY;
            positions = new Dictionary<int, PointF>();

            foreach (var kv in graphe.Noeuds)
            {
                if (kv.Value.Data is Station s)
                {
                    float px = (float)((s.Longitude - minLon) / (maxLon - minLon) * w) + margeX;
                    float py = (float)((maxLat - s.Latitude) / (maxLat - minLat) * h) + margeY;
                    positions[kv.Key] = new PointF(px, py);
                }
            }
        }

        private void DessinerGraphe(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Arêtes
            foreach (var lien in graphe.Liens)
            {
                if (!positions.ContainsKey(lien.Noeud1.Id) || !positions.ContainsKey(lien.Noeud2.Id)) continue;
                var p1 = positions[lien.Noeud1.Id];
                var p2 = positions[lien.Noeud2.Id];

                // Flèche si non bidirectionnel
                DrawArrow(g, p1, p2, lien.Bidirectionnel ? Pens.Gray : Pens.Black);

                // Poids au milieu
                var mid = new PointF((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
                g.DrawString(lien.Poids.ToString(), Font, Brushes.DarkBlue, mid);
            }

            // Nœuds
            foreach (var kv in graphe.Noeuds)
            {
                if (!positions.TryGetValue(kv.Key, out var pos)) continue;

                Brush fill = Brushes.White;
                if (coloration.TryGetValue(kv.Key, out var col)) fill = col;
                else if (bfsVisites.Contains(kv.Key)) fill = Brushes.Yellow;
                else if (dfsVisites.Contains(kv.Key)) fill = Brushes.Red;
                else if (dijkstraVisites.Contains(kv.Key)) fill = Brushes.Green;
                else if (bellmanVisites.Contains(kv.Key)) fill = Brushes.Violet;
                else if (floydVisites.Contains(kv.Key)) fill = Brushes.Pink;

                g.FillEllipse(fill, pos.X - rayon, pos.Y - rayon, 2 * rayon, 2 * rayon);
                g.DrawEllipse(Pens.Blue, pos.X - rayon, pos.Y - rayon, 2 * rayon, 2 * rayon);

                string lbl = (kv.Value.Data as Station)?.Libelle ?? kv.Key.ToString();
                var sz = g.MeasureString(lbl, Font);
                g.DrawString(lbl, Font, Brushes.Black, pos.X - sz.Width / 2, pos.Y - sz.Height / 2);
            }
        }

        private void DrawArrow(Graphics g, PointF a, PointF b, Pen pen)
        {
            const float arrowSize = 8f;
            float dx = b.X - a.X, dy = b.Y - a.Y;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            if (len < 1e-3) return;
            float ux = dx / len, uy = dy / len;
            var src = new PointF(a.X + ux * rayon, a.Y + uy * rayon);
            var dst = new PointF(b.X - ux * rayon, b.Y - uy * rayon);

            g.DrawLine(pen, src, dst);
            var perp = new PointF(-uy, ux);
            var p1 = new PointF(dst.X - ux * arrowSize + perp.X * arrowSize / 2,
                                 dst.Y - uy * arrowSize + perp.Y * arrowSize / 2);
            var p2 = new PointF(dst.X - ux * arrowSize - perp.X * arrowSize / 2,
                                 dst.Y - uy * arrowSize - perp.Y * arrowSize / 2);
            g.FillPolygon(pen.Brush, new[] { dst, p1, p2 });
        }

        private void ClearAll()
        {
            bfsVisites.Clear();
            dfsVisites.Clear();
            dijkstraVisites.Clear();
            bellmanVisites.Clear();
            floydVisites.Clear();
            coloration.Clear();
        }

        private void BoutonListeAdj_Click(object s, EventArgs e)
        {
            string txt = "Liste d'adjacence:\n" + graphe.AfficherListeAdjacence();
            AfficherEnPleinEcran("Liste d'adjacence", txt);
        }
        private void BoutonMatriceAdj_Click(object s, EventArgs e)
        {
            string txt = "Matrice d'adjacence:\n" + graphe.AfficherMatriceAdjacence();
            AfficherEnPleinEcran("Matrice d'adjacence", txt);
        }
        private void BoutonConnexe_Click(object s, EventArgs e)
        {
            bool res = graphe.EstConnexe();
            AfficherEnPleinEcran("Connexe", $"Le graphe est connexe ? {res}");
        }
        private void BoutonCycle_Click(object s, EventArgs e)
        {
            bool res = graphe.ContientCycle();
            AfficherEnPleinEcran("Cycle", $"Le graphe contient un cycle ? {res}");
        }
        private void BoutonBFS_Click(object s, EventArgs e)
        {
            ClearAll();
            if (graphe.Noeuds.ContainsKey(1))
            {
                var queue = new Queue<int>(); queue.Enqueue(1); bfsVisites.Add(1);
                while (queue.Count > 0)
                {
                    int u = queue.Dequeue();
                    foreach (var n in graphe.Noeuds[u].Voisins)
                        if (bfsVisites.Add(n.Id)) queue.Enqueue(n.Id);
                }
            }
            Invalidate();
        }
        private void BoutonDFS_Click(object s, EventArgs e)
        {
            ClearAll();
            if (graphe.Noeuds.ContainsKey(1))
            {
                var stack = new Stack<int>(); stack.Push(1);
                while (stack.Count > 0)
                {
                    int u = stack.Pop();
                    if (dfsVisites.Add(u))
                        foreach (var n in graphe.Noeuds[u].Voisins)
                            if (!dfsVisites.Contains(n.Id)) stack.Push(n.Id);
                }
            }
            Invalidate();
        }
        private void BoutonDijkstra_Click(object s, EventArgs e)
        {
            ClearAll();
            if (graphe.Noeuds.ContainsKey(1) && graphe.Noeuds.ContainsKey(3))
                foreach (var id in graphe.CheminDijkstra(1, 3)) dijkstraVisites.Add(id);
            Invalidate();
        }
        private void BoutonBellman_Click(object s, EventArgs e)
        {
            ClearAll();
            if (graphe.Noeuds.ContainsKey(11) && graphe.Noeuds.ContainsKey(30))
                foreach (var id in graphe.CheminBellmanFord(11, 30)) bellmanVisites.Add(id);
            Invalidate();
        }
        private void BoutonFloyd_Click(object s, EventArgs e)
        {
            ClearAll();
            if (graphe.Noeuds.ContainsKey(6) && graphe.Noeuds.ContainsKey(22))
                foreach (var id in graphe.CheminFloydWarshall(6, 22)) floydVisites.Add(id);
            Invalidate();
        }
        private void BoutonComparer_Click(object s, EventArgs e)
        {
            ClearAll();
            var results = new List<(string nom, long t, List<int> chemin)>();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            sw.Restart();
            var ch1 = graphe.CheminDijkstra(11, 21);
            sw.Stop();
            results.Add(("Dijkstra", sw.ElapsedMilliseconds, ch1));

            sw.Restart();
            var ch2 = graphe.CheminBellmanFord(11, 21);
            sw.Stop();
            results.Add(("Bellman-Ford", sw.ElapsedMilliseconds, ch2));

            sw.Restart();
            var ch3 = graphe.CheminFloydWarshall(11, 21);
            sw.Stop();
            results.Add(("Floyd-Warshall", sw.ElapsedMilliseconds, ch3));

            var fastest = results.OrderBy(r => r.t).First();
            foreach (var id in fastest.chemin)
            {
                if (fastest.nom == "Dijkstra") dijkstraVisites.Add(id);
                if (fastest.nom == "Bellman-Ford") bellmanVisites.Add(id);
                if (fastest.nom == "Floyd-Warshall") floydVisites.Add(id);
            }

            string msg = string.Join("\n", results.Select(r => $"{r.nom} : {r.t} ms (Chemin: {string.Join("-", r.chemin)})"))
                         + $"\nLe plus rapide est : {fastest.nom}";
            AfficherEnPleinEcran("Comparaison algorithmes", msg);
            Invalidate();
        }
        private void BoutonColoration_Click(object s, EventArgs e)
        {
            ClearAll();
            // Brush palette
            Brush[] palette = { Brushes.LightBlue, Brushes.LightGreen, Brushes.LightSalmon, Brushes.LightYellow, Brushes.LightPink, Brushes.LightGray };
            foreach (var node in graphe.Noeuds.Keys)
            {
                var used = new HashSet<Brush>();
                foreach (var v in graphe.Noeuds[node].Voisins)
                    if (coloration.TryGetValue(v.Id, out var b)) used.Add(b);
                coloration[node] = palette.First(c => !used.Contains(c));
            }
            Invalidate();
        }

        private void AfficherEnPleinEcran(string titre, string contenu)
        {
            var fen = new Form { Text = titre, WindowState = FormWindowState.Maximized };
            var tb = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Dock = DockStyle.Fill, Text = contenu };
            fen.Controls.Add(tb); fen.ShowDialog();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CalculatePositions();
            Invalidate();
        }
    }
}
