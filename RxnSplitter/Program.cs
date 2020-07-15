using System;
using com.epam.indigo;

namespace RxnSplitter
{
    class Program
    {
        static void Main(string[] args)
        {
            Indigo indigo = new Indigo();
            Console.WriteLine($"Indigo version {indigo.version()}");
            IndigoObject mol = indigo.loadMolecule("[C:1]([C:5]1[CH:10]=[CH:9][C:8]([OH:11])=[CH:7][CH:6]=1)([CH3:4])([CH3:3])[CH3:2]");
            Console.WriteLine(mol.molfile());
            IndigoObject rxn = indigo.loadReaction("[Cl-].[Al+3].[Cl-].[Cl-].[Cl:5][CH2:6][CH2:7][CH2:8][C:9](Cl)=[O:10].[C:12]1([CH:18]([CH3:20])[CH3:19])[CH:17]=[CH:16][CH:15]=[CH:14][CH:13]=1>C(Cl)Cl>[Cl:5][CH2:6][CH2:7][CH2:8][C:9]([C:15]1[CH:16]=[CH:17][C:12]([CH:18]([CH3:20])[CH3:19])=[CH:13][CH:14]=1)=[O:10] |f:0.1.2.3|	US20010000038A1	0256	2001	86%	86.9%");
            Console.WriteLine("Reactants:");
            foreach (IndigoObject rct in rxn.iterateReactants())
            {
                Console.WriteLine(rct.smiles());
            }
            Console.WriteLine("Products:");
            foreach(IndigoObject prd in rxn.iterateProducts())
            {
                Console.WriteLine(prd.smiles());
            }
        }
    }
}
