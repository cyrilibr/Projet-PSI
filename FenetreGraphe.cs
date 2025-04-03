using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Projet_PSI
{
    public class FenetreGraphe<T> : Form
    {
        private Graphe<T> graphe;//Instance du graphe générique à afficher
        private Dictionary<int, Point> positions;//Dictionnaire des positions aléatoires des nœuds
        private int rayon = 20;//Rayon des cercles représentant les nœuds

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
            btnBFS = CreerBouton("BFS (depuis 0)", BoutonBFS_Click);
            btnDFS = CreerBouton("DFS (depuis 0)", BoutonDFS_Click);
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

        private void BoutonBFS_Click(object sender, EventArgs e)
        {
            string info = "Parcours BFS (depuis le nœud 1) :\r\n" + graphe.ParcoursLargeur(1);
            AfficherEnPleinEcran("BFS", info);
        }

        private void BoutonDFS_Click(object sender, EventArgs e)
        {
            string info = "Parcours DFS (depuis le nœud 1) :\r\n" + graphe.ParcoursProfondeur(1);
            AfficherEnPleinEcran("DFS", info);
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
    }
}
