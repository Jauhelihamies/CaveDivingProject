using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.InputSystem;
public class MainMenuUI: MonoBehaviour
{
    public ImageFader StartAlarmSystem;
    public MainMenu GetFleeCode;


    public void Update()
    {

        Camera cam = Camera.main;

        Vector2 mousepos = Mouse.current.position.ReadValue();

        Ray mouseRay = cam.ScreenPointToRay(mousepos);
        RaycastHit hitInfo = new RaycastHit();
        if (Physics.Raycast(mouseRay, out hitInfo))
        {
            if (hitInfo.collider.gameObject.CompareTag("Start"))
            {
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    StartAlarmSystem.StartGame();
                }
            }
            if (hitInfo.collider.gameObject.CompareTag("Quit"))
            {
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    GetFleeCode.Flee();
                }
            }
        }
    }
}