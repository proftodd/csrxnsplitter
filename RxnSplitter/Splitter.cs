using System.Collections.Generic;
using System.Linq;
using NCDK;
using NCDK.Default;
using NCDK.Tools.Manipulator;

namespace RxnSplitter
{
    public class Splitter
    {
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
                    IMapping theMapping;
                    IMapping outMapping;
                    if (atomMap.TryGetValue(kvp.Key, out outMapping))
                    {
                        theMapping = outMapping;
                    } else
                    {
                        theMapping = new Mapping(kvp.Value, null);
                    }
                    return new KeyValuePair<IAtom, IMapping>(kvp.Value, outMapping);
                }).
                Select(kvp => new KeyValuePair<IAtom, IAtom>(kvp.Key, (IAtom) kvp.Value[1])).
                ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return rctMap;
        }

        private static IDictionary<int, IAtom> getMapNumbers(IEnumerable<IAtomContainer> atomList)
        {
            return atomList.
                SelectMany(sub => sub.Atoms).
                Where(atom => atom.GetProperty<int>(CDKPropertyName.AtomAtomMapping, -1) != -1).
                Select(atom => new KeyValuePair<int, IAtom>(atom.GetProperty<int>(CDKPropertyName.AtomAtomMapping), atom)).
                ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}