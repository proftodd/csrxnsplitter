using System.Collections.Generic;
using NUnit.Framework;
using NCDK;

namespace RxnSplitter
{
    [TestFixture]
    public class SplitterTests
    {
        string fcSmiles = "[CH3:9][CH:8]([CH3:10])[c:7]1[cH:11][cH:12][cH:13][cH:14][cH:15]1.[CH2:3]([CH2:4][C:5](=[O:6])Cl)[CH2:2][Cl:1]>[Al+3].[Cl-].[Cl-].[Cl-].C(Cl)Cl>[CH3:9][CH:8]([CH3:10])[c:7]1[cH:11][cH:12][c:13]([cH:14][cH:15]1)[C:5](=[O:6])[CH2:4][CH2:3][CH2:2][Cl:1] |f:2.3.4.5| Friedel-Crafts acylation [3.10.1]";
        string esterSmiles = "[CH3:5][CH2:6][OH:7].[CH3:1][C:2](=[O:3])[OH:4]>[H+]>[CH3:5][CH2:6][O:7][C:2](=[O:3])[CH3:1].[OH2:4] Ethyl esterification [1.7.3]";
        string unmappedEsterSmiles = "CCO.CC(=O)O>[H+]>CCOC(=O)C.O Ethyl esterification [1.7.3]";
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

        [Test]
        public void it_filters_inorganic_reactants_and_products()
        {
            var rxn = Splitter.ParseSmiles(esterSmiles);
            Splitter.filterInorganicsFromReaction(rxn);
            Assert.AreEqual(2, rxn.Reactants.Count);
            Assert.AreEqual(1, rxn.Products.Count);
        }

        [Test]
        public void it_returns_empty_maps_if_no_mappings_are_present()
        {
            var rxn = Splitter.ParseSmiles(unmappedEsterSmiles);
            Splitter.filterInorganicsFromReaction(rxn);
            var map = Splitter.GetMaps(rxn);
            Assert.IsEmpty(map);
        }

        [Test]
        public void it_produces_correct_mappings()
        {
            var rxn = Splitter.ParseSmiles(esterSmiles);
            var ethanol = rxn.Reactants[0];
            var aceticAcid = rxn.Reactants[1];
            var ethylAcetate = rxn.Products[0];

            var expectedMap = new Dictionary<IAtom, IAtom>();
            expectedMap.Add(aceticAcid.Atoms[0], ethylAcetate.Atoms[5]);
            expectedMap.Add(aceticAcid.Atoms[1], ethylAcetate.Atoms[3]);
            expectedMap.Add(aceticAcid.Atoms[2], ethylAcetate.Atoms[4]);
            expectedMap.Add(aceticAcid.Atoms[3], null);
            expectedMap.Add(ethanol.Atoms[0], ethylAcetate.Atoms[0]);
            expectedMap.Add(ethanol.Atoms[1], ethylAcetate.Atoms[1]);
            expectedMap.Add(ethanol.Atoms[2], ethylAcetate.Atoms[2]);

            Splitter.filterInorganicsFromReaction(rxn);
            var map = Splitter.GetMaps(rxn);
            Assert.IsNotEmpty(map);
            Assert.AreEqual(expectedMap, map);
        }
    }
}