// Projet_PSI/FenetreGraphe.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Projet_PSI
{
    public class FenetreGraphe<T> : Form
    {
        private Graphe<T> graphe;
        private Dictionary<int, PointF> positions;
        private const int rayon = 10;
        private const int margeX = 50;
        private const int margeY = 50;

        private NumericUpDown nudDepart;
        private NumericUpDown nudArrivee;

        private int DepartId => (int)nudDepart?.Value;
        private int ArriveeId => (int)nudArrivee?.Value;

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

        private HashSet<int> bfsVisites = new HashSet<int>();
        private HashSet<int> dfsVisites = new HashSet<int>();
        private HashSet<int> dijkstraVisites = new HashSet<int>();
        private HashSet<int> bellmanVisites = new HashSet<int>();
        private HashSet<int> floydVisites = new HashSet<int>();
        private Dictionary<int, Brush> coloration = new Dictionary<int, Brush>();

        /// <summary>
        /// Constructeur standard.
        /// </summary>
        public FenetreGraphe(Graphe<T> graphe)
        {
            this.graphe = graphe;
            Text = "Visualisation Géographique du Graphe";
            WindowState = FormWindowState.Maximized;

            InitializeComponents();
            CalculatePositions();
            Paint += DessinerGraphe;
        }

        /// <summary>
        /// Constructeur surcharge : pré-sélectionne le départ et l'arrivée, et surligne le chemin Dijkstra.
        /// </summary>
        public FenetreGraphe(Graphe<T> graphe, int departId, int arriveeId)
            : this(graphe)
        {
            nudDepart.Value = departId;
            nudArrivee.Value = arriveeId;

            foreach (var id in graphe.CheminDijkstra(departId, arriveeId))
                dijkstraVisites.Add(id);

            Invalidate();
        }

        private void InitializeComponents()
        {
            int x = margeX;
            int y = margeY;
            int espace = 10;

            var lblDepart = new Label { Text = "Départ ID :", Location = new Point(x, y), AutoSize = true };
            Controls.Add(lblDepart);
            x += lblDepart.Width + espace;

            nudDepart = new NumericUpDown
            {
                Location = new Point(x, y),
                Minimum = 0,
                Maximum = graphe.Noeuds.Count > 0 ? graphe.Noeuds.Keys.Max() : 1000,
                Width = 60
            };
            Controls.Add(nudDepart);
            x += (int)nudDepart.Width + espace;

            var lblArrivee = new Label { Text = "Arrivée ID :", Location = new Point(x, y), AutoSize = true };
            Controls.Add(lblArrivee);
            x += lblArrivee.Width + espace;

            nudArrivee = new NumericUpDown
            {
                Location = new Point(x, y),
                Minimum = 0,
                Maximum = graphe.Noeuds.Count > 0 ? graphe.Noeuds.Keys.Max() : 1000,
                Width = 60
            };
            Controls.Add(nudArrivee);
            x += (int)nudArrivee.Width + espace;

            Button Create(string text, EventHandler handler)
            {
                var btn = new Button { Text = text, AutoSize = true, Location = new Point(x, y - 50) };
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
            btnDijkstra = Create("Dijkstra", BoutonDijkstra_Click);
            btnBellman = Create("Bellman-Ford", BoutonBellman_Click);
            btnFloyd = Create("Floyd-Warshall", BoutonFloyd_Click);
            btnComparer = Create("Comparer Algos", BoutonComparer_Click);
            btnColoration = Create("Coloration", BoutonColoration_Click);
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
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Arêtes
            foreach (var lien in graphe.Liens)
            {
                if (!positions.ContainsKey(lien.Noeud1.Id) || !positions.ContainsKey(lien.Noeud2.Id)) continue;
                var p1 = positions[lien.Noeud1.Id];
                var p2 = positions[lien.Noeud2.Id];
                DrawArrow(g, p1, p2, lien.Bidirectionnel);
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

        private void DrawArrow(Graphics g, PointF a, PointF b, bool bidirectionnel)
        {
            const float arrowSize = 8f;
            float dx = b.X - a.X, dy = b.Y - a.Y;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            if (len < 1e-3) return;
            float ux = dx / len, uy = dy / len;
            var src = new PointF(a.X + ux * rayon, a.Y + uy * rayon);
            var dst = new PointF(b.X - ux * rayon, b.Y - uy * rayon);

            g.DrawLine(Pens.Black, src, dst);
            if (bidirectionnel)
            {
                var perp = new PointF(-uy, ux);
                var p1 = new PointF(dst.X - ux * arrowSize + perp.X * arrowSize / 2,
                                     dst.Y - uy * arrowSize + perp.Y * arrowSize / 2);
                var p2 = new PointF(dst.X - ux * arrowSize - perp.X * arrowSize / 2,
                                     dst.Y - uy * arrowSize - perp.Y * arrowSize / 2);
                g.FillPolygon(Pens.Black.Brush, new[] { dst, p1, p2 });
            }
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
            => AfficherEnPleinEcran("Liste d'adjacence", graphe.AfficherListeAdjacence());

        private void BoutonMatriceAdj_Click(object s, EventArgs e)
            => AfficherEnPleinEcran("Matrice d'adjacence", graphe.AfficherMatriceAdjacence());

        private void BoutonConnexe_Click(object s, EventArgs e)
            => AfficherEnPleinEcran("Connexe", $"Le graphe est connexe ? {graphe.EstConnexe()}");

        private void BoutonCycle_Click(object s, EventArgs e)
            => AfficherEnPleinEcran("Cycle", $"Le graphe contient un cycle ? {graphe.ContientCycle()}");

        private void BoutonBFS_Click(object s, EventArgs e)
        {
            ClearAll();
            if (graphe.Noeuds.ContainsKey(DepartId))
            {
                var q = new Queue<int>();
                q.Enqueue(DepartId);
                bfsVisites.Add(DepartId);
                while (q.Count > 0)
                {
                    int u = q.Dequeue();
                    foreach (var n in graphe.Noeuds[u].Voisins)
                        if (bfsVisites.Add(n.Id)) q.Enqueue(n.Id);
                }
            }
            Invalidate();
        }

        private void BoutonDFS_Click(object s, EventArgs e)
        {
            ClearAll();
            if (graphe.Noeuds.ContainsKey(1))
            {
                var stack = new Stack<int>();
                stack.Push(1);
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
            if (graphe.Noeuds.ContainsKey(DepartId) && graphe.Noeuds.ContainsKey(ArriveeId))
                foreach (var id in graphe.CheminDijkstra(DepartId, ArriveeId))
                    dijkstraVisites.Add(id);
            Invalidate();
        }

        private void BoutonBellman_Click(object s, EventArgs e)
        {
            ClearAll();
            if (graphe.Noeuds.ContainsKey(DepartId) && graphe.Noeuds.ContainsKey(ArriveeId))
                foreach (var id in graphe.CheminBellmanFord(DepartId, ArriveeId))
                    bellmanVisites.Add(id);
            Invalidate();
        }

        private void BoutonFloyd_Click(object s, EventArgs e)
        {
            ClearAll();
            if (graphe.Noeuds.ContainsKey(DepartId) && graphe.Noeuds.ContainsKey(ArriveeId))
                foreach (var id in graphe.CheminFloydWarshall(DepartId, ArriveeId))
                    floydVisites.Add(id);
            Invalidate();
        }

        private void BoutonComparer_Click(object s, EventArgs e)
        {
            ClearAll();
            if (!graphe.Noeuds.ContainsKey(DepartId) || !graphe.Noeuds.ContainsKey(ArriveeId))
                return;

            var results = new List<(string nom, long t, List<int> chemin)>();
            var sw = Stopwatch.StartNew();

            sw.Restart();
            var chDij = graphe.CheminDijkstra(DepartId, ArriveeId);
            sw.Stop();
            results.Add(("Dijkstra", sw.ElapsedMilliseconds, chDij));

            sw.Restart();
            var chBell = graphe.CheminBellmanFord(DepartId, ArriveeId);
            sw.Stop();
            results.Add(("Bellman-Ford", sw.ElapsedMilliseconds, chBell));

            sw.Restart();
            var chFloy = graphe.CheminFloydWarshall(DepartId, ArriveeId);
            sw.Stop();
            results.Add(("Floyd-Warshall", sw.ElapsedMilliseconds, chFloy));

            var fastest = results.OrderBy(r => r.t).First();
            foreach (var id in fastest.chemin)
            {
                if (fastest.nom == "Dijkstra") dijkstraVisites.Add(id);
                else if (fastest.nom == "Bellman-Ford") bellmanVisites.Add(id);
                else if (fastest.nom == "Floyd-Warshall") floydVisites.Add(id);
            }

            string msg = string.Join("\n",
                          results.Select(r => $"{r.nom} : {r.t} ms (Chemin : {string.Join("-", r.chemin)})"))
                      + $"\n\nLe plus rapide est : {fastest.nom}";

            AfficherEnPleinEcran("Comparaison des algorithmes", msg);
            Invalidate();
        }

        private void BoutonColoration_Click(object s, EventArgs e)
        {
            ClearAll();
            Brush[] palette = {
                Brushes.LightBlue, Brushes.LightGreen, Brushes.LightSalmon,
                Brushes.LightYellow, Brushes.LightPink,  Brushes.LightGray
            };

            var ordre = graphe.Noeuds.Values
                            .OrderByDescending(n => n.Voisins.Count)
                            .Select(n => n.Id)
                            .ToList();

            var couleurParNoeud = new Dictionary<int, int>();
            int couleurCourante = 0;
            foreach (var id in ordre)
            {
                if (couleurParNoeud.ContainsKey(id)) continue;
                couleurParNoeud[id] = couleurCourante;
                foreach (var autre in ordre)
                {
                    if (couleurParNoeud.ContainsKey(autre)) continue;
                    bool conflit = graphe.Noeuds[autre].Voisins
                                           .Any(v => couleurParNoeud.TryGetValue(v.Id, out int c) &&
                                                     c == couleurCourante);
                    if (!conflit)
                        couleurParNoeud[autre] = couleurCourante;
                }
                couleurCourante++;
            }

            foreach (var kv in couleurParNoeud)
                coloration[kv.Key] = palette[kv.Value % palette.Length];

            Invalidate();
        }

        private void AfficherEnPleinEcran(string titre, string contenu)
        {
            var fen = new Form { Text = titre, WindowState = FormWindowState.Maximized };
            var tb = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Dock = DockStyle.Fill, Text = contenu };
            fen.Controls.Add(tb);
            fen.ShowDialog();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CalculatePositions();
            Invalidate();
        }
    }
}
