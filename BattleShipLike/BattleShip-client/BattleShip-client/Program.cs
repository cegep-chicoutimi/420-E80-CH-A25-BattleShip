using BattleShipLibrary;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

        static void Main(string[] args)
        {
            DemandeInfoServeur();
        }

        #region Connexion Réseau
        public static void DemandeInfoServeur()
        {
            do
            {
                Console.Write("Entrer l'adresse où vous connecter : ");
                string saisie = Console.ReadLine() ?? "";

                if (!ValiderAdresseIp(saisie))
                    WriteWarning("Adresse IP invalide.");
                else
                    _adresseValide = true;

            } while (!_adresseValide);

            Console.Clear();
            WriteWaiting($"Connexion à l'adresse {_adresseIp} en cours...");

            try
            {
                IPAddress ipAddress = IPAddress.Parse(_adresseIp);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, PORT);

                using Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ConnexionServeur(sender, remoteEP);
            }
            catch (Exception e)
            {
                WriteWarning(e.Message);
            }
        }

        public static void ConnexionServeur(Socket socket, IPEndPoint endPoint)
        {
            try
            {
                socket.Connect(endPoint);
                WriteSuccessful("Connecté au serveur !");
                ObtenirCouleursJoueurs();
                JouerPartie(socket);
            }
            catch (Exception e)
            {
                WriteWarning($"Erreur de connexion : {e.Message}");
            }

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
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
        public static void JouerPartie(Socket sender)
        {
            while (_rematch)
            {
                _rematch = false;
                int hits = 0;

                BattleShip partie = new BattleShip();
                int nbTotalCases = 11;
                partie.CouleurJoueur = couleurJoueur;
                partie.CouleurServeur = couleurServeur;

                int colonnes = SaisieTaille("Nombre de colonnes ?");
                int lignes = SaisieTaille("Nombre de lignes ?");
                EnvoyerMessage(sender, partie.SerializeData($"{colonnes}/{lignes}"));

                partie.StartGame(colonnes, lignes);
                bool win = false;
                bool monTour = true; // le client commence

                while (!win)
                {
                    if (monTour)
                    {
                        // Le client tire tant qu'il touche
                        bool continuer = true;
                        while (continuer && !win)
                        {
                            string coords = partie.ChoisirCase("Choisissez la case à toucher : ");
                            EnvoyerMessage(sender, partie.SerializeData(coords));

                            bool touche = partie.DeserializeBoolData(RecevoirMessage(sender));
                            var (col, row) = partie.Positions[coords];
                            Console.Clear();

                            if (touche)
                            {
                                partie.EnemyGrille[col, row] = "X";
                                hits++;
                                WriteSuccessful("Bateau touché ! Vous rejouez !");
                                // continuer reste true -> on rejoue
                            }
                            else
                            {
                                partie.EnemyGrille[col, row] = "O";
                                WriteWaiting("Bateau non touché. À l’adversaire !");
                                continuer = false; // on passe le tour
                            }

                            partie.AfficherMaGrille();
                            partie.AfficherEnemyGrille();
                            Console.WriteLine($"Vous avez touché {hits} fois / {nbTotalCases}");

                            if (hits == nbTotalCases)
                            {
                                EnvoyerMessage(sender, partie.SerializeData("0")); // j'ai gagné
                                WriteSuccessful("Vous avez gagné !");
                                win = true;
                                break;
                            }
                            else
                            {
                                EnvoyerMessage(sender, partie.SerializeData("1")); // la partie continue
                            }
                        }
                        monTour = !monTour; // si on a manqué, on passe au tour de l'adversaire
                    }
                    else
                    {
                        // Tour de l'adversaire : il peut enchaîner s'il touche
                        bool advContinuer = true;
                        while (advContinuer && !win)
                        {
                            string coordRecus = partie.DeserializeStringData(RecevoirMessage(sender));
                            string resultSerialized = partie.IsTouched(coordRecus); // string sérialisée (true/false)
                            EnvoyerMessage(sender, resultSerialized);
                            bool serveurATouche = partie.DeserializeBoolData(resultSerialized);

                            string confirmation = partie.DeserializeStringData(RecevoirMessage(sender));
                            if (confirmation != "1")
                            {
                                win = true;
                                WriteWarning("Vous avez perdu !");
                                break;
                            }

                            if (!serveurATouche)
                            {
                                // l'adversaire a manqué -> on reprend la main
                                advContinuer = false;
                                monTour = true;
                            }
                            // sinon advContinuer reste true et on continue à recevoir ses tirs
                        }
                    }
                }

                // Rematch (le serveur envoie la question)
                string questionRematch = partie.DeserializeStringData(RecevoirMessage(sender));
                string r;
                do
                {
                    Console.WriteLine(questionRematch);
                    r = Console.ReadLine();
                    if (r != "o" && r != "n") WriteWarning("Entrez une réponse valide !");
                } while (r != "o" && r != "n");

                EnvoyerMessage(sender, partie.SerializeData(r));
                if (r == "o") _rematch = true;

                Console.Clear();
            }
        }



        public static int SaisieTaille(string prompt)
        {
            int n;
            int tailleMin = 6;
            int tailleMax = 12;
            bool isValid;

            do
            {
                Console.WriteLine(prompt);
                isValid = int.TryParse(Console.ReadLine(), out n);
                if (!isValid || n < tailleMin || n > tailleMax)
                    Console.WriteLine($"Nombre invalide. Choisissez entre {tailleMin} et {tailleMax}.");
            } while (!isValid || n < tailleMin || n > tailleMax);

            return n;
        }
        #endregion

        #region Réseau
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
            return data.Substring(0, data.IndexOf("?"));
        }
        #endregion

        #region Affichage
        private static void WriteWarning(string text) => WriteColored(text, ConsoleColor.Red);
        private static void WriteSuccessful(string text) => WriteColored(text, ConsoleColor.Green);
        private static void WriteWaiting(string text) => WriteColored(text, ConsoleColor.Yellow);
        private static void WriteColored(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
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

        static ConsoleColor ChoisirCouleur(string nomJoueur)
        {
            Console.WriteLine($"Choisissez une couleur pour {nomJoueur} :");
            var couleurs = Enum.GetValues(typeof(ConsoleColor)).Cast<ConsoleColor>().ToArray();

            for (int i = 0; i < couleurs.Length; i++)
                Console.WriteLine($"{i}: {couleurs[i]}");

            Console.Write("Votre choix (numéro) : ");
            string saisie = Console.ReadLine();

            if (int.TryParse(saisie, out int choix) && choix >= 0 && choix < couleurs.Length)
                return couleurs[choix];

            Console.WriteLine("Choix invalide. Couleur par défaut utilisée (Gray).");
            return ConsoleColor.Gray;
        }
        #endregion
    }
}
