using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;

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
            // Vérifie récursivement si le type T ou ses propriétés contiennent un IDictionary
            bool ContientDictionnaire(Type type)
            {
                if (typeof(IDictionary).IsAssignableFrom(type) ||
                    (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
                    return true;

                foreach (var prop in type.GetProperties())
                {
                    if (ContientDictionnaire(prop.PropertyType))
                        return true;
                }

                return false;
            }

            if (ContientDictionnaire(typeof(T)))
            {
                Console.WriteLine("Erreur : le type contient un dictionnaire, XmlSerializer ne peut pas le sérialiser.");
                return;
            }

            var serializer = new XmlSerializer(typeof(List<T>));
            using var writer = new StreamWriter(chemin);
            serializer.Serialize(writer, objets);
            Console.WriteLine($"Export XML terminé : {chemin}");
        }
    }
}
