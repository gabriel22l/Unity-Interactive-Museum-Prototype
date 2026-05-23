using System.Collections;
using UnityEngine;

public class ItemWorldObject : MonoBehaviour, IInteractable
{
    [SerializeField] private itemDataSO itemDataSo;
    [SerializeField] private AudioClip[] pickUpAudioClips;
    public int amount = 1;
    
    private Collider objCollider;
    
    private bool isInteractable = true;
    public bool IsInteractable => isInteractable;
    
    private void OnEnable()
    {
        TryGetComponent(out objCollider);
    }
    public void Interact(InteractionContext interactionContext)
    {
        InventoryController inventoryController = interactionContext.PlayerContext.InventoryController;
        if (inventoryController == null) return;

        GameObject player = interactionContext.PlayerContext.gameObject;
        
        int ogAmount = amount;
        int remainingAmount = inventoryController.AddItem(itemDataSo, amount);
        
        //play pick up sound
        if(remainingAmount < ogAmount && pickUpAudioClips != null && pickUpAudioClips.Length > 0) 
            interactionContext.PlayerContext.AudioEmitter.PlayRandomSfx(pickUpAudioClips, 0.5f);
        
        if (remainingAmount == 0)
        {
            if (player == null)
            {
                Destroy(gameObject);
            } else StartCoroutine(PickUpAnimDestroy(player.transform));
        }
        else amount = remainingAmount;
    }
    private IEnumerator PickUpAnimDestroy(Transform playerTransform)
    {
        if(objCollider != null) 
            objCollider.enabled = false; //disable collider to disable interaction once interaction is registered
        float duration = 0.2f;
        float timer = 0;
        Vector3 startPosition = transform.position;
        Vector3 startScale = transform.localScale;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            transform.position = Vector3.Lerp(startPosition, playerTransform.position + new Vector3(0, 1.5f, 0), t);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }
        Destroy(gameObject);
    }
    public void SetInteractable(bool interactable)
    {
        this.isInteractable = interactable;
    }
}
