using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            float interactionRange = 2f;
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactionRange);
            foreach(Collider collider in colliderArray)
            {
                if(collider.TryGetComponent(out NPCInteraction npcInteraction)){
                    npcInteraction.Interact();
                }
            }
        } 
    }
}
