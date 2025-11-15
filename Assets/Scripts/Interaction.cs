using UnityEngine;

public class Interaction : MonoBehaviour
{ 
    public GameObject KeyInfo;
    public GameObject TextPanel;

    void Awake()
    {
        KeyInfo.SetActive(false);
        TextPanel.SetActive(false);
    }


    void Update()
    {
        if(KeyInfo.activeSelf && Input.GetKeyDown(KeyCode.E))
        {
            TextPanel.SetActive(true);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        KeyInfo.SetActive(true);
    }

    public void OnTriggerExit(Collider other)
    {
        KeyInfo.SetActive(false);
        TextPanel.SetActive(false);
    }
}
