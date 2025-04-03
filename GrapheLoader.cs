using MySql.Data.MySqlClient;
using Projet_PSI.Utils;

namespace Projet_PSI
{
    public static class GrapheLoader
    {
        public static Graphe<Station> ChargerDepuisBDD()
        {
            Graphe<Station> graphe = new Graphe<Station>();
            Dictionary<int, Station> stations = new Dictionary<int, Station>();

            using (var reader = Bdd.Lire("SELECT IDStation, LibelleStation, Latitude, Longitude FROM stations_metro"))
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32("IDStation");
                    string libelle = reader.GetString("LibelleStation");
                    double lat = reader.GetDouble("Latitude");
                    double lon = reader.GetDouble("Longitude");

                    Station s = new Station { ID = id, Libelle = libelle, Latitude = lat, Longitude = lon };
                    stations[id] = s;
                    graphe.AjouterNoeud(new Noeud<Station>(id, s));
                }
                reader.Close();
            }

            using (var reader = Bdd.Lire("SELECT IDStation, Precedent, Suivant, TempsChangement FROM stations_metro"))
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32("IDStation");
                    int? prec = reader.IsDBNull(reader.GetOrdinal("Precedent")) ? null : reader.GetInt32("Precedent");
                    int? suiv = reader.IsDBNull(reader.GetOrdinal("Suivant")) ? null : reader.GetInt32("Suivant");
                    int poids = reader.IsDBNull(reader.GetOrdinal("TempsChangement")) ? 1 : reader.GetInt32("TempsChangement");

                    if (prec.HasValue && stations.ContainsKey(prec.Value))
                        graphe.AjouterLien(prec.Value, id, poids);

                    if (suiv.HasValue && stations.ContainsKey(suiv.Value))
                        graphe.AjouterLien(id, suiv.Value, poids);
                }
                reader.Close();
            }

            return graphe;
        }
    }
}
