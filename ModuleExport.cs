using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.Json;
using Projet_PSI.Utils;

namespace Projet_PSI.Modules
{
    /// <summary>
    /// Module permettant d’exporter les données de la base au format JSON ou XML.
    /// </summary>
    public static class ModuleExport
    {
        /// <summary>
        /// Lance le menu d’exportation des différentes tables.
        /// </summary>
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

        /// <summary>
        /// Exporte les données d'une table au format spécifié (JSON ou XML).
        /// </summary>
        /// <param name="table">Nom de la table à exporter.</param>
        private static void Exporter(string table)
        {
            Console.Write("Format (json/xml) : ");
            string format = Console.ReadLine()?.ToLower();

            string bureau = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string chemin = Path.Combine(bureau, $"{table}_export.{format}");

            if (format == "json")
            {
                var liste = new List<Dictionary<string, object>>();
                using (var reader = Bdd.Lire($"SELECT * FROM {table}"))
                {
                    while (reader.Read())
                    {
                        var ligne = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                            ligne[reader.GetName(i)] = reader.GetValue(i);
                        liste.Add(ligne);
                    }
                    reader.Close();
                }

                string json = JsonSerializer.Serialize(liste, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(chemin, json);
                Console.WriteLine($"Export JSON terminé : {chemin}");
            }
            else if (format == "xml")
            {
                var dt = new DataTable(table);
                using (var reader = Bdd.Lire($"SELECT * FROM {table}"))
                {
                    dt.Load(reader);
                    reader.Close();
                }

                dt.WriteXml(chemin, XmlWriteMode.WriteSchema);
                Console.WriteLine($"Export XML terminé : {chemin}");
            }
            else
            {
                Console.WriteLine("Format non pris en charge.");
            }
        }
    }
}
