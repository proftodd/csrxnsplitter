using NUnit.Framework;
using NCDK;

namespace RxnSplitter
{
    [TestFixture]
    public class SplitterTests
    {
        string fcSmiles = "[CH3:9][CH:8]([CH3:10])[c:7]1[cH:11][cH:12][cH:13][cH:14][cH:15]1.[CH2:3]([CH2:4][C:5](=[O:6])Cl)[CH2:2][Cl:1]>[Al+3].[Cl-].[Cl-].[Cl-].C(Cl)Cl>[CH3:9][CH:8]([CH3:10])[c:7]1[cH:11][cH:12][c:13]([cH:14][cH:15]1)[C:5](=[O:6])[CH2:4][CH2:3][CH2:2][Cl:1] |f:2.3.4.5| Friedel-Crafts acylation [3.10.1]";
        string methaneOxidation = "C>>O=C=O";

        [Test]
        public void it_can_parse_a_reaction_smiles()
        {
            IReaction rxn = Splitter.ParseSmiles(fcSmiles);
            Assert.NotNull(rxn);
        }

        [Test]
        public void it_adds_smiles_to_parsed_reaction_as_a_property()
        {
            var rxn = Splitter.ParseSmiles(methaneOxidation);
            Assert.NotNull(rxn.GetProperty<string>(CDKPropertyName.SMILES));
            Assert.AreEqual(methaneOxidation, rxn.GetProperty<string>(CDKPropertyName.SMILES));
        }

        [Test]
        public void it_correctly_parses_reactants_and_products()
        {
            var rxn = Splitter.ParseSmiles(fcSmiles);
            Assert.AreEqual(2, rxn.Reactants.Count);
            Assert.AreEqual(1, rxn.Products.Count);
        }
    }
}