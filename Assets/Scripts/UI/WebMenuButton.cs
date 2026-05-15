using UnityEngine;

public class WebMenuButton : MonoBehaviour
{
    enum WebAction
    {
        NONE,

        // if a support is added not next to a trunk, it adds weight to the branch.
        // To be precise, a ws always adds 1 weight, it's just that when adjacent to a trunk, it also adds 2 support.
        ADD_SUPPORT,
        REMOVE_SUPPORT,
        WEBRELLA,
        STRING_DOWN,
    }

    [SerializeField]
    WebMenuButton upButton,
        rightButton,
        downButton,
        leftButton;

    [SerializeField]
    WebAction webAction;

    public WebMenuButton GetLeftButton() => leftButton;

    public WebMenuButton GetRightButton() => rightButton;

    public WebMenuButton GetUpButton() => upButton;

    public WebMenuButton GetDownButton() => downButton;

    public void OnClick()
    {
        switch (webAction)
        {
            case WebAction.ADD_SUPPORT:
                Debug.Log("Performing web action " + WebAction.ADD_SUPPORT);
                break;
            case WebAction.REMOVE_SUPPORT:
                Debug.Log("Performing web action " + WebAction.REMOVE_SUPPORT);
                break;
            case WebAction.WEBRELLA:
                Debug.Log("Performing web action " + WebAction.WEBRELLA);
                break;
            case WebAction.STRING_DOWN:
                Debug.Log("Performing web action " + WebAction.STRING_DOWN);
                break;
            default:
                Debug.Log("Performing web action " + WebAction.NONE);
                break;
        }
    }
}
