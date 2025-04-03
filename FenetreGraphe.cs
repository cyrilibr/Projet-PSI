using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Projet_PSI
{
    public class FenetreGraphe<T> : Form
    {
        private Graphe<T> graphe;// Instance du graphe générique à afficher
        private Dictionary<int, Point> positions;// Dictionnaire des positions aléatoires des nœuds
        private int rayon = 20;// Rayon des cercles représentant les nœuds

        private Button btnListeAdjacence;
        private Button btnMatriceAdjacence;
        private Button btnBFS;
        private Button btnDFS;
        private Button btnConnexe;
        private Button btnCycle;

        /// <summary>
        /// Constructeur de la classe FenetreGraphe (générique).
        /// </summary>
        /// <param name="graphe">Instance du graphe générique à afficher.</param>
        public FenetreGraphe(Graphe<T> graphe)
        {
            this.graphe = graphe;
            this.Text = "Visualisation du Graphe";
            this.Size = new Size(800, 600);

            this.Paint += DessinerGraphe;

            InitialiserBoutons();

            this.MouseClick += SourisCliquee;

            GenererPositions();
        }

        /// <summary>
        /// Crée et positionne les différents boutons sur la fenêtre.
        /// </summary>
        private void InitialiserBoutons()
        {
            btnListeAdjacence = new Button();
            btnListeAdjacence.Text = "Liste d'adjacence";
            btnListeAdjacence.Location = new Point(10, 10);
            btnListeAdjacence.Click += BoutonListeAdjacence_Click;
            this.Controls.Add(btnListeAdjacence);

            btnMatriceAdjacence = new Button();
            btnMatriceAdjacence.Text = "Matrice d'adjacence";
            btnMatriceAdjacence.Location = new Point(50, 10);
            btnMatriceAdjacence.Click += BoutonMatriceAdjacence_Click;
            this.Controls.Add(btnMatriceAdjacence);

            btnBFS = new Button();
            btnBFS.Text = "BFS (depuis 0)";
            btnBFS.Location = new Point(90, 10);
            btnBFS.Click += BoutonBFS_Click;
            this.Controls.Add(btnBFS);

            btnDFS = new Button();
            btnDFS.Text = "DFS (depuis 0)";
            btnDFS.Location = new Point(130, 10);
            btnDFS.Click += BoutonDFS_Click;
            this.Controls.Add(btnDFS);

            btnConnexe = new Button();
            btnConnexe.Text = "Est connexe ?";
            btnConnexe.Location = new Point(170, 10);
            btnConnexe.Click += BoutonConnexe_Click;
            this.Controls.Add(btnConnexe);

            btnCycle = new Button();
            btnCycle.Text = "Contient un cycle ?";
            btnCycle.Location = new Point(210, 10);
            btnCycle.Click += BoutonCycle_Click;
            this.Controls.Add(btnCycle);
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
                positions[noeud.Id] = new Point(rand.Next(100, 700), rand.Next(100, 500));
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

                g.FillEllipse(Brushes.LightBlue, pos.X - rayon, pos.Y - rayon, 2 * rayon, 2 * rayon);
                g.DrawEllipse(Pens.Black, pos.X - rayon, pos.Y - rayon, 2 * rayon, 2 * rayon);

                string affichage = noeud.Data?.ToString() ?? "null";
                g.DrawString(affichage, new Font("Arial", 10),
                             Brushes.Black, pos.X - (rayon / 2), pos.Y - (rayon / 2));
            }
        }

        /// <summary>
        /// Gère le clic de la souris pour afficher les informations d'un nœud
        /// quand on clique sur son cercle.
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
                    string message = $"ID: {noeud.Id}\nData (T): {affichageData}\nLiens: ";

                    var liensDuNoeud = graphe.Liens
                        .Where(l => l.Noeud1.Id == noeud.Id || l.Noeud2.Id == noeud.Id)
                        .Select(l => l.Noeud1.Id == noeud.Id ? l.Noeud2.Id : l.Noeud1.Id);

                    message += string.Join(", ", liensDuNoeud);

                    MessageBox.Show(message, "Informations sur le sommet",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                }
            }
        }

        #region Événements des boutons

        private void BoutonListeAdjacence_Click(object sender, EventArgs e)
        {
            string msg = "Liste d'adjacence :\n" + graphe.AfficherListeAdjacence();
            MessageBox.Show(msg, "Liste d'adjacence", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BoutonMatriceAdjacence_Click(object sender, EventArgs e)
        {
            string msg = "Matrice d'adjacence :\n" + graphe.AfficherMatriceAdjacence();
            MessageBox.Show(msg, "Matrice d'adjacence", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BoutonBFS_Click(object sender, EventArgs e)
        {
            string msg = "Parcours BFS (depuis le nœud 0) :\n" + graphe.ParcoursLargeur(0);
            MessageBox.Show(msg, "BFS", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BoutonDFS_Click(object sender, EventArgs e)
        {
            string msg = "Parcours DFS (depuis le nœud 0) :\n" + graphe.ParcoursProfondeur(0);
            MessageBox.Show(msg, "DFS", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BoutonConnexe_Click(object sender, EventArgs e)
        {
            bool connexe = graphe.EstConnexe();
            string msg = $"Le graphe est-il connexe ? {connexe}";
            MessageBox.Show(msg, "Est Connexe ?", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BoutonCycle_Click(object sender, EventArgs e)
        {
            bool cycle = graphe.ContientCycle();
            string msg = $"Le graphe contient-il un cycle ? {cycle}";
            MessageBox.Show(msg, "Contient un cycle ?", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion
    }
}
