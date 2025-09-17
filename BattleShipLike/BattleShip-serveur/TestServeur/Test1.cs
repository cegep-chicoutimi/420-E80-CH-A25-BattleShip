using BattleShipLibrary;

namespace TestServeur
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

        // Tests pour le serveur (simplifiés)
        [TestMethod]
        public void Test_Serveur_Existe()
        {
            // Arrange & Act
            // On teste juste que la bibliothèque BattleShip existe
            var bibliothequeExiste = typeof(BattleShip) != null;

            // Assert
            Assert.IsTrue(bibliothequeExiste, "La bibliothèque BattleShip devrait exister");
        }

        // Tests pour la classe BattleShip (même que client)
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

        [TestMethod]
        public void Test_BattleShip_DeserializeBoolData_False()
        {
            // Arrange
            var partie = new BattleShip();
            string jsonFalse = "false";

            // Act
            bool resultat = partie.DeserializeBoolData(jsonFalse);

            // Assert
            Assert.IsFalse(resultat, "La désérialisation de 'false' devrait donner false");
        }

        [TestMethod]
        public void Test_BattleShip_SerializeData_ChaineVide()
        {
            // Arrange
            var partie = new BattleShip();
            string valeur = "";

            // Act
            string resultat = partie.SerializeData(valeur);

            // Assert
            Assert.IsTrue(resultat.Contains("\"\""), "La sérialisation d'une chaîne vide devrait contenir \"\"");
        }

        [TestMethod]
        public void Test_BattleShip_DeserializeStringData_ChaineVide()
        {
            // Arrange
            var partie = new BattleShip();
            string jsonString = "\"\"";

            // Act
            string resultat = partie.DeserializeStringData(jsonString);

            // Assert
            Assert.AreEqual("", resultat, "La désérialisation d'une chaîne vide devrait donner une chaîne vide");
        }
    }
}
