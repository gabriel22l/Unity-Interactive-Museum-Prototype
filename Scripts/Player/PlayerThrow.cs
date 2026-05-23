using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerThrow : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InventoryController inventoryController;
    
    [SerializeField] private bool enableThrow = true;
    
    [SerializeField] private Transform throwOrigin;

    [SerializeField] private int projectileCount;
    [SerializeField] private float spreadAngle = 15f;

    [SerializeField] private float throwForce = 10;

    [SerializeField] private AudioEmitter audioEmitter;
    [SerializeField] private AudioClip[] throwClips;

    private void Awake()
    {
        playerInput.OnThrowInput += Throw;
    }
    private void Throw()
    {
        if (!enableThrow) return;

        itemDataSO itemData = inventoryController?.GetFirstItemOfType(ItemType.Throwable);
        if (itemData == null) return;
        
        inventoryController.RemoveResource(itemData, 1);
        for (int i = 0; i < projectileCount; i++)
        {
            float angle = Random.Range(-spreadAngle, spreadAngle);
            Vector3 direction = 
                i == 0 ? 
                throwOrigin.forward : 
                Quaternion.AngleAxis(angle, throwOrigin.up) * throwOrigin.forward;
            
            GameObject projectile = Instantiate(itemData.itemPrefab, throwOrigin.position, Quaternion.identity);
            projectile.transform.forward = direction;
            if (!projectile.TryGetComponent(out OfferingProjectile p))
            {
                Debug.LogError($"no projectile component found on {projectile.name}");
                continue;
            }
            
            p.Recoverable = projectileCount == 1;
            
            p.Initialize(throwForce, direction);
            if(throwClips != null && throwClips.Length > 0)
                audioEmitter?.PlayRandomSfx(throwClips, 0.3f);
        }
    }
}