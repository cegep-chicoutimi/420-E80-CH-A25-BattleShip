using BattleShipLibrary;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

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

        private static void StartServer()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Any;
                IPEndPoint localEP = new IPEndPoint(ipAddress, PORT);

                using Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEP);
                listener.Listen(1);

                (couleurServeur, couleurJoueur) = ConfigManager.LoadOrChooseColors();

                while (true)
                {
                    ConsoleUI.WriteWaiting($"Serveur en attente de connexion sur le port {PORT}...");
                    using Socket socket = listener.Accept();
                    ConsoleUI.WriteSuccessful("Client connecté !");

                    SocketHelper network = new SocketHelper(socket);

                    // Lecture du choix IA/humain
                    string choixIAString = network.Receive();
                    bool jouerContreIA = bool.Parse(choixIAString);
                    Console.WriteLine($"Client a choisi de jouer contre l'IA : {jouerContreIA}");

                    _rematch = true;
                    JouerPartie(network, jouerContreIA);

                    ConsoleUI.WriteWaiting("Client déconnecté. En attente d'un nouveau client...");
                }
            }
            catch (Exception e)
            {
                ConsoleUI.WriteWarning($"Erreur serveur : {e.Message}");
            }
        }

        private static void JouerPartie(SocketHelper network, bool jouerContreIA)
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
                bool monTour = false;

                IA ia = null;
                if (jouerContreIA)
                    ia = new IA(partie);

                while (!win)
                {
                    if (!jouerContreIA)
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
                                    monTour = false;
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
                                    monTour = true; // passe le tour au joueur
                                }
                            }
                        }
                    }
                    else
                    {
                        if (monTour)
                        {
                            // Tour IA
                            bool continuer = true;
                            while (continuer && !win)
                            {
                                // IA choisit sa case
                                string coords = ia.NextMove();
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
                                    monTour = false;
                                }

                                partie.AfficherMaGrille();
                                partie.AfficherEnemyGrille();
                                ConsoleUI.WriteWaiting($"L'IA a tiré sur {coords}");
                                Console.WriteLine($"Vous avez touché {hits} fois / {nbTotalCases}");

                                network.Send(partie.SerializeData(hits == nbTotalCases ? "0" : "1"));

                                if (hits == nbTotalCases)
                                {
                                    ConsoleUI.WriteSuccessful("Vous avez gagné !");
                                    win = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // Tour client (inchangé)
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
                                    monTour = true; // Passe le tour à l’IA
                                }
                            }
                        }
                    }
                }

                // Réception de la question du serveur pour rematch
                network.Send(partie.SerializeData("Voulez-vous une revanche (o/n)"));
                string reponseClient = partie.DeserializeStringData(network.Receive());
                _rematch = reponseClient == "o";

                Console.Clear();
            }
        }
    }
}
