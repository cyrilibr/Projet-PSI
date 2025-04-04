using System;
using Projet_PSI.Modules;

namespace Projet_PSI
{
    public static class ModulePrincipal
    {
        public static void Lancer(Graphe<Station> graphe)
        {
            bool quitter = false;
            while (!quitter)
            {
                Console.Clear();
                Console.WriteLine("=== MENU PRINCIPAL ===");
                Console.WriteLine("1. Gestion des clients");
                Console.WriteLine("2. Gestion des cuisiniers");
                Console.WriteLine("3. Gestion des commandes");
                Console.WriteLine("4. Chemin de livraison");
                Console.WriteLine("5. Statistiques générales");
                Console.WriteLine("0. Quitter");
                Console.Write("Choix : ");
                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1": ModuleClient.Lancer(); break;
                    case "2": ModuleCuisinier.Lancer(); break;
                    case "3": ModuleCommande.Lancer(); break;
                    case "4": ModuleChemin.Lancer(graphe); break;
                    case "5": ModuleStatistiques.Lancer(); break;
                    case "0": quitter = true; break;
                    default:
                        Console.WriteLine("Choix invalide. Appuyez sur une touche pour réessayer.");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
}