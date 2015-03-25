using System.Collections;
using UnityEngine;

public struct GridCell {
    public int ID;
    public GridCell(int id)
    {
        ID = id;
    }
    public override string ToString() { return "" + ID; }
}
