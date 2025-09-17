using System;
using System.Linq;

namespace TestClient
{
    /// <summary>
    /// Wrapper pour accéder aux méthodes statiques du client
    /// </summary>
    public static class ClientWrapper
    {
        /// <summary>
        /// Valide une adresse IP (copie de la logique du client)
        /// </summary>
        public static bool ValiderAdresseIp(string adresse)
        {
            if (string.IsNullOrWhiteSpace(adresse)) return false;

            string[] splitValues = adresse.Split('.');
            if (splitValues.Length != 4) return false;

            bool valide = splitValues.All(part => byte.TryParse(part, out _));
            return valide;
        }
    }
}
