using NCDK;
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
    }
}