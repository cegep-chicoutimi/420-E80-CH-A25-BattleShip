using BattleShipLibrary;
using System;
using System.Net;
using System.Net.Sockets;

namespace BattleShip_server
{
    internal class Program
    {
        private const int PORT = 22222;
        private static bool _rematch = true;

        static ConsoleColor couleurJoueur;
        static ConsoleColor couleurServeur;
        static int nbTotalCases = 11;

        static void Main()
        {
            StartServer();
        }

        #region Serveur
        private static void StartServer()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Any;
                IPEndPoint localEP = new IPEndPoint(ipAddress, PORT);

                using Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEP);
                listener.Listen(1);

                (couleurServeur, couleurJoueur) = ConfigManager.LoadOrChooseColors(); // inverse par rapport au client

                while (true)
                {
                    ConsoleUI.WriteWaiting($"Serveur en attente de connexion sur le port {PORT}...");
                    using Socket socket = listener.Accept();
                    ConsoleUI.WriteSuccessful("Client connecté !");

                    SocketHelper network = new SocketHelper(socket);

                    _rematch = true;
                    JouerPartie(network);

                    ConsoleUI.WriteWaiting("Client déconnecté. En attente d'un nouveau client...");
                }
            }
            catch (Exception e)
            {
                ConsoleUI.WriteWarning($"Erreur serveur : {e.Message}");
            }
        }

        #endregion

        #region Partie
        private static void JouerPartie(SocketHelper network)
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

                string tailleSerialized = network.Receive();
                var tailles = partie.DeserializeStringData(tailleSerialized).Split('/');
                int colonnes = int.Parse(tailles[0]);
                int lignes = int.Parse(tailles[1]);

                partie.StartGame(colonnes, lignes);

                bool win = false;
                bool monTour = false; // serveur commence après le client

                while (!win)
                {
                    if (monTour)
                    {
                        bool continuer = true;
                        while (continuer && !win)
                        {
                            string coords = ConsoleUI.AskForCoordinate(partie, "Choisissez la case à toucher : ");
                            network.Send(partie.SerializeData(coords));

                            bool touche = partie.DeserializeBoolData(network.Receive());
                            var (col, row) = partie.Positions[coords];
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

                            bool clientATouche = partie.DeserializeBoolData(resultSerialized);

                            string confirmation = partie.DeserializeStringData(network.Receive());
                            if (confirmation != "1")
                            {
                                win = true;
                                ConsoleUI.WriteWarning("Vous avez perdu !");
                                break;
                            }

                            if (!clientATouche)
                            {
                                advContinuer = false;
                                monTour = true;
                            }
                        }
                    }
                }

                network.Send(partie.SerializeData("Voulez-vous une revanche (o/n)"));

                // On attend la réponse du client
                string reponseClient = partie.DeserializeStringData(network.Receive());

                // Si le client dit oui, on rejoue
                if (reponseClient == "o") _rematch = true;
                else _rematch = false;

                Console.Clear();
            }
        }
        #endregion
    }
}
