using BattleShipLibrary;

namespace TestClient
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void TestMethod1()
        {
            // Test simple pour vérifier que le projet compile
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_ValiderAdresseIp_AdresseValide()
        {
            // Arrange
            string adresseValide = "127.0.0.1";

            // Act
            bool resultat = ClientWrapper.ValiderAdresseIp(adresseValide);

            // Assert
            Assert.IsTrue(resultat, "127.0.0.1 devrait être une adresse IP valide");
        }

        [TestMethod]
        public void Test_ValiderAdresseIp_AdresseInvalide()
        {
            // Arrange
            string adresseInvalide = "300.300.300.300";

            // Act
            bool resultat = ClientWrapper.ValiderAdresseIp(adresseInvalide);

            // Assert
            Assert.IsFalse(resultat, "300.300.300.300 devrait être une adresse IP invalide");
        }

        [TestMethod]
        public void Test_ValiderAdresseIp_FormatIncorrect()
        {
            // Arrange
            string formatIncorrect = "abc.def.ghi";

            // Act
            bool resultat = ClientWrapper.ValiderAdresseIp(formatIncorrect);

            // Assert
            Assert.IsFalse(resultat, "abc.def.ghi devrait être invalide");
        }

        [TestMethod]
        public void Test_ValiderAdresseIp_ChaineVide()
        {
            // Arrange
            string chaineVide = "";

            // Act
            bool resultat = ClientWrapper.ValiderAdresseIp(chaineVide);

            // Assert
            Assert.IsFalse(resultat, "Une chaîne vide devrait être invalide");
        }

        [TestMethod]
        public void Test_ValiderAdresseIp_Localhost()
        {
            // Arrange
            string localhost = "localhost";

            // Act
            bool resultat = ClientWrapper.ValiderAdresseIp(localhost);

            // Assert
            Assert.IsFalse(resultat, "localhost devrait être invalide (pas au format IP)");
        }

        [TestMethod]
        public void Test_ValiderAdresseIp_AdresseAvecEspaces()
        {
            // Arrange
            string adresseAvecEspaces = " 127.0.0.1 ";

            // Act
            bool resultat = ClientWrapper.ValiderAdresseIp(adresseAvecEspaces);

            // Assert
            Assert.IsTrue(resultat, "Une adresse avec espaces est acceptée par la méthode originale");
        }

        [TestMethod]
        public void Test_ValiderAdresseIp_AdresseIncomplete()
        {
            // Arrange
            string adresseIncomplete = "192.168.1";

            // Act
            bool resultat = ClientWrapper.ValiderAdresseIp(adresseIncomplete);

            // Assert
            Assert.IsFalse(resultat, "Une adresse incomplète devrait être invalide");
        }

        [TestMethod]
        public void Test_ValiderAdresseIp_AdresseAvecLettres()
        {
            // Arrange
            string adresseAvecLettres = "192.168.1.a";

            // Act
            bool resultat = ClientWrapper.ValiderAdresseIp(adresseAvecLettres);

            // Assert
            Assert.IsFalse(resultat, "Une adresse avec des lettres devrait être invalide");
        }

        [TestMethod]
        public void Test_ValiderAdresseIp_AdresseNegative()
        {
            // Arrange
            string adresseNegative = "192.168.-1.1";

            // Act
            bool resultat = ClientWrapper.ValiderAdresseIp(adresseNegative);

            // Assert
            Assert.IsFalse(resultat, "Une adresse avec des valeurs négatives devrait être invalide");
        }

        [TestMethod]
        public void Test_ValiderAdresseIp_AdresseTropGrande()
        {
            // Arrange
            string adresseTropGrande = "192.168.1.256";

            // Act
            bool resultat = ClientWrapper.ValiderAdresseIp(adresseTropGrande);

            // Assert
            Assert.IsFalse(resultat, "Une adresse avec des valeurs > 255 devrait être invalide");
        }

        // Tests pour la classe BattleShip (simplifiés)
        [TestMethod]
        public void Test_BattleShip_SerializeData_Bool()
        {
            // Arrange
            var partie = new BattleShip();
            bool valeur = true;

            // Act
            string resultat = partie.SerializeData(valeur);

            // Assert
            Assert.IsTrue(resultat.Contains("true"), "La sérialisation bool devrait contenir 'true'");
        }

        [TestMethod]
        public void Test_BattleShip_SerializeData_String()
        {
            // Arrange
            var partie = new BattleShip();
            string valeur = "test";

            // Act
            string resultat = partie.SerializeData(valeur);

            // Assert
            Assert.IsTrue(resultat.Contains("test"), "La sérialisation string devrait contenir 'test'");
        }

        [TestMethod]
        public void Test_BattleShip_DeserializeBoolData()
        {
            // Arrange
            var partie = new BattleShip();
            string jsonTrue = "true";

            // Act
            bool resultat = partie.DeserializeBoolData(jsonTrue);

            // Assert
            Assert.IsTrue(resultat, "La désérialisation de 'true' devrait donner true");
        }

        [TestMethod]
        public void Test_BattleShip_DeserializeStringData()
        {
            // Arrange
            var partie = new BattleShip();
            string jsonString = "\"hello\"";

            // Act
            string resultat = partie.DeserializeStringData(jsonString);

            // Assert
            Assert.AreEqual("hello", resultat, "La désérialisation devrait donner 'hello'");
        }
    }
}
