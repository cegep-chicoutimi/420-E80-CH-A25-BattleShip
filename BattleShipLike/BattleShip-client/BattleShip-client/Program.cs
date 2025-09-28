using BattleShipLibrary;
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Linq;

namespace BattleShip_client
{
    internal class Program
    {
        private const int PORT = 22222;
        private static bool _adresseValide = false;
        private static string _adresseIp = "0.0.0.0";
        private static bool _rematch = true;

        static ConsoleColor couleurJoueur;
        static ConsoleColor couleurServeur;

        static int nbTotalCases = 11;

        static void Main(string[] args)
        {
            DemandeInfoServeur();
        }

        #region Connexion Réseau
        public static void DemandeInfoServeur()
        {
            // Demande de l'adresse du serveur
            do
            {
                Console.Write("Entrer l'adresse où vous connecter : ");
                string saisie = Console.ReadLine() ?? "";

                if (!ValiderAdresseIp(saisie))
                    ConsoleUI.WriteWarning("Adresse IP invalide.");
                else
                    _adresseValide = true;

            } while (!_adresseValide);

            Console.Clear();
            ConsoleUI.WriteWaiting($"Connexion à l'adresse {_adresseIp} en cours...");

            // Demande si le joueur veut jouer contre l'IA
            bool jouerContreIA = false;
            string choixIA;
            do
            {
                Console.Write("Voulez-vous jouer contre l'ordinateur (IA) ? (o/n) : ");
                choixIA = Console.ReadLine()?.Trim().ToLower() ?? "";
                if (choixIA != "o" && choixIA != "n")
                    ConsoleUI.WriteWarning("Entrez 'o' pour oui ou 'n' pour non !");
            } while (choixIA != "o" && choixIA != "n");

            jouerContreIA = choixIA == "o";


            try
            {
                IPAddress ipAddress = IPAddress.Parse(_adresseIp);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, PORT);

                using Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sender.Connect(remoteEP);

                ConsoleUI.WriteSuccessful("Connecté au serveur !");
                (couleurJoueur, couleurServeur) = ConfigManager.LoadOrChooseColors();

                SocketHelper socketHelper = new SocketHelper(sender);

                // Envoyer le booléen au serveur : "True" = IA, "False" = humain
                socketHelper.Send(jouerContreIA.ToString());

                JouerPartie(socketHelper);
            }
            catch (Exception e)
            {
                ConsoleUI.WriteWarning($"Erreur de connexion : {e.Message}");
            }
        }


        public static bool ValiderAdresseIp(string adresse)
        {
            if (string.IsNullOrWhiteSpace(adresse)) return false;

            string[] splitValues = adresse.Split('.');
            if (splitValues.Length != 4) return false;

            bool valide = splitValues.All(part => byte.TryParse(part, out _));
            if (valide) _adresseIp = adresse;

            return valide;
        }
        #endregion

        #region Partie
        public static void JouerPartie(SocketHelper network)
        {
            while (_rematch)
            {
                _rematch = false;
                int hits = 0;

                BattleShip partie = new BattleShip
                {
                    CouleurJoueur = couleurJoueur,
                    CouleurServeur = couleurServeur
                };

                int colonnes = ConsoleUI.SaisieTaille("Nombre de colonnes ?");
                int lignes = ConsoleUI.SaisieTaille("Nombre de lignes ?");
                network.Send(partie.SerializeData($"{colonnes}/{lignes}"));

                partie.StartGame(colonnes, lignes);
                bool win = false;
                bool monTour = true;

                while (!win)
                {
                    if (monTour)
                    {
                        bool continuer = true;
                        while (continuer && !win)
                        {
                            string coords = partie.ChoisirCase("Choisissez la case à toucher : ", false);
                            network.Send(partie.SerializeData(coords));

                            bool touche = partie.DeserializeBoolData(network.Receive());
                            var (col, row) = partie.Positions[coords];
                            partie.EnemyGrille.Cells[col, row] = touche ? "B" : "X"; // B = touché, X = manqué

                            Console.Clear();

                            if (touche)
                            {
                                hits++;
                                ConsoleUI.WriteSuccessful("Bateau touché ! Vous rejouez !");
                            }
                            else
                            {
                                ConsoleUI.WriteWaiting("Bateau non touché. À l’adversaire !");
                                continuer = false;
                            }

                            partie.AfficherMaGrille();
                            partie.AfficherEnemyGrille();
                            Console.WriteLine($"Vous avez touché {hits} fois / {nbTotalCases}");

                            network.Send(partie.SerializeData(hits == nbTotalCases ? "0" : "1"));

                            if (hits == nbTotalCases)
                            {
                                ConsoleUI.WriteSuccessful("Vous avez gagné !");
                                win = true;
                                break;
                            }
                        }
                        monTour = !monTour;
                    }
                    else
                    {
                        bool advContinuer = true;
                        while (advContinuer && !win)
                        {
                            string coordRecus = partie.DeserializeStringData(network.Receive());
                            string resultSerialized = partie.IsTouched(coordRecus);
                            network.Send(resultSerialized);

                            bool serveurATouche = partie.DeserializeBoolData(resultSerialized);

                            string confirmation = partie.DeserializeStringData(network.Receive());
                            if (confirmation != "1")
                            {
                                win = true;
                                ConsoleUI.WriteWarning("Vous avez perdu !");
                                break;
                            }

                            if (!serveurATouche)
                            {
                                advContinuer = false;
                                monTour = true;
                            }
                        }
                    }
                }

                // Réception de la question du serveur
                string questionRematch = partie.DeserializeStringData(network.Receive());
                string r;
                do
                {
                    Console.WriteLine(questionRematch);
                    r = Console.ReadLine();
                    if (r != "o" && r != "n") ConsoleUI.WriteWarning("Entrez une réponse valide !");
                } while (r != "o" && r != "n");

                // Envoi de la réponse au serveur
                network.Send(partie.SerializeData(r));
                if (r == "o") _rematch = true;


                Console.Clear();
            }
        }
        #endregion
    }
}