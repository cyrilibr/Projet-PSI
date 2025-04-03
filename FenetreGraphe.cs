using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Projet_PSI
{
    public class FenetreGraphe<T> : Form
    {
        private Graphe<T> graphe;//Instance du graphe générique
        private Dictionary<int, Point> positions;//Position des nœuds
        private int rayon = 20;

        private Button btnListeAdjacence;
        private Button btnMatriceAdjacence;
        private Button btnBFS;
        private Button btnDFS;
        private Button btnConnexe;
        private Button btnCycle;
        private Button btnDijkstra;       
        private Button btnBellman;        
        private Button btnFloyd;          
        private Button btnComparer;       
        private Button btnReset;          

        private HashSet<int> bfsVisites = new HashSet<int>();
        private HashSet<int> dfsVisites = new HashSet<int>();
        private HashSet<int> dijkstraVisites = new HashSet<int>();
        private HashSet<int> bellmanVisites = new HashSet<int>();
        private HashSet<int> floydVisites = new HashSet<int>();

        public FenetreGraphe(Graphe<T> graphe)
        {
            this.graphe = graphe;
            this.Text = "Visualisation du Graphe";
            this.WindowState = FormWindowState.Maximized;

            this.Paint += DessinerGraphe;
            this.MouseClick += SourisCliquee;

            InitialiserBoutons();
            GenererPositions();
        }

        private void InitialiserBoutons()
        {
            int x = 10;
            int y = 10;

            Button CreerBouton(string texte, EventHandler onClick)
            {
                var btn = new Button();
                btn.Text = texte;
                btn.Location = new Point(x, y);
                btn.Click += onClick;
                this.Controls.Add(btn);

                x += 130;
                return btn;
            }

            btnListeAdjacence = CreerBouton("Liste d'adjacence", BoutonListeAdjacence_Click);
            btnMatriceAdjacence = CreerBouton("Matrice d'adjacence", BoutonMatriceAdjacence_Click);
            btnBFS = CreerBouton("BFS (1->tous)", BoutonBFS_Click);
            btnDFS = CreerBouton("DFS (1->tous)", BoutonDFS_Click);
            btnConnexe = CreerBouton("Est connexe ?", BoutonConnexe_Click);
            btnCycle = CreerBouton("Contient cycle ?", BoutonCycle_Click);
            btnDijkstra = CreerBouton("Dijkstra (1->3)", BoutonDijkstra_Click);
            btnBellman = CreerBouton("Bellman (1->3)", BoutonBellman_Click);
            btnFloyd = CreerBouton("Floyd (1->3)", BoutonFloyd_Click);
            btnComparer = CreerBouton("Comparer Algos", BoutonComparer_Click);
            btnReset = CreerBouton("Réinitialiser", BoutonReset_Click);
        }

        private void GenererPositions()
        {
            positions = new Dictionary<int, Point>();
            Random rand = new Random();
            foreach (var noeud in graphe.Noeuds.Values)
            {
                positions[noeud.Id] = new Point(rand.Next(100, 1500), rand.Next(100, 900));
            }
        }

        private void DessinerGraphe(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            foreach (var lien in graphe.Liens)
            {
                var p1 = positions[lien.Noeud1.Id];
                var p2 = positions[lien.Noeud2.Id];
                g.DrawLine(Pens.Black, p1, p2);
            }

            foreach (var noeud in graphe.Noeuds.Values)
            {
                int id = noeud.Id;
                var pos = positions[id];

                Brush couleurRemplissage = Brushes.LightBlue;

                if (bfsVisites.Contains(id)) couleurRemplissage = Brushes.Yellow;
                else if (dfsVisites.Contains(id)) couleurRemplissage = Brushes.Red;
                else if (dijkstraVisites.Contains(id)) couleurRemplissage = Brushes.Green;
                else if (bellmanVisites.Contains(id)) couleurRemplissage = Brushes.Violet;
                else if (floydVisites.Contains(id)) couleurRemplissage = Brushes.Pink;

                g.FillEllipse(couleurRemplissage, pos.X - rayon, pos.Y - rayon, 2 * rayon, 2 * rayon);

                Pen contour = Pens.Black;

                g.DrawEllipse(contour, pos.X - rayon, pos.Y - rayon, 2 * rayon, 2 * rayon);

                string affichage = noeud.Data?.ToString() ?? "null";
                g.DrawString(affichage, new Font("Arial", 10),
                             Brushes.Black, pos.X - (rayon / 2), pos.Y - (rayon / 2));
            }
        }

        private void SourisCliquee(object sender, MouseEventArgs e)
        {
            foreach (var noeud in graphe.Noeuds.Values)
            {
                Point pos = positions[noeud.Id];
                Rectangle zone = new Rectangle(pos.X - rayon, pos.Y - rayon, 2 * rayon, 2 * rayon);

                if (zone.Contains(e.Location))
                {
                    string affichageData = noeud.Data?.ToString() ?? "null";
                    var liensDuNoeud = graphe.Liens
                        .Where(l => l.Noeud1.Id == noeud.Id || l.Noeud2.Id == noeud.Id)
                        .Select(l => l.Noeud1.Id == noeud.Id ? l.Noeud2.Id : l.Noeud1.Id);

                    string message = $"ID: {noeud.Id}\r\n" +
                                     $"Data (T): {affichageData}\r\n" +
                                     $"Liens: {string.Join(", ", liensDuNoeud)}";

                    AfficherEnPleinEcran("Informations sur le sommet", message);
                    break;
                }
            }
        }

        private void BoutonListeAdjacence_Click(object sender, EventArgs e)
        {
            string info = "Liste d'adjacence :\r\n" + graphe.AfficherListeAdjacence();
            AfficherEnPleinEcran("Liste d'adjacence", info);
        }

        private void BoutonMatriceAdjacence_Click(object sender, EventArgs e)
        {
            string info = "Matrice d'adjacence :\r\n" + graphe.AfficherMatriceAdjacence();
            AfficherEnPleinEcran("Matrice d'adjacence", info);
        }

        private void BoutonConnexe_Click(object sender, EventArgs e)
        {
            bool estConnexe = graphe.EstConnexe();
            string info = $"Le graphe est-il connexe ? {estConnexe}";
            AfficherEnPleinEcran("Est Connexe ?", info);
        }

        private void BoutonCycle_Click(object sender, EventArgs e)
        {
            bool cycle = graphe.ContientCycle();
            string info = $"Le graphe contient-il un cycle ? {cycle}";
            AfficherEnPleinEcran("Contient un cycle ?", info);
        }

        private void BoutonBFS_Click(object sender, EventArgs e)
        {
            ResetAllColorations();

            if (!graphe.Noeuds.ContainsKey(1)) return;
            var queue = new Queue<int>();
            queue.Enqueue(1);
            bfsVisites.Add(1);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                var noeudCourant = graphe.Noeuds[current];
                foreach (var voisin in noeudCourant.Voisins)
                {
                    if (!bfsVisites.Contains(voisin.Id))
                    {
                        bfsVisites.Add(voisin.Id);
                        queue.Enqueue(voisin.Id);
                    }
                }
            }
            Invalidate();
        }

        private void BoutonDFS_Click(object sender, EventArgs e)
        {
            ResetAllColorations();

            if (!graphe.Noeuds.ContainsKey(1)) return;
            var stack = new Stack<int>();
            stack.Push(1);

            while (stack.Count > 0)
            {
                int current = stack.Pop();
                if (!dfsVisites.Contains(current))
                {
                    dfsVisites.Add(current);

                    var noeudCourant = graphe.Noeuds[current];
                    foreach (var voisin in noeudCourant.Voisins)
                    {
                        if (!dfsVisites.Contains(voisin.Id))
                        {
                            stack.Push(voisin.Id);
                        }
                    }
                }
            }
            Invalidate();
        }

        private void BoutonDijkstra_Click(object sender, EventArgs e)
        {
            ResetAllColorations();

            if (!graphe.Noeuds.ContainsKey(9) || !graphe.Noeuds.ContainsKey(18)) return;
            var chemin = graphe.CheminDijkstra(9, 18);

            foreach (var id in chemin)
            {
                dijkstraVisites.Add(id);
            }
            Invalidate();
        }

        private void BoutonBellman_Click(object sender, EventArgs e)
        {
            ResetAllColorations();

            if (!graphe.Noeuds.ContainsKey(11) || !graphe.Noeuds.ContainsKey(30)) return;
            var chemin = graphe.CheminBellmanFord(11, 30);

            foreach (var id in chemin)
            {
                bellmanVisites.Add(id);
            }
            Invalidate();
        }

        private void BoutonFloyd_Click(object sender, EventArgs e)
        {
            ResetAllColorations();

            if (!graphe.Noeuds.ContainsKey(6) || !graphe.Noeuds.ContainsKey(22)) return;
            var chemin = graphe.CheminFloydWarshall(6, 22);

            foreach (var id in chemin)
            {
                floydVisites.Add(id);
            }
            Invalidate();
        }

        private void BoutonComparer_Click(object sender, EventArgs e)
        {
            ResetAllColorations();

            int noeud_d = 11;
            int noeud_f = 21;

            if (!graphe.Noeuds.ContainsKey(noeud_d) || !graphe.Noeuds.ContainsKey(noeud_f)) return;

            var resultats = new List<(string nom, long tempsMs, List<int> chemin)>();

            var sw = Stopwatch.StartNew();
            
            sw.Restart();
            var cheminDij = graphe.CheminDijkstra(noeud_d, noeud_f);
            sw.Stop();
            resultats.Add(("Dijkstra", sw.ElapsedMilliseconds, cheminDij));

            sw.Restart();
            var cheminBell = graphe.CheminBellmanFord(noeud_d, noeud_f);
            sw.Stop();
            resultats.Add(("Bellman-Ford", sw.ElapsedMilliseconds, cheminBell));

            sw.Restart();
            var cheminFloyd = graphe.CheminFloydWarshall(noeud_d, noeud_f);
            sw.Stop();
            resultats.Add(("Floyd-Warshall", sw.ElapsedMilliseconds, cheminFloyd));

            var plusRapide = resultats.OrderBy(r => r.tempsMs).First();

            // 1) On colorie "en plein" le chemin du plus rapide
            if (plusRapide.nom == "Dijkstra")
                foreach (var id in plusRapide.chemin) dijkstraVisites.Add(id);
            if (plusRapide.nom == "Bellman-Ford")
                foreach (var id in plusRapide.chemin) bellmanVisites.Add(id);
            if (plusRapide.nom == "Floyd-Warshall")
                foreach (var id in plusRapide.chemin) floydVisites.Add(id);

            foreach (var r in resultats)
            {
                if (r.nom == plusRapide.nom) continue;  
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Comparaison des algorithmes (millisecondes) :");
            foreach (var r in resultats.OrderBy(r => r.tempsMs))
            {
                sb.AppendLine($"{r.nom} : {r.tempsMs} ms (Chemin: {string.Join("-", r.chemin)})");
            }
            sb.AppendLine($"\nLe plus rapide est : {plusRapide.nom}");

            AfficherEnPleinEcran("Comparaison des algos", sb.ToString());

            Invalidate();
        }

        private void BoutonReset_Click(object sender, EventArgs e)
        {
            ResetAllColorations();
            Invalidate();
        }

        private void ResetAllColorations()
        {
            bfsVisites.Clear();
            dfsVisites.Clear();
            dijkstraVisites.Clear();
            bellmanVisites.Clear();
            floydVisites.Clear();
        }

        private void AfficherEnPleinEcran(string titre, string contenu)
        {
            Form fenetre = new Form();
            fenetre.Text = titre;
            fenetre.WindowState = FormWindowState.Maximized;

            TextBox textBox = new TextBox();
            textBox.Multiline = true;
            textBox.Dock = DockStyle.Fill;
            textBox.ReadOnly = true;
            textBox.ScrollBars = ScrollBars.Both;
            textBox.Text = contenu;

            fenetre.Controls.Add(textBox);
            fenetre.ShowDialog();
        }

        /*
        private Graphe<T> graphe;//Instance du graphe générique à afficher
        private Dictionary<int, Point> positions;//Dictionnaire des positions aléatoires des nœuds
        private int rayon = 20;//Rayon des cercles représentant les nœuds

        private Button btnListeAdjacence;
        private Button btnMatriceAdjacence;
        private Button btnBFS;
        private Button btnDFS;
        private Button btnConnexe;
        private Button btnCycle;

        private HashSet<int> bfsVisites = new HashSet<int>();
        private HashSet<int> dfsVisites = new HashSet<int>();

        /// <summary>
        /// Constructeur de la classe FenetreGraphe (générique).
        /// </summary>
        /// <param name="graphe">Instance du graphe générique à afficher.</param>
        public FenetreGraphe(Graphe<T> graphe)
        {
            this.graphe = graphe;
            this.Text = "Visualisation du Graphe";

            this.WindowState = FormWindowState.Maximized;

            this.Paint += DessinerGraphe;

            InitialiserBoutons();

            this.MouseClick += SourisCliquee;

            GenererPositions();
        }

        /// <summary>
        /// Crée et positionne les différents boutons sur la fenêtre (en ligne, en haut à gauche).
        /// </summary>
        private void InitialiserBoutons()
        {
            int x = 10;
            int y = 10;

            Button CreerBouton(string texte, EventHandler onClick)
            {
                var btn = new Button();
                btn.Text = texte;
                btn.Location = new Point(x, y);
                btn.Click += onClick;
                this.Controls.Add(btn);

                x += 130;
                return btn;
            }

            btnListeAdjacence = CreerBouton("Liste d'adjacence", BoutonListeAdjacence_Click);
            btnMatriceAdjacence = CreerBouton("Matrice d'adjacence", BoutonMatriceAdjacence_Click);
            btnBFS = CreerBouton("BFS (depuis 1)", BoutonBFS_Click);
            btnDFS = CreerBouton("DFS (depuis 1)", BoutonDFS_Click);
            btnConnexe = CreerBouton("Est connexe ?", BoutonConnexe_Click);
            btnCycle = CreerBouton("Contient un cycle ?", BoutonCycle_Click);
        }

        /// <summary>
        /// Génère des positions aléatoires pour chaque nœud du graphe.
        /// </summary>
        private void GenererPositions()
        {
            positions = new Dictionary<int, Point>();
            Random rand = new Random();

            foreach (var noeud in graphe.Noeuds.Values)
            {
                positions[noeud.Id] = new Point(rand.Next(100, 1500), rand.Next(100, 900));
            }
        }

        /// <summary>
        /// Dessine le graphe sur la fenêtre (événement Paint).
        /// </summary>
        private void DessinerGraphe(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            foreach (var lien in graphe.Liens)
            {
                Point p1 = positions[lien.Noeud1.Id];
                Point p2 = positions[lien.Noeud2.Id];
                g.DrawLine(Pens.Black, p1, p2);
            }

            foreach (var noeud in graphe.Noeuds.Values)
            {
                Point pos = positions[noeud.Id];

                // Choix de la couleur selon BFS / DFS
                // Si un nœud est dans BFS ET DFS, on peut choisir de prioriser BFS par exemple
                Brush couleurRemplissage;
                if (bfsVisites.Contains(noeud.Id))
                {
                    couleurRemplissage = Brushes.Yellow; // BFS
                }
                else if (dfsVisites.Contains(noeud.Id))
                {
                    couleurRemplissage = Brushes.Red; // DFS
                }
                else
                {
                    couleurRemplissage = Brushes.LightBlue; // Nœud non-visité par BFS/DFS
                }

                g.FillEllipse(couleurRemplissage, pos.X - rayon, pos.Y - rayon, 2 * rayon, 2 * rayon);
                g.DrawEllipse(Pens.Black, pos.X - rayon, pos.Y - rayon, 2 * rayon, 2 * rayon);

                string affichage = noeud.Data?.ToString() ?? "null";
                g.DrawString(affichage, new Font("Arial", 10),
                             Brushes.Black, pos.X - (rayon / 2), pos.Y - (rayon / 2));
            }
        }

        /// <summary>
        /// Gère le clic de la souris pour afficher les informations d'un nœud
        /// quand on clique sur son cercle (Fenêtre plein écran).
        /// </summary>
        private void SourisCliquee(object sender, MouseEventArgs e)
        {
            foreach (var noeud in graphe.Noeuds.Values)
            {
                Point pos = positions[noeud.Id];
                Rectangle zone = new Rectangle(pos.X - rayon, pos.Y - rayon, 2 * rayon, 2 * rayon);

                if (zone.Contains(e.Location))
                {
                    string affichageData = noeud.Data?.ToString() ?? "null";
                    var liensDuNoeud = graphe.Liens
                        .Where(l => l.Noeud1.Id == noeud.Id || l.Noeud2.Id == noeud.Id)
                        .Select(l => l.Noeud1.Id == noeud.Id ? l.Noeud2.Id : l.Noeud1.Id);

                    string message = $"ID: {noeud.Id}\r\n" +
                                     $"Data (T): {affichageData}\r\n" +
                                     $"Liens: {string.Join(", ", liensDuNoeud)}";

                    // On affiche une Form maximisée avec un TextBox multiligne
                    AfficherEnPleinEcran("Informations sur le sommet", message);
                    break;
                }
            }
        }

        #region Événements des boutons

        private void BoutonListeAdjacence_Click(object sender, EventArgs e)
        {
            string info = "Liste d'adjacence :\r\n" + graphe.AfficherListeAdjacence();
            AfficherEnPleinEcran("Liste d'adjacence", info);
        }

        private void BoutonMatriceAdjacence_Click(object sender, EventArgs e)
        {
            string info = "Matrice d'adjacence :\r\n" + graphe.AfficherMatriceAdjacence();
            AfficherEnPleinEcran("Matrice d'adjacence", info);
        }

        /// <summary>
        /// Exécute un BFS depuis le nœud d'ID 1 et colorie les nœuds visités.
        /// </summary>
        private void BoutonBFS_Click(object sender, EventArgs e)
        {
            bfsVisites.Clear();
            dfsVisites.Clear();

            if (!graphe.Noeuds.ContainsKey(1)) return;

            var queue = new Queue<int>();
            queue.Enqueue(1);
            bfsVisites.Add(1);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                var noeudCourant = graphe.Noeuds[current];
                foreach (var voisin in noeudCourant.Voisins)
                {
                    if (!bfsVisites.Contains(voisin.Id))
                    {
                        bfsVisites.Add(voisin.Id);
                        queue.Enqueue(voisin.Id);
                    }
                }
            }

            Invalidate();
        }

        /// <summary>
        /// Exécute un DFS depuis le nœud d'ID 1 et colorie les nœuds visités.
        /// </summary>
        private void BoutonDFS_Click(object sender, EventArgs e)
        {
            dfsVisites.Clear();
            bfsVisites.Clear();

            if (!graphe.Noeuds.ContainsKey(1)) return;

            var stack = new Stack<int>();
            stack.Push(1);

            while (stack.Count > 0)
            {
                int current = stack.Pop();
                if (!dfsVisites.Contains(current))
                {
                    dfsVisites.Add(current);

                    var noeudCourant = graphe.Noeuds[current];
                    foreach (var voisin in noeudCourant.Voisins)
                    {
                        if (!dfsVisites.Contains(voisin.Id))
                        {
                            stack.Push(voisin.Id);
                        }
                    }
                }
            }

            Invalidate();
        }

        private void BoutonConnexe_Click(object sender, EventArgs e)
        {
            bool estConnexe = graphe.EstConnexe();
            string info = $"Le graphe est-il connexe ? {estConnexe}";
            AfficherEnPleinEcran("Est Connexe ?", info);
        }

        private void BoutonCycle_Click(object sender, EventArgs e)
        {
            bool cycle = graphe.ContientCycle();
            string info = $"Le graphe contient-il un cycle ? {cycle}";
            AfficherEnPleinEcran("Contient un cycle ?", info);
        }

        #endregion

        /// <summary>
        /// Crée et affiche en plein écran une nouvelle fenêtre
        /// contenant le texte passé en paramètre.
        /// (Utilisé pour la liste d'adjacence, la matrice, ou les infos d'un nœud.)
        /// </summary>
        /// <param name="titre">Titre de la nouvelle fenêtre.</param>
        /// <param name="contenu">Texte à afficher.</param>
        private void AfficherEnPleinEcran(string titre, string contenu)
        {
            Form fenetre = new Form();
            fenetre.Text = titre;
            fenetre.WindowState = FormWindowState.Maximized;

            TextBox textBox = new TextBox();
            textBox.Multiline = true;
            textBox.Dock = DockStyle.Fill;
            textBox.ReadOnly = true;
            textBox.ScrollBars = ScrollBars.Both;
            textBox.Text = contenu;

            fenetre.Controls.Add(textBox);
            fenetre.ShowDialog();
        }
        */
    }
}