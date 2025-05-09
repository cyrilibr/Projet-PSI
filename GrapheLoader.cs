using Projet_PSI.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Projet_PSI
{
    /// <summary>
    /// Charge les données de la table stations_metro et construit un graphe unifié de stations.
    /// </summary>
    public static class GrapheLoader
    {
        /// <summary>
        /// Charge les stations depuis la base de données, unifie les stations physiques
        /// de même nom et crée les nœuds et arêtes correspondants dans un graphe.
        /// </summary>
        /// <returns>Un graphe de stations prêtes pour le calcul de chemin.</returns>
        public static Graphe<Station> ChargerDepuisBDD()
        {
            var listeBrute = new List<DonneeStationBrute>();
            using (var lecteur = Bdd.Lire(@"
                SELECT IDStation, LibelleStation, Latitude, Longitude,
                       CommuneNom, CommuneCode, Precedent, Suivant,
                       TempsChangement, EstOriente, LibelLigne
                  FROM stations_metro"))
            {
                while (lecteur.Read())
                {
                    listeBrute.Add(new DonneeStationBrute
                    {
                        Id = lecteur.GetInt32("IDStation"),
                        Nom = lecteur.GetString("LibelleStation"),
                        Latitude = lecteur.GetDouble("Latitude"),
                        Longitude = lecteur.GetDouble("Longitude"),
                        NomCommune = lecteur.GetString("CommuneNom"),
                        CodeCommune = lecteur.GetInt32("CommuneCode"),
                        Precedent = lecteur.IsDBNull("Precedent") ? (int?)null : lecteur.GetInt32("Precedent"),
                        Suivant = lecteur.IsDBNull("Suivant") ? (int?)null : lecteur.GetInt32("Suivant"),
                        TempsChangement = lecteur.GetInt32("TempsChangement"),
                        EstOriente = lecteur.GetBoolean("EstOriente"),
                        NomLigne = lecteur.GetString("LibelLigne")
                    });
                }
            }

            var groupes = listeBrute.GroupBy(s => s.Nom).ToList();
            int nStationsUnifiees = groupes.Count;
            var graphe = new Graphe<Station>(nStationsUnifiees);
            var mapIdBddVersUnifie = new Dictionary<int, int>(listeBrute.Count);

            for (int idUnifie = 0; idUnifie < nStationsUnifiees; idUnifie++)
            {
                var premiereEntree = groupes[idUnifie].First();
                var stationUnifiee = new Station
                {
                    ID = idUnifie,
                    Libelle = premiereEntree.Nom,
                    Latitude = premiereEntree.Latitude,
                    Longitude = premiereEntree.Longitude,
                    CommuneNom = premiereEntree.NomCommune,
                    CommuneCode = premiereEntree.CodeCommune,
                    LibelLigne = premiereEntree.NomLigne,
                    Precedent = -1,
                    Suivant = -1,
                    TempsChangement = premiereEntree.TempsChangement,
                    EstOriente = false
                };
                graphe.AjouterNoeud(idUnifie, stationUnifiee);

                foreach (var raw in groupes[idUnifie])
                {
                    mapIdBddVersUnifie[raw.Id] = idUnifie;
                }
            }


            foreach (var raw in listeBrute)
            {
                if (!mapIdBddVersUnifie.TryGetValue(raw.Id, out int u))
                    continue;

                int poidsMinutes = raw.TempsChangement;

                if (raw.Precedent.HasValue &&
                    mapIdBddVersUnifie.TryGetValue(raw.Precedent.Value, out int vPrev) &&
                    u != vPrev)
                {
                    graphe.AjouterLien(vPrev, u, poidsMinutes);
                }

                if (raw.Suivant.HasValue &&
                    mapIdBddVersUnifie.TryGetValue(raw.Suivant.Value, out int vNext) &&
                    u != vNext)
                {
                    graphe.AjouterLien(u, vNext, poidsMinutes);
                }
            }

            return graphe;
        }

        /// <summary>
        /// Classe temporaire pour stocker les données brutes extraites de la base.
        /// </summary>
        private class DonneeStationBrute
        {
            /// <summary>Identifiant original en base.</summary>
            public int Id { get; set; }
            /// <summary>Libellé de la station.</summary>
            public string Nom { get; set; }
            /// <summary>Coordonnée latitude.</summary>
            public double Latitude { get; set; }
            /// <summary>Coordonnée longitude.</summary>
            public double Longitude { get; set; }
            /// <summary>Nom de la commune.</summary>
            public string NomCommune { get; set; }
            /// <summary>Code INSEE de la commune.</summary>
            public int CodeCommune { get; set; }
            /// <summary>ID BDD de la station précédente (null si terminus).</summary>
            public int? Precedent { get; set; }
            /// <summary>ID BDD de la station suivante (null si terminus).</summary>
            public int? Suivant { get; set; }
            /// <summary>Temps de correspondance (en secondes).</summary>
            public int TempsChangement { get; set; }
            /// <summary>Orientation (non utilisé sur le graphe unifié).</summary>
            public bool EstOriente { get; set; }
            /// <summary>Nom de la ligne (ex: "Ligne 1").</summary>
            public string NomLigne { get; set; }
        }
    }
}
