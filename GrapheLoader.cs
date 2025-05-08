using System;
using System.Collections.Generic;
// Nous n'avons pas besoin de System.Linq pour cette version adaptée aux débutants.
using Projet_PSI.Utils; // Ceci est pour Bdd.Lire afin d'accéder à la base de données.

namespace Projet_PSI
{
    public static class GrapheLoader
    {
        // Ceci est une classe auxiliaire pour stocker temporairement les données brutes
        // que nous lisons depuis la base de données pour chaque enregistrement de station.
        // Chaque ligne de la table 'stations_metro' de la base de données deviendra un de ces objets.
        private class DonneesStationBrutes // Équivalent de RawStationData
        {
            public int IDStation { get; set; } // ID original de la base de données
            public string LibelleStation { get; set; } // Nom de la station, ex: "Châtelet"
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string CommuneNom { get; set; }
            public int CommuneCode { get; set; }
            public int Precedent { get; set; } // ID original de la station précédente sur la même ligne
            public int Suivant { get; set; }   // ID original de la station suivante sur la même ligne
            public int TempsChangement { get; set; }
            public bool EstOriente { get; set; }
            public string LibelLigne { get; set; } // Nom de la ligne de métro, ex: "Ligne 1"
        }

        public static Graphe<Station> ChargerDepuisBDD()
        {
            // ÉTAPE 1 : Lire tous les enregistrements de stations depuis la base de données.
            // Nous allons les stocker dans une liste de nos objets auxiliaires DonneesStationBrutes.
            var toutesLesStationsDeLaBDD = new List<DonneesStationBrutes>();

            // Utiliser Bdd.Lire pour exécuter une requête SQL. Cela nécessite que votre classe Bdd soit configurée.
            using (var lecteur = Bdd.Lire(@"
                SELECT IDStation, LibelleStation, Latitude, Longitude, CommuneNom, CommuneCode, Precedent, Suivant, TempsChangement, EstOriente, LibelLigne
                FROM stations_metro"))
            {
                // Le 'lecteur' nous permet de parcourir chaque ligne retournée par la requête SQL.
                while (lecteur.Read()) // Read() retourne true s'il y a une autre ligne, false sinon.
                {
                    // Créer un nouvel objet DonneesStationBrutes pour la ligne actuelle.
                    var stationBrute = new DonneesStationBrutes();

                    stationBrute.IDStation = lecteur.GetInt32("IDStation");
                    stationBrute.LibelleStation = lecteur.GetString("LibelleStation");
                    stationBrute.Latitude = lecteur.GetDouble("Latitude");
                    stationBrute.Longitude = lecteur.GetDouble("Longitude");
                    stationBrute.CommuneNom = lecteur.GetString("CommuneNom");
                    stationBrute.CommuneCode = lecteur.GetInt32("CommuneCode");

                    // 'Precedent' et 'Suivant' peuvent être NULL dans la base de données s'il s'agit d'un terminus.
                    // Nous devons vérifier s'ils sont NULL avant d'essayer de les lire.
                    // GetOrdinal récupère la position de la colonne. IsDBNull vérifie si la valeur à cette position est NULL.
                    if (lecteur.IsDBNull(lecteur.GetOrdinal("Precedent")))
                    {
                        stationBrute.Precedent = -1; // Utiliser -1 pour signifier 'pas de station précédente'.
                    }
                    else
                    {
                        stationBrute.Precedent = lecteur.GetInt32("Precedent");
                    }

                    if (lecteur.IsDBNull(lecteur.GetOrdinal("Suivant")))
                    {
                        stationBrute.Suivant = -1; // Utiliser -1 pour signifier 'pas de station suivante'.
                    }
                    else
                    {
                        stationBrute.Suivant = lecteur.GetInt32("Suivant");
                    }

                    stationBrute.TempsChangement = lecteur.GetInt32("TempsChangement");
                    stationBrute.EstOriente = lecteur.GetBoolean("EstOriente");
                    stationBrute.LibelLigne = lecteur.GetString("LibelLigne");

                    // Ajouter l'objet nouvellement rempli à notre liste.
                    toutesLesStationsDeLaBDD.Add(stationBrute);
                }
                lecteur.Close(); // Important de fermer le lecteur une fois terminé.
            }

            // ÉTAPE 2 : Unifier les stations.
            // Certaines stations physiques (comme "Châtelet") peuvent avoir plusieurs entrées dans la base de données
            // (par exemple, une pour la Ligne 1, une pour la Ligne 4, etc.). Nous voulons représenter chaque
            // station physique comme UN SEUL nœud dans notre graphe.

            // Ce dictionnaire va mapper un nom de station (chaîne, ex: "Châtelet")
            // à un nouvel ID unique (entier, ex: 0) que nous utiliserons dans notre graphe.
            var nomStationVersIdUnifieGrapheMap = new Dictionary<string, int>();

            // Ce dictionnaire va mapper un ID original de la base de données (entier, ex: IDStation 15 pour "Châtelet Ligne 1")
            // au nouvel ID de graphe unifié (entier, ex: 0 pour "Châtelet").
            var idBDDOriginalVersIdUnifieGrapheMap = new Dictionary<int, int>();

            // Ce dictionnaire stockera les objets 'Station' réels qui iront dans notre graphe.
            // La clé sera le nouvel ID de graphe unifié (entier), et la valeur sera l'objet 'Station'.
            var donneesNoeudsUnifiesGraphe = new Dictionary<int, Station>();

            int prochainIdUnifieGrapheDisponible = 0; // Sera utilisé pour assigner de nouveaux IDs : 0, 1, 2, ...

            // Parcourir chaque enregistrement de station que nous avons lu de la base de données.
            foreach (var donneesStationActuelleBDD in toutesLesStationsDeLaBDD)
            {
                string nomDeLaStation = donneesStationActuelleBDD.LibelleStation;
                int idUnifieGraphePourCeNom;

                // Vérifier si nous avons déjà traité une station avec ce nom.
                if (nomStationVersIdUnifieGrapheMap.ContainsKey(nomDeLaStation))
                {
                    // Oui, nous avons déjà vu ce nom. Obtenir son ID de graphe unifié assigné.
                    idUnifieGraphePourCeNom = nomStationVersIdUnifieGrapheMap[nomDeLaStation];
                }
                else
                {
                    // Non, c'est la première fois que nous voyons ce nom de station.
                    // Nous devons :
                    // 1. Lui assigner un nouvel ID de graphe unifié.
                    // 2. Créer un objet 'Station' pour ce nouveau nœud unifié.
                    // 3. Stocker le mappage de ce nom vers le nouvel ID.

                    idUnifieGraphePourCeNom = prochainIdUnifieGrapheDisponible;
                    prochainIdUnifieGrapheDisponible++; // Incrémenter pour la prochaine nouvelle station unique.

                    // Stocker le mappage du nom de la station vers ce nouvel ID unifié.
                    nomStationVersIdUnifieGrapheMap[nomDeLaStation] = idUnifieGraphePourCeNom;

                    // Créer l'objet 'Station' pour notre graphe.
                    // Nous utiliserons les coordonnées et les informations de la commune de la première entrée de base de données
                    // que nous rencontrons pour ce nom.
                    // L''ID' de cet objet Station sera notre nouvel idUnifieGraphePourCeNom.
                    var stationPourNoeudDuGraphe = new Station
                    {
                        ID = idUnifieGraphePourCeNom, // Ceci est l'ID pour le nœud du graphe.
                        Libelle = nomDeLaStation,
                        Latitude = donneesStationActuelleBDD.Latitude,
                        Longitude = donneesStationActuelleBDD.Longitude,
                        CommuneNom = donneesStationActuelleBDD.CommuneNom,
                        CommuneCode = donneesStationActuelleBDD.CommuneCode,
                        LibelLigne = donneesStationActuelleBDD.LibelLigne, // Nous prenons la ligne de cette entrée BDD spécifique.
                                                                           // Si une station unifiée doit représenter plusieurs lignes,
                                                                           // la classe 'Station' et cette logique nécessiteraient un ajustement.
                        Precedent = -1, // Ces champs dans Station sont pour la structure interne du graphe,
                        Suivant = -1,   // pas directement utilisés par GrapheLoader dans cette configuration simplifiée.
                        TempsChangement = donneesStationActuelleBDD.TempsChangement, // Pourrait être une moyenne ou une valeur spécifique.
                        EstOriente = false // Normalement, les stations unifiées ne sont pas intrinsèquement "orientées".
                    };

                    // Ajouter ce nouvel objet 'Station' à notre collection de nœuds de graphe.
                    donneesNoeudsUnifiesGraphe[idUnifieGraphePourCeNom] = stationPourNoeudDuGraphe;
                }

                // Maintenant, mapper l'ID original de la base de données de la donneesStationActuelleBDD
                // à l'idUnifieGraphePourCeNom que nous venons de trouver ou de créer.
                // Cela nous aidera plus tard lorsque nous établirons les connexions (liens/arêtes).
                idBDDOriginalVersIdUnifieGrapheMap[donneesStationActuelleBDD.IDStation] = idUnifieGraphePourCeNom;
            }

            // ÉTAPE 3 : Créer le graphe et y ajouter les nœuds unifiés.
            // La taille du graphe pour la matrice d'adjacence sera le nombre de stations unifiées uniques.
            int nombreDeStationsUniques = prochainIdUnifieGrapheDisponible;
            var graphe = new Graphe<Station>(nombreDeStationsUniques);

            // Itérer à travers notre collection d'objets 'Station' unifiés et les ajouter au graphe.
            // La Clé de `donneesNoeudsUnifiesGraphe` est l'ID unifié, la Valeur est l'objet Station.
            foreach (KeyValuePair<int, Station> paireCleValeur in donneesNoeudsUnifiesGraphe)
            {
                int idNoeudUnifie = paireCleValeur.Key;
                Station donneesStationPourLeNoeud = paireCleValeur.Value;
                graphe.AjouterNoeud(idNoeudUnifie, donneesStationPourLeNoeud);
            }

            // ÉTAPE 4 : Ajouter des liens (arêtes) entre les nœuds unifiés dans le graphe.
            // Nous allons à nouveau parcourir toutes les stations originales de la base de données.
            // Les champs 'Precedent' et 'Suivant' nous indiquent les connexions.
            foreach (var donneesStationActuelleBDD in toutesLesStationsDeLaBDD)
            {
                // Obtenir l'ID de graphe unifié pour la station actuelle que nous examinons.
                // Par exemple, si donneesStationActuelleBDD est "Châtelet Ligne 1 (ID 15 de la BDD)",
                // idUnifiePourStationCourante sera son ID de graphe unifié (ex: 0).
                int idUnifiePourStationCourante = idBDDOriginalVersIdUnifieGrapheMap[donneesStationActuelleBDD.IDStation];

                // ---- Gérer la station 'Precedent' ----
                int idBaseDeDonneesDuPrecedent = donneesStationActuelleBDD.Precedent;
                if (idBaseDeDonneesDuPrecedent != -1) // -1 signifie pas de station précédente.
                {
                    // Nous avons besoin de l'ID de graphe unifié de cette station précédente.
                    // Le rechercher dans notre map : idBDDOriginalVersIdUnifieGrapheMap.
                    if (idBDDOriginalVersIdUnifieGrapheMap.ContainsKey(idBaseDeDonneesDuPrecedent))
                    {
                        int idUnifiePourStationPrecedente = idBDDOriginalVersIdUnifieGrapheMap[idBaseDeDonneesDuPrecedent];

                        // Ajouter un lien de la station précédente vers la station actuelle.
                        // Nous n'ajoutons un lien que s'ils sont réellement des nœuds unifiés différents
                        // (pour éviter qu'une station ne soit liée à elle-même si les données étaient structurées ainsi).
                        if (idUnifiePourStationPrecedente != idUnifiePourStationCourante)
                        {
                            // Poids par défaut de 1. Vous pourriez utiliser 'TempsChangement' ou une autre valeur ici.
                            graphe.AjouterLien(idUnifiePourStationPrecedente, idUnifiePourStationCourante, poids: 1);
                        }
                    }
                    // Else : L'ID Precedent de la base de données n'a pas été trouvé dans notre map. Cela pourrait indiquer un problème de données.
                }

                // ---- Gérer la station 'Suivant' ----
                int idBaseDeDonneesDuSuivant = donneesStationActuelleBDD.Suivant;
                if (idBaseDeDonneesDuSuivant != -1) // -1 signifie pas de station suivante.
                {
                    // Obtenir l'ID de graphe unifié de cette station suivante.
                    if (idBDDOriginalVersIdUnifieGrapheMap.ContainsKey(idBaseDeDonneesDuSuivant))
                    {
                        int idUnifiePourStationSuivante = idBDDOriginalVersIdUnifieGrapheMap[idBaseDeDonneesDuSuivant];

                        // Ajouter un lien de la station actuelle vers la station suivante.
                        if (idUnifiePourStationCourante != idUnifiePourStationSuivante)
                        {
                            graphe.AjouterLien(idUnifiePourStationCourante, idUnifiePourStationSuivante, poids: 1);
                        }
                    }
                    // Else : L'ID Suivant de la base de données n'a pas été trouvé. Problème de données ?
                }
            }

            // Le graphe est maintenant chargé avec les stations unifiées et leurs connexions.
            return graphe;
        }
    }
}