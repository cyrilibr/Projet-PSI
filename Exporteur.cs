using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;

namespace Projet_PSI.Modules
{
    /// <summary>
    /// Fournit des méthodes génériques pour exporter des listes d'objets
    /// au format JSON ou XML.
    /// </summary>
    public static class Exporteur
    {
        /// <summary>
        /// Sérialise une liste d'objets au format JSON et écrit le résultat dans un fichier.
        /// </summary>
        /// <typeparam name="T">Type des objets à sérialiser.</typeparam>
        /// <param name="objets">Liste d'objets à exporter.</param>
        /// <param name="chemin">Chemin du fichier de sortie.</param>
        public static void ExporterJson<T>(List<T> objets, string chemin)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(objets, options);
            File.WriteAllText(chemin, json);
            Console.WriteLine($"Export JSON terminé : {chemin}");
        }

        /// <summary>
        /// Sérialise une liste d'objets au format XML et écrit le résultat dans un fichier.
        /// Vérifie l'absence de dictionnaires, non pris en charge par XmlSerializer.
        /// </summary>
        /// <typeparam name="T">Type des objets à sérialiser.</typeparam>
        /// <param name="objets">Liste d'objets à exporter.</param>
        /// <param name="chemin">Chemin du fichier de sortie.</param>
        public static void ExporterXml<T>(List<T> objets, string chemin)
        {
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
