using System;
using System.Windows.Forms;

namespace Projet_PSI
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var graphe = GrapheLoader.ChargerDepuisBDD();

            Console.WriteLine("Choisissez le mode d'affichage :");
            Console.WriteLine("1. Interface Console (avec connexion)");
            Console.WriteLine("2. Interface Graphique");
            Console.Write("Votre choix : ");
            string choix = Console.ReadLine();

            if (choix == "1")
            {
                Modules.ModuleAuthentification.Lancer(graphe);
            }
            else if (choix == "2")
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var grapheStations = GrapheLoader.ChargerDepuisBDD();
                Console.WriteLine($"Graphe chargé avec {grapheStations.Noeuds.Count} stations unifiées et {grapheStations.Liens.Count} liens orientés.");

                Form fenetre = new FenetreGraphe<Station>(grapheStations);
                Application.Run(fenetre);
            }
            else
            {
                Console.WriteLine("Choix invalide. Fin du programme.");
            }
        }
    }
}
