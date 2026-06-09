using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FruitEditorController : MonoBehaviour
{
    FruitEditor fe = null;

    [SerializeField]
    TMP_InputField weightIF,
        deltaWeightIF,
        deltaWebIF;

    [SerializeField]
    Toggle neededT;

    public void SelectFruit(FruitEditor fe)
    {
        this.fe = fe;
        ((TextMeshProUGUI)weightIF.placeholder).text = fe.weight.ToString();
        ((TextMeshProUGUI)deltaWeightIF.placeholder).text = fe.deltaWeight.ToString();
        ((TextMeshProUGUI)deltaWebIF.placeholder).text = fe.deltaWeb.ToString();
        neededT.isOn = fe.needed;
    }

    public void OnChangeWeight() =>
        fe.weight = int.TryParse(weightIF.text, out _)
            ? int.Parse(weightIF.text)
            : int.Parse(((TextMeshProUGUI)weightIF.placeholder).text);

    public void OnChangeDeltaWeight() =>
        fe.deltaWeight = int.TryParse(deltaWeightIF.text, out _)
            ? int.Parse(deltaWeightIF.text)
            : int.Parse(((TextMeshProUGUI)deltaWeightIF.placeholder).text);

    public void OnChangeDeltaWeb() =>
        fe.deltaWeb = int.TryParse(deltaWebIF.text, out _)
            ? int.Parse(deltaWebIF.text)
            : int.Parse(((TextMeshProUGUI)deltaWebIF.placeholder).text);

    public void OnChangeIsNeeded() => fe.needed = neededT.isOn;
}
