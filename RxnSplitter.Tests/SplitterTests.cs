using System.Collections.Generic;
using NUnit.Framework;
using NCDK;
using NCDK.Isomorphisms;

namespace RxnSplitter
{
    [TestFixture]
    public class SplitterTests
    {
        string fcSmiles = "[CH3:9][CH:8]([CH3:10])[c:7]1[cH:11][cH:12][cH:13][cH:14][cH:15]1.[CH2:3]([CH2:4][C:5](=[O:6])Cl)[CH2:2][Cl:1]>[Al+3].[Cl-].[Cl-].[Cl-].C(Cl)Cl>[CH3:9][CH:8]([CH3:10])[c:7]1[cH:11][cH:12][c:13]([cH:14][cH:15]1)[C:5](=[O:6])[CH2:4][CH2:3][CH2:2][Cl:1] |f:2.3.4.5| Friedel-Crafts acylation [3.10.1]";
        string esterSmiles = "[CH3:5][CH2:6][OH:7].[CH3:1][C:2](=[O:3])[OH:4]>[H+]>[CH3:5][CH2:6][O:7][C:2](=[O:3])[CH3:1].[OH2:4] Ethyl esterification [1.7.3]";
        string unmappedEsterSmiles = "CCO.CC(=O)O>[H+]>CCOC(=O)C.O Ethyl esterification [1.7.3]";
        string chiralResolution = "[NH2:1][CH:2]([CH3:3])[C:4](=[O:5])[OH:6].[NH2:7][CH:8]([CH3:9])[C:10](=[O:11])[OH:12].[CH3:13][C:14](=[O:15])[O:16][C:17](=[O:18])[CH3:19]>>[NH2:1][C@H:2]([CH3:3])[C:4](=[O:5])[OH:6].[CH3:13][C:14](=[O:15])[NH:7][C@@H:8]([CH3:9])[C:10](=[O:11])[OH:12].[OH:16][C:17](=[O:18])[CH3:19] Chiral resolution";
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

        [Test]
        public void it_maps_all_reactants_to_all_products_if_no_map_is_present()
        {
            var rxn = Splitter.ParseSmiles(unmappedEsterSmiles);
            Splitter.filterInorganicsFromReaction(rxn);
            var map = Splitter.GetMaps(rxn);
            List<IReaction> edges = Splitter.SplitReaction(rxn, map);
            Assert.AreEqual(2, edges.Count);
            Assert.IsTrue(edges.TrueForAll(e => e.Reactants.Count == 1 && e.Products.Count == 1));
            // TODO: Find a way to make this independent of order of generated edges
            Assert.IsTrue(new IsomorphismTester(edges[0].Reactants[0]).IsIsomorphic(rxn.Reactants[0]));
            Assert.IsTrue(new IsomorphismTester(edges[1].Reactants[0]).IsIsomorphic(rxn.Reactants[1]));
            Assert.IsTrue(new IsomorphismTester(edges[0].Products[0]).IsIsomorphic(rxn.Products[0]));
            Assert.IsTrue(new IsomorphismTester(edges[1].Products[0]).IsIsomorphic(rxn.Products[0]));
        }

        [Test]
        public void it_uses_maps_to_correctly_split_reactions()
        {
            var rxn = Splitter.ParseSmiles(chiralResolution);
            Splitter.filterInorganicsFromReaction(rxn);
            var map = Splitter.GetMaps(rxn);
            var edges = Splitter.SplitReaction(rxn, map);
            Assert.AreEqual(4, edges.Count);
            Assert.IsTrue(edges.TrueForAll(e => e.Reactants.Count == 1 && e.Products.Count == 1));
            // TODO: Find a way to make this independent of order of generated edges
            // DL-alanine to L-alanine
            Assert.IsTrue(new IsomorphismTester(edges[0].Reactants[0]).IsIsomorphic(rxn.Reactants[0]));
            Assert.IsTrue(new IsomorphismTester(edges[0].Products[0]).IsIsomorphic(rxn.Products[0]));
            // DL-alanine to n-acetyl-D-alanine
            Assert.IsTrue(new IsomorphismTester(edges[1].Reactants[0]).IsIsomorphic(rxn.Reactants[1]));
            Assert.IsTrue(new IsomorphismTester(edges[1].Products[0]).IsIsomorphic(rxn.Products[1]));
            // acetic anhydride to n-acetyl-D-alanine
            Assert.IsTrue(new IsomorphismTester(edges[2].Reactants[0]).IsIsomorphic(rxn.Reactants[2]));
            Assert.IsTrue(new IsomorphismTester(edges[2].Products[0]).IsIsomorphic(rxn.Products[1]));
            // acetic anhydride to acetic acid
            Assert.IsTrue(new IsomorphismTester(edges[3].Reactants[0]).IsIsomorphic(rxn.Reactants[2]));
            Assert.IsTrue(new IsomorphismTester(edges[3].Products[0]).IsIsomorphic(rxn.Products[2]));
        }

        [Test]
        public void it_adds_smiles_to_edges_if_no_map_is_present()
        {
            var edges = Splitter.ParseAndSplitReaction(unmappedEsterSmiles);
            Assert.IsTrue(edges.Exists(e => e.GetProperty<string>(CDKPropertyName.SMILES) == "CCO>>CCOC(=O)C"));
            Assert.IsTrue(edges.Exists(e => e.GetProperty<string>(CDKPropertyName.SMILES) == "CC(=O)O>>CCOC(=O)C"));
        }

        [Test]
        public void it_adds_smiles_to_edges_with_map_information()
        {
            var edges = Splitter.ParseAndSplitReaction(esterSmiles);
            Assert.IsTrue(edges.Exists(e => e.GetProperty<string>(CDKPropertyName.SMILES) == "[CH3:5][CH2:6][OH:7]>>[CH3:5][CH2:6][O:7][C:2](=[O:3])[CH3:1]"));
            Assert.IsTrue(edges.Exists(e => e.GetProperty<string>(CDKPropertyName.SMILES) == "[CH3:1][C:2](=[O:3])[OH:4]>>[CH3:5][CH2:6][O:7][C:2](=[O:3])[CH3:1]"));
        }

        [Test]
        public void it_generates_InChIKeys_correctly()
        {
            var edge = Splitter.ParseAndSplitReaction(methaneOxidation)[0];
            edge.SetProperty(CDKPropertyName.SMILES, methaneOxidation);
            var ikPair = Splitter.GetInchiKeyPair(edge);
            Assert.AreEqual("VNWKTOKETHGBQD-UHFFFAOYSA-N", ikPair.Item1);
            Assert.AreEqual("CURLTUGMZLYLDI-UHFFFAOYSA-N", ikPair.Item2);
            Assert.AreEqual("C>>O=C=O", ikPair.Item3);
        }
    }
}