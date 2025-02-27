using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Projet_PSI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int n = 0;
            Graphe graphe;

            if (n == 1)
            {
                graphe = new Graphe(5);

                // Ajout des Noeuds
                graphe.AjouterNoeud(0, "Alice");
                graphe.AjouterNoeud(1, "Bob");
                graphe.AjouterNoeud(2, "Charlie");
                graphe.AjouterNoeud(3, "David");
                graphe.AjouterNoeud(4, "Emma");

                // Ajout des Liens
                graphe.AjouterLien(0, 1);
                graphe.AjouterLien(0, 2);
                graphe.AjouterLien(1, 3);
                graphe.AjouterLien(2, 4);
                graphe.AjouterLien(3, 4);
            }
            else
            {
                graphe = new Graphe(35);

                // Ajout des Noeuds
                for (int i = 0; i < 35; i++)
                {
                    graphe.AjouterNoeud(i, i.ToString());
                }

                // Ajout des Liens

                var liens = new (int, int)[] {
                (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (7, 1), (8, 1), (9, 1), (11, 1), (12, 1), (13, 1), (14, 1), (18, 1), (20, 1), (22, 1), (32, 1),
                (3, 2), (4, 2), (8, 2), (14, 2), (18, 2), (20, 2), (22, 2), (31, 2),
                (4, 3), (8, 3), (9, 3), (10, 3), (14, 3), (28, 3), (29, 3), (33, 3),
                (8, 4), (13, 4), (14, 4), (7, 5), (11, 5), (7, 6), (11, 6), (17, 6),
                (17, 7), (31, 9), (33, 9), (34, 9), (34, 10), (34, 14), (33, 15), (34, 15),
                (33, 16), (34, 16), (33, 19), (34, 19), (34, 20), (33, 21), (34, 21), (33, 23),
                (34, 23), (26, 24), (28, 24), (30, 24), (33, 24), (34, 24), (26, 25), (28, 25),
                (32, 25), (32, 26), (30, 27), (34, 27), (34, 28), (32, 29), (34, 29), (33, 30),
                (34, 30), (33, 31), (34, 31), (33, 32), (34, 32), (34, 33)
            };

                foreach (var lien in liens)
                {
                    graphe.AjouterLien(lien.Item1, lien.Item2);
                }
            }

            // Affichage de la fenêtre
            Application.EnableVisualStyles();
            Application.Run(new FenetreGraphe(graphe));
        }
    }
}
/*
2 1
3 1
4 1
5 1
6 1
7 1
8 1
9 1
11 1
12 1
13 1
14 1
18 1
20 1
22 1
32 1
3 2
4 2
8 2
14 2
18 2
20 2
22 2
31 2
4 3
8 3
9 3
10 3
14 3
28 3
29 3
33 3
8 4
13 4
14 4
7 5
11 5
7 6
11 6
17 6
17 7
31 9
33 9
34 9
34 10
34 14
33 15
34 15
33 16
34 16
33 19
34 19
34 20
33 21
34 21
33 23
34 23
26 24
28 24
30 24
33 24
34 24
26 25
28 25
32 25
32 26
30 27
34 27
34 28
32 29
34 29
33 30
34 30
33 31
34 31
33 32
34 32
34 33
*/