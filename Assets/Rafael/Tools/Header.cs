using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

#if UNITY_EDITOR
public class Header : MonoBehaviour
{
    [ReadOnly]
    public string headerName = "header";
    public string style = "-";
    public int maxLength = 15;
    public bool spaceAroundName = true;

    public enum Justification{Center, Left, Right};

    public bool fit = true;

    //FIT
    [ConditionalField(nameof(fit))]
    public Justification fitJustify = Justification.Center;

    //NOT FIT
    [ConditionalField(nameof(fit), true)]
    public int nbOfStyle;

    [ConditionalField(nameof(fit), true)]
    public Justification notFitJustify = Justification.Center;


    private void OnValidate()
    {
        
    }
}
#endif
