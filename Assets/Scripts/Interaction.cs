using UnityEngine;

public class Interaction : MonoBehaviour
{ 
    public GameObject KeyInfo;
    public GameObject TextPanel;

    public bool isPressedE = false;

    void Awake()
    {
        KeyInfo.SetActive(false);
        TextPanel.SetActive(false);
    }


    void Update()
    {
         if((KeyInfo.activeSelf || isPressedE) && Input.GetKeyDown(KeyCode.E))
        {
            // 切換狀態
            isPressedE = !isPressedE;
            TextPanel.SetActive(isPressedE);
            
            // 如果需要在打開視窗時隱藏提示，可以加這行：
            // KeyInfo.SetActive(!isPressedE); 
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
