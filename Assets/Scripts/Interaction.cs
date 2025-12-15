using UnityEngine;

public class Interaction : MonoBehaviour
{ 
    public GameObject KeyInfo;
    public GameObject TextPanel;

    public bool isPressedE = false;
    public bool isPlayerInside = false;

    void Awake()
    {
        KeyInfo.SetActive(false);
        this.TextPanel.SetActive(false);
    }


    void Update()
    {
         if((KeyInfo.activeSelf || isPressedE) && Input.GetKeyDown(KeyCode.E))
        {
            // 切換狀態
            isPressedE = !isPressedE;
            this.TextPanel.SetActive(isPressedE);
            
            // 如果需要在打開視窗時隱藏提示，可以加這行：
            // KeyInfo.SetActive(!isPressedE); 
        }   
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            KeyInfo.SetActive(true);
        }
        
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            KeyInfo.SetActive(false);
        }
        
        this.TextPanel.SetActive(false);
        isPressedE = false;
    }
}
