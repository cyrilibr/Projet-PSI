using System;
using Projet_PSI.Modules;

namespace Projet_PSI
{
    /// <summary>
    /// Module principal de l'application qui gère le menu général et la navigation entre les modules.
    /// </summary>
    public static class ModulePrincipal
    {
        /// <summary>
        /// Lance le menu principal et redirige vers les différents modules selon le choix de l'utilisateur.
        /// </summary>
        /// <param name="graphe">Le graphe des stations utilisé pour les trajets de livraison.</param>
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
                Console.WriteLine("6. Exporter les données");
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
                    case "6": ModuleExport.Lancer(); break;
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
