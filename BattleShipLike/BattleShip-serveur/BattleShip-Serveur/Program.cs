using BattleShipLibrary;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BattleShip_Serveur
{
    public class Program
    {
        //Serveur
        static bool ConditionFin;

        static void Main(string[] args)
        {
            AttenteClient();
        }

        public static void AttenteClient()
        {
            IPAddress ipAdress = IPAddress.Any;
            IPEndPoint endPoint = new IPEndPoint(ipAdress, 22222);

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(endPoint);
                listener.Listen(1);

                while (true)
                {
                    Console.Clear();
                    WriteWaiting("En attente d'une connexion ...");

                    Socket handler = listener.Accept();
                    ConditionFin = false;

                    try
                    {
                        JouerPartie(handler);
                    }
                    catch (Exception ex)
                    {
                        WriteWarning($"Erreur durant la partie : {ex.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                WriteWarning(e.Message);
            }
        }

        public static void JouerPartie(Socket handler)
        {
            Console.Clear();
            WriteSuccessful("Client connecté!");

            while (!ConditionFin)
            {
                BattleShip partie = new BattleShip();
                partie.StartGame();

                int hits = 0;
                bool win = false;

                while (!win)
                {
                    // Tour de l’adversaire
                    string coordRecu = partie.DeserializeStringData(RecevoirMessage(handler));
                    string resultat = partie.IsTouched(coordRecu);
                    EnvoyerMessage(handler, resultat);

                    string ack = partie.DeserializeStringData(RecevoirMessage(handler));
                    if (ack != "1")
                    {
                        win = true;
                        WriteWarning("Vous avez perdu :`(");
                    }
                    else
                    {
                        // Tour du serveur
                        string coords = partie.ChoisirCase("Case à toucher : ");
                        EnvoyerMessage(handler, partie.SerializeData(coords));

                        bool touche = partie.DeserializeBoolData(RecevoirMessage(handler));
                        var (col, row) = partie.Positions[coords];

                        if (touche)
                        {
                            partie.EnemyGrille[col, row] = "X";
                            hits++;
                            WriteSuccessful("Bateau touché!");
                        }
                        else
                        {
                            partie.EnemyGrille[col, row] = "-";
                            WriteWaiting("Bateau non touché.");
                        }

                        partie.AfficherMaGrille();
                        partie.AfficherEnemyGrille();

                        if (hits == 2)
                        {
                            EnvoyerMessage(handler, partie.SerializeData("0"));
                            win = true;
                            WriteSuccessful("Vous avez gagné!");
                        }
                        else
                        {
                            EnvoyerMessage(handler, partie.SerializeData("1"));
                        }
                    }
                }

                if (DemanderRejouer(partie, handler) == "n")
                {
                    Console.WriteLine("Le client ne veut pas rejouer. Appuyez sur Entrée pour vous remettre en attente");
                    Console.ReadKey();
                    ConditionFin = true;
                }
            }
        }

        public static string DemanderRejouer(BattleShip partie, Socket handler)
        {
            WriteWaiting("Envoie de la demande de rematch au client. Veuillez patienter...");
            EnvoyerMessage(handler, partie.SerializeData("Voulez-vous rejouer une partie (o/n)"));
            return partie.DeserializeStringData(RecevoirMessage(handler));
        }

        #region Méthodes Réseau
        private static string RecevoirMessage(Socket handler)
        {
            byte[] buffer = new byte[100];
            int bytesRec = handler.Receive(buffer);

            string data = Encoding.ASCII.GetString(buffer, 0, bytesRec);
            return CleanData(data);
        }

        private static void EnvoyerMessage(Socket handler, string message)
        {
            message += "?";
            byte[] msg = Encoding.ASCII.GetBytes(message);
            handler.Send(msg, SocketFlags.None);
        }

        private static string CleanData(string data)
        {
            return data.Contains("?") ? data.Substring(0, data.IndexOf("?")) : data;
        }
        #endregion

        #region Modificateur de texte
        public static void WriteWarning(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void WriteSuccessful(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void WriteWaiting(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        #endregion
    }
}
