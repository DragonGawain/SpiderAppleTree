public class HiddenSupportEditor : EditorElement
{
    protected override EditorIdentity GetEditorIdentity() => EditorIdentity.SUPPORT;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnStart();
        supportValue = 2;
    }
}
