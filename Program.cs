using System;
using System.Windows.Forms;

namespace Projet_PSI
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Chargement du graphe...");
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
                Form fenetre = new FenetreGraphe<Station>(graphe);
                Application.Run(fenetre);
            }
            else
            {
                Console.WriteLine("Choix invalide. Fin du programme.");
            }
        }
    }
}
