public class TrunkSupportEditor : EditorElement
{
    protected override EditorIdentity GetEditorIdentity() => EditorIdentity.SUPPORT;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        coord = LevelManager.WorldSpaceToCoord(transform.position);
        OnStart();
    }

    public int supportValue = ISupport.TRUNK_SUPPORT;
}
