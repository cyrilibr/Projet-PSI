using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Projet_PSI
{
    public class FenetreGraphe : Form
    {
        private Button btnAfficherInfos; // Bouton pour afficher les informations du graphe
        private Graphe graphe; // Instance du graphe à afficher
        private Dictionary<int, Point> positions; // Dictionnaire des positions des nœuds
        private int rayon = 20; // Rayon des cercles représentant les nœuds

        /// <summary>
        /// Constructeur de la classe FenetreGraphe
        /// </summary>
        /// <param name="graphe">Instance du graphe à afficher</param>
        public FenetreGraphe(Graphe graphe)
        {
            this.graphe = graphe;
            this.Text = "Visualisation du Graphe";
            this.Size = new Size(800, 600);
            this.Paint += new PaintEventHandler(DessinerGraphe);
            this.Controls.Add(CreerBouton());
            this.MouseClick += new MouseEventHandler(SourisCliquee);
            GenererPositions();
        }

        /// <summary>
        /// Genere des positions aléatoires pour les nœuds du graphe
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
        /// Cree un bouton permettant d'afficher les informations du graphe
        /// </summary>
        /// <returns>Le bouton cree</returns>
        private Button CreerBouton()
        {
            btnAfficherInfos = new Button();
            btnAfficherInfos.Text = "Afficher Infos Graphe";
            btnAfficherInfos.Location = new Point(10, 10);
            btnAfficherInfos.Click += new EventHandler(BoutonClique);
            return btnAfficherInfos;
        }

        /// <summary>
        /// Affiche des informations sur le graphe lorsqu'on clique sur le bouton
        /// </summary>
        private void BoutonClique(object sender, EventArgs e)
        {
            string message = "Liste d'adjacence: " + graphe.AfficherListeAdjacence();
            message += "\n\nVérification des connexions:";
            message += $"\nAlice est connectée à Bob ? {graphe.ExisteLien(0, 1)}";
            message += $"\nAlice est connectée à David ? {graphe.ExisteLien(0, 3)}";
            message += "\n\nMatrice d'adjacence:" + graphe.AfficherMatriceAdjacence();
            message += "\nParcours en largeur (BFS) depuis Alice:" + graphe.ParcoursLargeur(0);
            message += "\nParcours en profondeur (DFS) depuis Alice: " + graphe.ParcoursProfondeur(0);
            message += $"\nLe graphe est connexe ? {graphe.EstConnexe()}";
            message += $"\nLe graphe contient un cycle ? {graphe.ContientCycle()}";

            MessageBox.Show(message, "Informations sur le Graphe", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Dessine le graphe sur la fenetre
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
                g.DrawString(noeud.Nom, new Font("Arial", 10), Brushes.Black, pos.X - rayon / 2, pos.Y - rayon / 2);
            }
        }

        /// <summary>
        /// Gere le clic de la souris pour afficher les informations d'un nœud
        /// </summary>
        private void SourisCliquee(object sender, MouseEventArgs e)
        {
            foreach (var noeud in graphe.Noeuds.Values)
            {
                Point pos = positions[noeud.Id];
                Rectangle zone = new Rectangle(pos.X - rayon, pos.Y - rayon, 2 * rayon, 2 * rayon);

                if (zone.Contains(e.Location))
                {
                    string message = $"ID: {noeud.Id}\nNom: {noeud.Nom}\nLiens: ";
                    message += string.Join(", ", graphe.Liens.Where(l => l.Noeud1.Id == noeud.Id || l.Noeud2.Id == noeud.Id)
                                                              .Select(l => l.Noeud1.Id == noeud.Id ? l.Noeud2.Id : l.Noeud1.Id));
                    MessageBox.Show(message, "Informations sur le sommet", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                }
            }
        }
    }
}
