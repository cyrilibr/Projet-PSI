using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using Projet_PSI.Utils;

namespace Projet_PSI.Modules
{
    public static class Exporteur
    {
        public static void ExporterJson<T>(List<T> objets, string chemin)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(objets, options);
            File.WriteAllText(chemin, json);
            Console.WriteLine($"Export JSON terminé : {chemin}");
        }

        public static void ExporterXml<T>(List<T> objets, string chemin)
        {
            var serializer = new XmlSerializer(typeof(List<T>));
            using var writer = new StreamWriter(chemin);
            serializer.Serialize(writer, objets);
            Console.WriteLine($"Export XML terminé : {chemin}");
        }
    }
}
