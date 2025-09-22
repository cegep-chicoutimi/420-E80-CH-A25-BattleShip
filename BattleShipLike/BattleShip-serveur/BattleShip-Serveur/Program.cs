using BattleShipLibrary;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BattleShip_Serveur
{
    internal class Program
    {
        private const int PORT = 22222;
        private static bool _rematch = true;

        private static ConsoleColor couleurJoueur;
        private static ConsoleColor couleurServeur;

        static void Main(string[] args)
        {
            AttenteClient();
        }

        public static void JouerPartie(Socket socket)
        {
            int hits = 0;
            bool win = false;

            BattleShip partie = new BattleShip();
            int nbTotalCases = 11;

            partie.CouleurJoueur = couleurJoueur;
            partie.CouleurServeur = couleurServeur;

            // On attend la taille de la grille envoyée par le client (ex: "4/4")
            string tailles = partie.DeserializeStringData(RecevoirMessage(socket));
            string[] parts = tailles.Split('/');
            int colonnes = int.Parse(parts[0]);
            int lignes = int.Parse(parts[1]);

            // On démarre la partie avec ces tailles
            partie.StartGame(colonnes, lignes);

            bool monTour = false; // le client commence dans ton protocole initial

            while (!win)
            {
                if (!monTour)
                {
                    // --- Tour du client : il peut enchainer s'il touche ---
                    bool clientContinuer = true;
                    while (clientContinuer && !win)
                    {
                        string coordRecus = partie.DeserializeStringData(RecevoirMessage(socket));
                        string resultSerialized = partie.IsTouched(coordRecus); // résultat sérialisé envoyé au client
                        EnvoyerMessage(socket, resultSerialized);
                        bool toucheParClient = partie.DeserializeBoolData(resultSerialized);

                        string confirmation = partie.DeserializeStringData(RecevoirMessage(socket));
                        if (confirmation != "1")
                        {
                            // le client a indiqué qu'il a gagné
                            win = true;
                            WriteWarning("Vous avez perdu !");
                            break;
                        }

                        if (toucheParClient)
                        {
                            // le client a touché -> il rejoue (on reste dans la boucle)
                            clientContinuer = true;
                        }
                        else
                        {
                            // le client a manqué -> on prend le tour
                            clientContinuer = false;
                            monTour = true;
                        }
                    }
                }
                else
                {
                    // --- Tour du serveur : on tire tant qu'on touche ---
                    bool continuer = true;
                    while (continuer && !win)
                    {
                        string coords = partie.ChoisirCase("Case à toucher : ");
                        EnvoyerMessage(socket, partie.SerializeData(coords));

                        bool touche = partie.DeserializeBoolData(RecevoirMessage(socket));
                        var (col, row) = partie.Positions[coords];

                        if (touche)
                        {
                            partie.EnemyGrille[col, row] = "X";
                            hits++;
                            WriteSuccessful("Bateau touché ! Vous rejouez !");
                            // continuer reste true
                        }
                        else
                        {
                            partie.EnemyGrille[col, row] = "O";
                            WriteWaiting("Bateau non touché. À l’adversaire !");
                            continuer = false; // on passe le tour au client
                        }

                        partie.AfficherMaGrille();
                        partie.AfficherEnemyGrille();
                        Console.WriteLine($"Vous avez touché {hits} fois / {nbTotalCases}");

                        if (hits == nbTotalCases)
                        {
                            EnvoyerMessage(socket, partie.SerializeData("0"));
                            WriteSuccessful("Vous avez gagné !");
                            win = true;
                            break;
                        }
                        else
                        {
                            EnvoyerMessage(socket, partie.SerializeData("1"));
                        }
                    }
                    monTour = !monTour;
                }
            }

            // Demande de rejouer
            WriteWaiting("Envoi de la demande de rematch au client...");
            EnvoyerMessage(socket, partie.SerializeData("Voulez-vous rejouer une partie (o/n)"));

            string rejouer = partie.DeserializeStringData(RecevoirMessage(socket));
            _rematch = rejouer == "o";
            if (_rematch) Console.Clear();
        }



        #region Communication Réseau
        public static void AttenteClient()
        {
            IPAddress ipAdress = IPAddress.Any;
            IPEndPoint endPoint = new IPEndPoint(ipAdress, PORT);

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(endPoint);
                listener.Listen(1);
                ObtenirCouleursJoueurs();

                while (true)
                {
                    Console.Clear();
                    WriteWaiting("En attente d'une connexion...");

                    using Socket handler = listener.Accept();
                    WriteSuccessful("Client connecté !");


                    _rematch = true;
                    while (_rematch)
                    {
                        JouerPartie(handler);
                    }
                }
            }
            catch (Exception e)
            {
                WriteWarning(e.Message);
            }
        }
        private static void EnvoyerMessage(Socket socket, string data)
        {
            string message = data + "?";
            byte[] msg = Encoding.ASCII.GetBytes(message);
            socket.Send(msg, SocketFlags.None);
        }

        private static string RecevoirMessage(Socket socket)
        {
            byte[] buffer = new byte[100];
            int bytesRec = socket.Receive(buffer);
            string data = Encoding.ASCII.GetString(buffer, 0, bytesRec);
            return data.Contains("?") ? data.Substring(0, data.IndexOf("?")) : data;
        }
        #endregion

        #region Affichage Console
        private static void WriteWarning(string text) => WriteColored(text, ConsoleColor.Red);
        private static void WriteSuccessful(string text) => WriteColored(text, ConsoleColor.Green);
        private static void WriteWaiting(string text) => WriteColored(text, ConsoleColor.Yellow);

        private static void WriteColored(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        static ConsoleColor ChoisirCouleur(string nomJoueur)
        {
            Console.WriteLine($"Choisissez une couleur pour {nomJoueur} :");
            var couleurs = Enum.GetValues(typeof(ConsoleColor));

            int index = 0;
            foreach (var couleur in couleurs)
            {
                Console.WriteLine($"{index}: {couleur}");
                index++;
            }

            Console.Write("Votre choix (numéro) : ");
            string saisie = Console.ReadLine();

            if (int.TryParse(saisie, out int choix) && choix >= 0 && choix < couleurs.Length)
            {
                return (ConsoleColor)couleurs.GetValue(choix);
            }
            else
            {
                Console.WriteLine("Choix invalide. Couleur par défaut utilisée (Gray).");
                return ConsoleColor.Gray;
            }
        }

        static void ObtenirCouleursJoueurs()
        {
            string configPath = "couleurs_config.txt";

            if (File.Exists(configPath))
            {
                string[] lignes = File.ReadAllLines(configPath);
                couleurJoueur = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), lignes[0]);
                couleurServeur = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), lignes[1]);

                Console.WriteLine($"Couleur Joueur chargée : {couleurJoueur}");
                Console.WriteLine($"Couleur Serveur chargée : {couleurServeur}");
            }
            else
            {
                Console.WriteLine("Configuration non trouvée. Choisissez les couleurs :");

                couleurJoueur = ChoisirCouleur("Joueur");
                do
                {
                    couleurServeur = ChoisirCouleur("Serveur");
                    if (couleurServeur == couleurJoueur)
                        Console.WriteLine("Les deux joueurs ne peuvent pas avoir la même couleur.");
                } while (couleurServeur == couleurJoueur);

                File.WriteAllLines(configPath, new[]
                {
                    couleurJoueur.ToString(),
                    couleurServeur.ToString()
                });

                Console.WriteLine("Configuration enregistrée.");
            }
        }
        #endregion
    }
}
