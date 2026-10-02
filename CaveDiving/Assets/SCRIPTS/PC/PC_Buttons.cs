using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class PC_Buttons : MonoBehaviour
{
    public enum ActionType { ActivateObject, WriteText }

    public ActionType action;

    [Header("If ActivateObject")]
    public GameObject target;
    public bool toggle;

    [Header("If WriteText")]
    public TMP_Text textField;
    [TextArea] public string message;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        switch (action)
        {
            case ActionType.ActivateObject:
                if (target == null) return;
                target.SetActive(toggle ? !target.activeSelf : true);
                break;

            case ActionType.WriteText:
                if (textField == null) return;
                textField.text = message;
                break;
        }
    }
}