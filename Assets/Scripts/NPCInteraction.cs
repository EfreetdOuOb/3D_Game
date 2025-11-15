using UnityEngine;

public class NPCInteraction : MonoBehaviour
{ 
    public GameObject KeyInfo;
    public GameObject TextPanel; 
    

    public void Interact()
    {
        TextPanel.SetActive(true);
    }
}
