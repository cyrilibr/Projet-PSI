using System.Drawing.Drawing2D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Projet_PSI
{
    /// <summary>
    /// Formulaire de visualisation du graphe de stations avec positions géographiques,
    /// affichage des arêtes orientées, pondérées en temps de transport, et coloration.
    /// Permet de choisir dynamiquement les IDs de départ et d'arrivée pour les algorithmes.
    /// </summary>
    public class FenetreGraphe<T> : Form
    {
        private readonly Graphe<T> graphe;
        private Dictionary<int, PointF> positions = new Dictionary<int, PointF>();
        private const int Rayon = 10;
        private const int MargeX = 50;
        private const int MargeY = 50;

        // Contrôles pour choix des ID
        private NumericUpDown nudDepart;
        private NumericUpDown nudArrivee;

        // Boutons
        private Button btnReset, btnListeAdj, btnMatrice, btnBFS, btnDFS,
                       btnConnexe, btnCycle, btnDijkstra, btnBellman,
                       btnFloyd, btnComparer, btnColoration;

        // États de visites / coloration
        private readonly HashSet<int> bfsVisites = new();
        private readonly HashSet<int> dfsVisites = new();
        private readonly HashSet<int> dijkstraVisites = new();
        private readonly HashSet<int> bellmanVisites = new();
        private readonly HashSet<int> floydVisites = new();
        private readonly Dictionary<int, Brush> coloration = new();

        public FenetreGraphe(Graphe<T> graphe)
        {
            this.graphe = graphe;
            Text = "Visualisation Géographique du Graphe";
            WindowState = FormWindowState.Maximized;
            BackColor = Color.White;

            // Optimisation du rendu
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);
            DoubleBuffered = true;

            InitializeComponents();
            CalculatePositions();
            Paint += DessinerGraphe;
        }

        private void InitializeComponents()
        {
            int x = MargeX, y = MargeY, espace = 10;

            // Label + NumericUpDown pour l'ID de départ
            var lblDepart = new Label
            {
                Text = "Départ ID:",
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };
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
            x += nudDepart.Width + espace;

            // Label + NumericUpDown pour l'ID d'arrivée
            var lblArrivee = new Label
            {
                Text = "Arrivée ID:",
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };
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

            // Passage à la ligne suivante pour les boutons
            y += nudDepart.Height + espace;
            x = MargeX;

            // Méthode locale de création de boutons
            Button Create(string text, EventHandler handler)
            {
                var btn = new Button
                {
                    Text = text,
                    AutoSize = true,
                    Location = new Point(x, y),
                    Font = new Font("Segoe UI", 9f, FontStyle.Regular)
                };
                btn.Click += handler;
                Controls.Add(btn);
                x += btn.Width + espace;
                return btn;
            }

            btnReset = Create("Réinitialiser", (s, e) => { ClearAll(); Invalidate(); });
            btnListeAdj = Create("Liste Adjacence", BoutonListeAdj_Click);
            btnMatrice = Create("Matrice Adjacence", BoutonMatriceAdj_Click);
            btnBFS = Create("BFS (→ tous)", BoutonBFS_Click);
            btnDFS = Create("DFS (→ tous)", BoutonDFS_Click);
            btnConnexe = Create("Est connexe ?", BoutonConnexe_Click);
            btnCycle = Create("Contient cycle ?", BoutonCycle_Click);
            btnDijkstra = Create("Dijkstra", BoutonDijkstra_Click);
            btnBellman = Create("Bellman–Ford", BoutonBellman_Click);
            btnFloyd = Create("Floyd–Warshall", BoutonFloyd_Click);
            btnComparer = Create("Comparer algos", BoutonComparer_Click);
            btnColoration = Create("Coloration", BoutonColoration_Click);
        }

        private void CalculatePositions()
        {
            var stations = graphe.Noeuds.Values
                .Select(n => n.Data as Station)
                .Where(s => s != null)
                .ToList();
            if (!stations.Any()) return;

            double minLat = stations.Min(s => s.Latitude),
                   maxLat = stations.Max(s => s.Latitude),
                   minLon = stations.Min(s => s.Longitude),
                   maxLon = stations.Max(s => s.Longitude);

            float w = ClientSize.Width - 2 * MargeX;
            float h = ClientSize.Height - 2 * MargeY;
            positions.Clear();

            foreach (var kv in graphe.Noeuds)
            {
                if (kv.Value.Data is Station s)
                {
                    float px = (float)((s.Longitude - minLon) / (maxLon - minLon) * w) + MargeX;
                    float py = (float)((maxLat - s.Latitude) / (maxLat - minLat) * h) + MargeY;
                    positions[kv.Key] = new PointF(px, py);
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Fond dégradé très esthétique
            using var lg = new LinearGradientBrush(ClientRectangle,
                                                    Color.LightSkyBlue,
                                                    Color.White,
                                                    90f);
            e.Graphics.FillRectangle(lg, ClientRectangle);
        }

        private void DessinerGraphe(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Arêtes
            foreach (var lien in graphe.Liens)
            {
                if (!positions.TryGetValue(lien.Noeud1.Id, out var p1) ||
                    !positions.TryGetValue(lien.Noeud2.Id, out var p2))
                    continue;

                if (lien.Bidirectionnel)
                    DrawBidirectionalEdge(g, p1, p2, Pens.Gray);
                else
                    DrawDirectedEdge(g, p1, p2, Pens.Black);

                // Affichage du temps de transport au milieu
                var mid = new PointF((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
                g.DrawString($"{lien.Poids} min", Font, Brushes.DarkBlue, mid);
            }

            // Nœuds
            foreach (var kv in graphe.Noeuds)
            {
                if (!positions.TryGetValue(kv.Key, out var pos)) continue;

                Brush fill = Brushes.White;
                if (coloration.ContainsKey(kv.Key)) fill = coloration[kv.Key];
                else if (bfsVisites.Contains(kv.Key)) fill = Brushes.Yellow;
                else if (dfsVisites.Contains(kv.Key)) fill = Brushes.Red;
                else if (dijkstraVisites.Contains(kv.Key)) fill = Brushes.Green;
                else if (bellmanVisites.Contains(kv.Key)) fill = Brushes.Violet;
                else if (floydVisites.Contains(kv.Key)) fill = Brushes.Pink;

                g.FillEllipse(fill, pos.X - Rayon, pos.Y - Rayon, 2 * Rayon, 2 * Rayon);
                g.DrawEllipse(Pens.Blue, pos.X - Rayon, pos.Y - Rayon, 2 * Rayon, 2 * Rayon);

                string lbl = (kv.Value.Data as Station)?.Libelle ?? kv.Key.ToString();
                var sz = g.MeasureString(lbl, Font);
                g.DrawString(lbl, Font, Brushes.Black, pos.X - sz.Width / 2, pos.Y - sz.Height / 2);
            }
        }

        private void DrawDirectedEdge(Graphics g, PointF a, PointF b, Pen pen)
        {
            const float arrowSize = 8f;
            float dx = b.X - a.X, dy = b.Y - a.Y;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            if (len < 0.1f) return;
            float ux = dx / len, uy = dy / len;
            var src = new PointF(a.X + ux * Rayon, a.Y + uy * Rayon);
            var dst = new PointF(b.X - ux * Rayon, b.Y - uy * Rayon);

            g.DrawLine(pen, src, dst);
            // Flèche
            var perp = new PointF(-uy, ux);
            var p1 = new PointF(dst.X - ux * arrowSize + perp.X * (arrowSize / 2),
                                 dst.Y - uy * arrowSize + perp.Y * (arrowSize / 2));
            var p2 = new PointF(dst.X - ux * arrowSize - perp.X * (arrowSize / 2),
                                 dst.Y - uy * arrowSize - perp.Y * (arrowSize / 2));
            g.FillPolygon(pen.Brush, new[] { dst, p1, p2 });
        }

        private void DrawBidirectionalEdge(Graphics g, PointF p1, PointF p2, Pen pen)
        {
            // Ligne simple
            g.DrawLine(pen, p1, p2);
            // Deux flèches opposées
            DrawDirectedEdge(g, p1, p2, pen);
            DrawDirectedEdge(g, p2, p1, pen);
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
            var txt = "Liste d'adjacence :\r\n" + graphe.AfficherListeAdjacence();
            AfficherEnPleinEcran("Liste d'adjacence", txt);
        }

        private void BoutonMatriceAdj_Click(object s, EventArgs e)
        {
            var txt = "Matrice d'adjacence :\r\n" + graphe.AfficherMatriceAdjacence();
            AfficherEnPleinEcran("Matrice d'adjacence", txt);
        }

        private void BoutonConnexe_Click(object s, EventArgs e)
        {
            bool res = graphe.EstConnexe();
            AfficherEnPleinEcran("Connexité", $"Le graphe est connexe ? {res}");
        }

        private void BoutonCycle_Click(object s, EventArgs e)
        {
            bool res = graphe.ContientCycle();
            AfficherEnPleinEcran("Cycle dans le graphe", $"Le graphe contient un cycle ? {res}");
        }

        private void BoutonBFS_Click(object s, EventArgs e)
        {
            ClearAll();
            int start = (int)nudDepart.Value;
            if (graphe.Noeuds.ContainsKey(start))
            {
                var queue = new Queue<int>();
                queue.Enqueue(start);
                bfsVisites.Add(start);
                while (queue.Count > 0)
                {
                    int u = queue.Dequeue();
                    foreach (var n in graphe.Noeuds[u].Voisins)
                        if (bfsVisites.Add(n.Id))
                            queue.Enqueue(n.Id);
                }
            }
            Invalidate();
        }

        private void BoutonDFS_Click(object s, EventArgs e)
        {
            ClearAll();
            int start = (int)nudDepart.Value;
            if (graphe.Noeuds.ContainsKey(start))
            {
                var stack = new Stack<int>();
                stack.Push(start);
                while (stack.Count > 0)
                {
                    int u = stack.Pop();
                    if (dfsVisites.Add(u))
                        foreach (var n in graphe.Noeuds[u].Voisins)
                            if (!dfsVisites.Contains(n.Id))
                                stack.Push(n.Id);
                }
            }
            Invalidate();
        }

        private void BoutonDijkstra_Click(object s, EventArgs e)
        {
            ClearAll();
            int start = (int)nudDepart.Value, end = (int)nudArrivee.Value;
            if (graphe.Noeuds.ContainsKey(start) && graphe.Noeuds.ContainsKey(end))
                foreach (var id in graphe.CheminDijkstra(start, end))
                    dijkstraVisites.Add(id);
            Invalidate();
        }

        private void BoutonBellman_Click(object s, EventArgs e)
        {
            ClearAll();
            int start = (int)nudDepart.Value, end = (int)nudArrivee.Value;
            if (graphe.Noeuds.ContainsKey(start) && graphe.Noeuds.ContainsKey(end))
                foreach (var id in graphe.CheminBellmanFord(start, end))
                    bellmanVisites.Add(id);
            Invalidate();
        }

        private void BoutonFloyd_Click(object s, EventArgs e)
        {
            ClearAll();
            int start = (int)nudDepart.Value, end = (int)nudArrivee.Value;
            if (graphe.Noeuds.ContainsKey(start) && graphe.Noeuds.ContainsKey(end))
                foreach (var id in graphe.CheminFloydWarshall(start, end))
                    floydVisites.Add(id);
            Invalidate();
        }

        private void BoutonComparer_Click(object s, EventArgs e)
        {
            ClearAll();
            int start = (int)nudDepart.Value, end = (int)nudArrivee.Value;
            var results = new List<(string nom, long t, List<int> chemin)>();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            sw.Restart();
            var ch1 = graphe.CheminDijkstra(start, end);
            sw.Stop();
            results.Add(("Dijkstra", sw.ElapsedMilliseconds, ch1));

            sw.Restart();
            var ch2 = graphe.CheminBellmanFord(start, end);
            sw.Stop();
            results.Add(("Bellman–Ford", sw.ElapsedMilliseconds, ch2));

            sw.Restart();
            var ch3 = graphe.CheminFloydWarshall(start, end);
            sw.Stop();
            results.Add(("Floyd–Warshall", sw.ElapsedMilliseconds, ch3));

            var fastest = results.OrderBy(r => r.t).First();
            foreach (var id in fastest.chemin)
            {
                if (fastest.nom == "Dijkstra") dijkstraVisites.Add(id);
                if (fastest.nom == "Bellman–Ford") bellmanVisites.Add(id);
                if (fastest.nom == "Floyd–Warshall") floydVisites.Add(id);
            }

            string msg = string.Join("\r\n",
                results.Select(r => $"{r.nom} : {r.t} ms  → {string.Join(" → ", r.chemin)}")) +
                $"\r\n\nPlus rapide : {fastest.nom}";
            AfficherEnPleinEcran("Comparaison des algorithmes", msg);
            Invalidate();
        }

        private void BoutonColoration_Click(object s, EventArgs e)
        {
            ClearAll();
            Brush[] palette = {
                Brushes.LightBlue, Brushes.LightGreen, Brushes.LightSalmon,
                Brushes.LightYellow, Brushes.LightPink, Brushes.LightGray
            };
            foreach (var node in graphe.Noeuds.Keys)
            {
                var used = new HashSet<Brush>();
                foreach (var v in graphe.Noeuds[node].Voisins)
                    if (coloration.TryGetValue(v.Id, out var b))
                        used.Add(b);
                coloration[node] = palette.First(c => !used.Contains(c));
            }
            Invalidate();
        }

        private void AfficherEnPleinEcran(string titre, string contenu)
        {
            using var fen = new Form
            {
                Text = titre,
                WindowState = FormWindowState.Maximized,
                Font = new Font("Consolas", 10f, FontStyle.Regular)
            };
            var tb = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10f, FontStyle.Regular),
                Text = contenu
            };
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
