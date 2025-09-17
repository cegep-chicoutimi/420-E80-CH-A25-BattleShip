using BattleShipLibrary;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace BattleShip_client
{
    public class Program
    {
        private const int PORT = 22222;

        private static bool _adresseValide = false;
        private static string _adresseIp = "0.0.0.0";

        private static bool _rematch = true;

        static void Main(string[] args)
        {
            DemandeInfoServeur();
        }

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
                JouerPartie(socket);
            }
            catch (ArgumentNullException ane)
            {
                WriteWarning($"Exception d'argument null : {ane.Message}");
            }
            catch (SocketException se)
            {
                WriteWarning($"Exception de Socket : {se.Message}");
            }
            catch (Exception e)
            {
                WriteWarning($"Exception inattendue : {e.Message}");
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

        public static void JouerPartie(Socket sender)
        {
            while (_rematch)
            {
                _rematch = false;
                int hits = 0;

                BattleShip partie = new BattleShip();
                partie.StartGame();

                bool win = false;

                while (!win)
                {
                    // --- Tour du joueur ---
                    string coords = partie.ChoisirCase("Choisissez la case à toucher : ");
                    EnvoyerMessage(sender, partie.SerializeData(coords));

                    bool touche = partie.DeserializeBoolData(RecevoirMessage(sender));

                    Console.Clear();
                    if (touche)
                    {
                        var (col, row) = partie.Positions[coords];
                        partie.EnemyGrille[col, row] = "X";
                        hits++;
                        WriteSuccessful("Bateau touché !");
                    }
                    else
                    {
                        var (col, row) = partie.Positions[coords];
                        partie.EnemyGrille[col, row] = "O";
                        WriteWaiting("Bateau non touché");
                    }
                    partie.AfficherMaGrille();
                    partie.AfficherEnemyGrille();
                    if (hits == 2)
                    {
                        EnvoyerMessage(sender, partie.SerializeData("0"));
                        WriteSuccessful("Vous avez gagné !");
                        win = true;
                    }
                    else EnvoyerMessage(sender, partie.SerializeData("1"));

                    if (!win)
                    {
                        // --- Tour adverse ---
                        string coordRecus = partie.DeserializeStringData(RecevoirMessage(sender));
                        EnvoyerMessage(sender, partie.IsTouched(coordRecus));

                        string confirmation = partie.DeserializeStringData(RecevoirMessage(sender));
                        if (confirmation != "1")
                        {
                            win = true;
                            WriteWarning("Vous avez perdu !");
                        }
                    }
                }

                string r;
                string rejouer = partie.DeserializeStringData(RecevoirMessage(sender));
                do
                {
                    Console.WriteLine(rejouer);
                    r = Console.ReadLine();
                    if (r != "o" && r != "n") WriteWarning("Entrez une réponse valide !");
                } while (r != "o" && r != "n");

                EnvoyerMessage(sender, partie.SerializeData(r));

                if (r == "o") _rematch = true;
                Console.Clear();
            }
        }
        #region Communication Réseau
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
        #endregion
    }
}