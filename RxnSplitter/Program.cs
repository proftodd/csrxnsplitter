using System;
using NCDK;
using NCDK.Tools.Manipulator;

namespace RxnSplitter
{
    class Program
    {
        static void Main(string[] args)
        {
            IAtomContainer mol1 = Chem.MolFromSmiles("[C:1]([C:5]1[CH:10]=[CH:9][C:8]([OH:11])=[CH:7][CH:6]=1)([CH3:4])([CH3:3])[CH3:2]");
            IMolecularFormula form1 = MolecularFormulaManipulator.GetMolecularFormula(mol1);
            Console.WriteLine(MolecularFormulaManipulator.GetString(form1));
            IAtomContainer mol2 = Chem.MolFromFile("/Users/john/My Documents/1,3-diisopropenylbenzene.mol");
            var form2 = MolecularFormulaManipulator.GetMolecularFormula(mol2);
            Console.WriteLine(MolecularFormulaManipulator.GetString(form2));
        }
    }
}
