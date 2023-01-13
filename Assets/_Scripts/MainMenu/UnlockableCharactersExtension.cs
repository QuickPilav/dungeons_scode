using System;
using System.Collections.Generic;
using static CharactersUI;

public static class UnlockableCharactersExtension
{
    public static UnlockablePlayerClasses Add(this UnlockablePlayerClasses userType, params UnlockablePlayerClasses[] typesToAdd)
    {
        foreach (var flag in typesToAdd)
        {
            userType |= flag;
        }
        return userType;
    }
    public static UnlockablePlayerClasses Remove(this UnlockablePlayerClasses userType, params UnlockablePlayerClasses[] typesToRemove)
    {
        foreach (var item in typesToRemove)
        {
            userType &= ~item;
        }
        return userType;
    }
    public static bool CustomHasFlag(this UnlockablePlayerClasses userType, UnlockablePlayerClasses typeToCompare)
        => (userType & typeToCompare) == typeToCompare;

    public static IEnumerable<UnlockablePlayerClasses> GetFlags(this UnlockablePlayerClasses input)
    {
        foreach (UnlockablePlayerClasses value in Enum.GetValues(input.GetType()))
            if (input.CustomHasFlag(value))
                yield return value;
    }

    public static UnlockablePlayerClasses ConvertToUnlockable(this PlayerClass plyClass)
        => (UnlockablePlayerClasses)Enum.GetValues(typeof(UnlockablePlayerClasses)).GetValue((int)plyClass + 1);


    /*
     
    if(!deneme.CustomHasFlag( Deneme.classMami))
    {
        deneme = deneme.Add(Deneme.classMami);
    }
    else
    {
        deneme = deneme.Remove(Deneme.classMami);
    }


     */
}
