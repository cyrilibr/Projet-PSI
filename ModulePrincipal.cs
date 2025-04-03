using System;
using Projet_PSI.Modules;

namespace Projet_PSI
{
    public static class MenuPrincipal
    {
        public static void Lancer()
        {
            bool quitter = false;
            while (!quitter)
            {
                Console.Clear();
                Console.WriteLine("=== MENU PRINCIPAL ===");
                Console.WriteLine("1. Module Client");
                Console.WriteLine("2. Module Cuisinier");
                Console.WriteLine("3. Module Commande");
                Console.WriteLine("4. Module Chemin");
                Console.WriteLine("5. Module Statistiques");
                Console.WriteLine("0. Quitter");
                Console.Write("Choix : ");
                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1": ModuleClient.Lancer(); break;
                    case "2": ModuleCuisinier.Lancer(); break;
                    case "3": ModuleCommande.Lancer(); break;
                    case "4": ModuleChemin.Lancer(); break;
                    case "5": ModuleStatistiques.Lancer(); break;
                    case "0": quitter = true; break;
                    default: Console.WriteLine("Choix invalide."); break;
                }

                if (!quitter)
                {
                    Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                    Console.ReadKey();
                }
            }

            Console.WriteLine("\nMerci d'avoir utilisé l'application. Au revoir !");
        }
    }
}