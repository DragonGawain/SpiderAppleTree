/*
A few possible cases.
Case 1: the lower branch is only being held by web strings (simple)
- The upper offers equal support to the lower. Weight on the lower connection point will affect the support potential of the upper.

Case 2: the lower branch also has some fixed (trunk) supports (complex)
- The upper increases the support strength of the lower by its own value. Load only gets transferred to the upper once the
initial support value of the lower is surpassed.

*/
public class HangingWebString : ISupport
{
    Coord coord;

    // public SupportType GetSupportType()
    // {
    //     throw new System.NotImplementedException();
    // }

    public int GetSupportValue()
    {
        throw new System.NotImplementedException();
    }

    public Coord GetCoord() => coord;

    public void OnSnap(Coord coord)
    {
        throw new System.NotImplementedException();
    }
}
