using BattleShipLibrary;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BattleShip_Serveur
{
    public class Program
    {
        private const int PORT = 22222;
        private static bool _rematch = true;

        static void Main(string[] args)
        {
            AttenteClient();
        }

        public static void AttenteClient()
        {
            IPAddress ipAdress = IPAddress.Any;
            IPEndPoint endPoint = new IPEndPoint(ipAdress, PORT);

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(endPoint);
                listener.Listen(1);

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

        public static void JouerPartie(Socket socket)
        {
            int hits = 0;
            bool win = false;

            BattleShip partie = new BattleShip();
            partie.StartGame();

            while (!win)
            {
                // --- Tour de l’adversaire ---
                string coordRecus = partie.DeserializeStringData(RecevoirMessage(socket));
                EnvoyerMessage(socket, partie.IsTouched(coordRecus));

                string confirmation = partie.DeserializeStringData(RecevoirMessage(socket));
                if (confirmation != "1")
                {
                    win = true;
                    WriteWarning("Vous avez perdu !");
                }
                else
                {
                    // --- Tour du serveur ---
                    string coords = partie.ChoisirCase("Case à toucher : ");
                    EnvoyerMessage(socket, partie.SerializeData(coords));

                    bool touche = partie.DeserializeBoolData(RecevoirMessage(socket));

                    var (col, row) = partie.Positions[coords];
                    if (touche)
                    {
                        partie.EnemyGrille[col, row] = "X";
                        hits++;
                        WriteSuccessful("Bateau touché !");
                    }
                    else
                    {
                        partie.EnemyGrille[col, row] = "O";
                        WriteWaiting("Bateau non touché");
                    }

                    partie.AfficherMaGrille();
                    partie.AfficherEnemyGrille();

                    if (hits == 2)
                    {
                        EnvoyerMessage(socket, partie.SerializeData("0"));
                        WriteSuccessful("Vous avez gagné !");
                        win = true;
                    }
                    else EnvoyerMessage(socket, partie.SerializeData("1"));
                }
            }

            string rejouer = partie.DeserializeStringData(RecevoirMessage(socket));
            if (rejouer == "o")
            {
                _rematch = true;
                Console.Clear();
            }
            else
            {
                _rematch = false;
                Console.WriteLine("Le client ne veut pas rejouer. Appuyez sur Entrée pour vous remettre en attente");
                Console.ReadKey();
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
        #endregion
    }
}
