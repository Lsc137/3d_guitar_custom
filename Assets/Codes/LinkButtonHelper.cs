using UnityEngine;

public class LinkButtonHelper : MonoBehaviour
{
    public void OpenURL(string urlToOpen)
    {
        Application.OpenURL(urlToOpen);
    }
}