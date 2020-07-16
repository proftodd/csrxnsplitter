using NCDK;

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
    }
}