using System;
using System.Collections.Generic;
using System.Linq;
using NCDK;
using NCDK.Default;
using NCDK.Graphs.InChI;
using NCDK.Smiles;
using NCDK.Tools.Manipulator;

namespace RxnSplitter
{
    public class Splitter
    {
        private static SmilesGenerator sg = new SmilesGenerator(SmiFlavors.Default | SmiFlavors.AtomAtomMap);
        private static readonly InChIGeneratorFactory inchiGeneratorFactory = InChIGeneratorFactory.Instance;

        public static List<IReaction> ParseAndSplitReaction(string smiles)
        {
            var rxn = ParseSmiles(smiles);
            filterInorganicsFromReaction(rxn);
            var map = GetMaps(rxn);
            return SplitReaction(rxn, map);
        }

        public static IReaction ParseSmiles(string smiles)
        {
            IReaction rxn = CDK.SmilesParser.ParseReactionSmiles(smiles);
            rxn.SetProperty(CDKPropertyName.SMILES, smiles);
            return rxn;
        }

        public static void filterInorganicsFromReaction(IReaction rxn)
        {
            filterInorganicsFromIterator(rxn.Reactants);
            filterInorganicsFromIterator(rxn.Products);
            return;
        }

        private static void filterInorganicsFromIterator(IChemObjectSet<IAtomContainer> rxnIt)
        {
            for (int i = rxnIt.Count - 1; i >= 0; --i)
            {
                var sub = rxnIt[i];
                var formula = MolecularFormulaManipulator.GetMolecularFormula(sub);
                if (!MolecularFormulaManipulator.ContainsElement(formula, ChemicalElement.C))
                {
                    rxnIt.Remove(sub);
                }
            }
        }

        public static IDictionary<IAtom, IAtom> GetMaps(IReaction rxn)
        {
            var rctMapNumbers = getMapNumbers(rxn.Reactants);
            var prdMapNumbers = getMapNumbers(rxn.Products);
            Dictionary<int, IMapping> atomMap = rctMapNumbers.Keys.Union(prdMapNumbers.Keys).
                Select(n => {
                    IAtom rctAtom, prdAtom;
                    rctMapNumbers.TryGetValue(n, out rctAtom);
                    prdMapNumbers.TryGetValue(n, out prdAtom);
                    return new KeyValuePair<int, IMapping>(n, new Mapping(rctAtom, prdAtom));
                }).
                ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            IDictionary<IAtom, IAtom> rctMap = rctMapNumbers.
                Select(kvp => {
                    IMapping outMapping;
                    IAtom prdAtom = null;
                    if (atomMap.TryGetValue(kvp.Key, out outMapping))
                    {
                        prdAtom = (IAtom) outMapping[1];
                    }
                    return new KeyValuePair<IAtom, IAtom>(kvp.Value, prdAtom);
                }).
                ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return rctMap;
        }

        private static IDictionary<int, IAtom> getMapNumbers(IEnumerable<IAtomContainer> atomList)
        {
            return atomList.
                SelectMany(sub => sub.Atoms).
                Select(atom => new KeyValuePair<int, IAtom>(atom.GetProperty<int>(CDKPropertyName.AtomAtomMapping, Int32.MaxValue), atom)).
                Where(kvp => kvp.Key != Int32.MaxValue).
                ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static List<IReaction> SplitReaction(IReaction rxn, IDictionary<IAtom, IAtom> map)
        {
            var rctPrdPairs = from rct in rxn.Reactants
                              from prd in rxn.Products
                              select new { rct, prd };
            var mappedPairs = rctPrdPairs.Where(pair => {
                if (map.Count == 0)
                {
                    return true;
                } else if (pair.rct.Atoms.Any(rctAtom => {
                    if (map.ContainsKey(rctAtom) && pair.prd.Atoms.Contains(map[rctAtom]))
                    {
                        return true;
                    } else
                    {
                        return false;
                    }
                }))
                {
                    return true;
                } else
                {
                    return false;
                }
            });
            return mappedPairs.Select(mp => {
                var newRxn = ChemObjectBuilder.Instance.NewReaction();
                newRxn.Reactants.Add(AtomContainerManipulator.CopyAndSuppressedHydrogens(mp.rct));
                newRxn.Products.Add(AtomContainerManipulator.CopyAndSuppressedHydrogens(mp.prd));
                newRxn.SetProperty(CDKPropertyName.SMILES, sg.Create(newRxn));
                return newRxn;
            }).ToList();
        }

        public static Tuple<string, string, string> GetInchiKeyPair(IReaction edge)
        {
            var rctInchi = GenerateInchi(edge.Reactants[0]);
            var prdInchi = GenerateInchi(edge.Products[0]);
            if (rctInchi == null || prdInchi == null)
            {
                return null;
            } else {
                return Tuple.Create(rctInchi, prdInchi, edge.GetProperty<string>(CDKPropertyName.SMILES));
            }
        }

        private static string GenerateInchi(IAtomContainer sub)
        {
            var inchiGenerator = inchiGeneratorFactory.GetInChIGenerator(sub);
            var returnStatus = inchiGenerator.ReturnStatus;
            if (returnStatus == InChIReturnCode.Warning || returnStatus == InChIReturnCode.Ok){
                return inchiGenerator.GetInChIKey();
            } else
            {
                return null;
            }
        }
    }
}