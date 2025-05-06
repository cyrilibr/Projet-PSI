using System;
using System.Collections.Generic;
using Projet_PSI.Utils;

namespace Projet_PSI.Modules
{
    public static class ModuleExport
    {
        public static void Lancer()
        {
            Console.Clear();
            Console.WriteLine("--- Export de Données ---");
            Console.WriteLine("1. Exporter Clients");
            Console.WriteLine("2. Exporter Cuisiniers");
            Console.WriteLine("3. Exporter Mets");
            Console.WriteLine("4. Exporter Commandes");
            Console.WriteLine("0. Retour");
            Console.Write("Choix : ");
            string choix = Console.ReadLine();

            switch (choix)
            {
                case "1": Exporter("Client"); break;
                case "2": Exporter("Cuisinier"); break;
                case "3": Exporter("Mets"); break;
                case "4": Exporter("CommandeLivraison"); break;
                case "0": return;
                default: Console.WriteLine("Choix invalide."); break;
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private static void Exporter(string table)
        {
            string format;
            Console.Write("Format (json/xml) : ");
            format = Console.ReadLine()?.ToLower();

            string bureau = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string chemin = Path.Combine(bureau, $"{table}_export.{format}");
            var liste = new List<Dictionary<string, object>>();

            using var reader = Bdd.Lire($"SELECT * FROM {table}");
            while (reader.Read())
            {
                var ligne = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                    ligne[reader.GetName(i)] = reader.GetValue(i);
                liste.Add(ligne);
            }
            reader.Close();

            if (format == "json")
                Exporteur.ExporterJson(liste, chemin);
            else if (format == "xml")
                Exporteur.ExporterXml(liste, chemin);
            else
                Console.WriteLine("Format non pris en charge.");
        }
    }
}
