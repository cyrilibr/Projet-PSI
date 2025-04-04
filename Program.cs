using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace Projet_PSI
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Choisissez le mode d'affichage :");
            Console.WriteLine("1. Interface Console");
            Console.WriteLine("2. Interface Graphique");
            Console.Write("Votre choix : ");
            string choix = Console.ReadLine();

            var graphe = GrapheLoader.ChargerDepuisBDD(); // C'est un Graphe<Station>

            if (choix == "1")
            {
                ModulePrincipal.Lancer(graphe);
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
