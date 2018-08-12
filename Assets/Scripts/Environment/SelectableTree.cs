using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Tree))]
public class SelectableTree : Selectable
{
    private Tree _tree;

    private void Start()
    {
        _tree = GetComponent<Tree>();
    }

    public override void Select()
    {
        _tree.Cut(1000, new Vector3(1, 0, 0));
    }
}
