using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;

namespace Projet_PSI
{
    /// <summary>
    /// Fenêtre d’affichage du graphe.
    /// </summary>
    public class FenetreGraphe : Form
    {
        private Button btnAfficherInfos;
        private Button btnTestAlgo;
        private Graphe<Station> graphe;
        private Dictionary<int, Point> positions;
        private int rayon = 20;

        public FenetreGraphe(Graphe<Station> graphe)
        {
            this.graphe = graphe;
            this.Text = "Visualisation du Graphe";
            this.Size = new Size(800, 600);

            this.Paint += new PaintEventHandler(DessinerGraphe);
            this.Controls.Add(CreerBoutonInfos());
            this.Controls.Add(CreerBoutonTestAlgo());
            this.MouseClick += new MouseEventHandler(SourisCliquee);

            GenererPositions();
        }

        private Button CreerBoutonInfos()
        {
            btnAfficherInfos = new Button();
            btnAfficherInfos.Text = "Afficher Inf";
            btnAfficherInfos.Location = new Point(10, 10);
            btnAfficherInfos.Click += new EventHandler(BoutonInfosClique);
            return btnAfficherInfos;
        }

        private Button CreerBoutonTestAlgo()
        {
            btnTestAlgo = new Button();
            btnTestAlgo.Text = "Test algos";
            btnTestAlgo.Location = new Point(10, 40);
            btnTestAlgo.Click += new EventHandler(BoutonTestAlgoClique);
            return btnTestAlgo;
        }

        private void BoutonInfosClique(object sender, EventArgs e)
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine("Liste d'adjacence:");
            message.AppendLine(graphe.AfficherListeAdjacence());
            message.AppendLine("\nVérification des connexions:");
            message.AppendLine($"Station A connectée à Station B ? {graphe.ExisteLien(0, 1)}");
            message.AppendLine($"Station A connectée à Station D ? {graphe.ExisteLien(0, 3)}");
            message.AppendLine("\nParcours en largeur (BFS) depuis la station 0:");
            message.AppendLine(graphe.ParcoursLargeur(0));
            message.AppendLine("\nParcours en profondeur (DFS) depuis la station 0:");
            message.AppendLine(graphe.ParcoursProfondeur(0));
            message.AppendLine($"\nLe graphe est connexe ? {graphe.EstConnexe()}");

            MessageBox.Show(message.ToString(), "Informations sur le Graphe",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BoutonTestAlgoClique(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            var sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            var dijkstra = graphe.Dijkstra(0);
            sw.Stop();
            sb.AppendLine($"Dijkstra : {sw.ElapsedMilliseconds} ms");
            sw.Reset();

            try
            {
                sw.Start();
                var bellman = graphe.BellmanFord(0);
                sw.Stop();
                sb.AppendLine($"Bellman-Ford : {sw.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                sw.Stop();
                sb.AppendLine($"Bellman-Ford : Erreur - {ex.Message}");
            }
            sw.Reset();

            sw.Start();
            var floyd = graphe.FloydWarshall();
            sw.Stop();
            sb.AppendLine($"Floyd-Warshall : {sw.ElapsedMilliseconds} ms");

            MessageBox.Show(sb.ToString(), "Temps d'exécution des algorithmes",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Génère des positions aléatoires pour l’affichage des nœuds.
        /// </summary>
        private void GenererPositions()
        {
            positions = new Dictionary<int, Point>();
            Random rand = new Random();
            foreach (var noeud in graphe.Noeuds.Values)
            {
                positions[noeud.Id] = new Point(rand.Next(100, 700), rand.Next(100, 500));
            }
        }

        private void DessinerGraphe(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Dessiner les liens
            foreach (var lien in graphe.Liens)
            {
                Point p1 = positions[lien.Noeud1.Id];
                Point p2 = positions[lien.Noeud2.Id];
                g.DrawLine(Pens.Black, p1, p2);
            }

            // Dessiner les nœuds
            foreach (var noeud in graphe.Noeuds.Values)
            {
                Point pos = positions[noeud.Id];
                g.FillEllipse(Brushes.LightBlue, pos.X - rayon, pos.Y - rayon, 2 * rayon, 2 * rayon);
                g.DrawEllipse(Pens.Black, pos.X - rayon, pos.Y - rayon, 2 * rayon, 2 * rayon);
                g.DrawString(noeud.Data.ToString(), new Font("Arial", 10),
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
                    string info = $"ID: {noeud.Id}\nStation: {noeud.Data}\nVoisins: ";
                    info += string.Join(", ", graphe.Liens
                        .Where(l => l.Noeud1.Id == noeud.Id || l.Noeud2.Id == noeud.Id)
                        .Select(l => l.Noeud1.Id == noeud.Id ? l.Noeud2.Data.ToString() : l.Noeud1.Data.ToString()));
                    MessageBox.Show(info, "Informations sur le nœud",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                }
            }
        }
    }
}
