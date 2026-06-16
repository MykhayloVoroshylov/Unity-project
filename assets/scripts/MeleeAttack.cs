//MeleeAttack.cs

using UnityEngine;
using UnityEngine.UI; // REQUIRED FOR DYNAMIC UI IMAGES
using System.Collections;

public class MeleeAttack : MonoBehaviour
{
    [Header("Melee Stats")]
    public float knifeRange = 3f;
    public float rifleBashRange = 2f; // Slightly shorter range than a blade stab
    public int knifeDamage = 25;
    public int bashDamage = 15; // Lower damage fallback strike

    [Header("References")]
    public Camera playerCamera;
    public RawImage crosshairUI;

    [Header("Crosshair Colors")]
    public Color neutralColor = Color.white;
    public Color targetInMeleeRangeColor = Color.red;

    [Header("Knife Procedural Animation")]
    public Transform knifeTransform;
    public float animationSpeed = 12f;
    
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isAttacking = false;

    // Fake Ammo flag for milestone testing until gun system is fully wired
    [Header("Weapon States (Testing)")]
    public bool isRifleAmmoEmpty = false; 

    void Start()
    {
        if (knifeTransform != null)
        {
            originalPosition = knifeTransform.localPosition;
            originalRotation = knifeTransform.localRotation;
        }
    }

    void Update()
    {
        // 1. Constantly probe the target in front to update UI indicator colors
        UpdateCrosshairColor();

        // 2. Handle Inputs
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            DetermineAndExecuteAttack();
        }
    }

    void UpdateCrosshairColor()
    {
        if (crosshairUI == null || playerCamera == null) return;

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, knifeRange))
        {
            ZombieHealth zombie = hit.collider.GetComponentInParent<ZombieHealth>();
            
            if (zombie != null)
            {
                // Set target color
                crosshairUI.color = targetInMeleeRangeColor;
                
                // JUICE: Make it slightly bigger when locked on so you CANNOT miss it!
                crosshairUI.rectTransform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                return;
            }
        }

        // Default neutral color and standard native scale
        crosshairUI.color = neutralColor;
        crosshairUI.rectTransform.localScale = Vector3.one;
    }

    void DetermineAndExecuteAttack()
    {
        // If out of ammunition, context-switch seamlessly to the heavy weapon slam
        if (isRifleAmmoEmpty)
        {
            Debug.Log("Out of ammo! Triggering Rifle Butt Bash!");
            StartCoroutine(AnimateRifleBash());
            ExecuteRaycastHit(rifleBashRange, bashDamage, "Rifle Butt Bash");
        }
        else
        {
            // Default fast tactical knife slash
            StartCoroutine(AnimateKnifeSlash());
            ExecuteRaycastHit(knifeRange, knifeDamage, "Knife Slash");
        }
    }

    void ExecuteRaycastHit(float range, int damageValue, string attackName)
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, range))
        {
            ZombieHealth zombie = hit.collider.GetComponentInParent<ZombieHealth>();
            if (zombie != null)
            {
                Debug.Log($"[{attackName}] landed cleanly on: {hit.collider.name}");
                zombie.TakeDamage(damageValue);
            }
        }
    }

    // Snappy, clean slicing swoop motion
    private IEnumerator AnimateKnifeSlash()
    {
        isAttacking = true;
        Vector3 strikePosition = originalPosition + new Vector3(-0.1f, -0.1f, 0.4f); 
        Quaternion strikeRotation = originalRotation * Quaternion.Euler(45f, -30f, 10f);

        float progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime * animationSpeed;
            if (knifeTransform != null)
            {
                knifeTransform.localPosition = Vector3.Lerp(originalPosition, strikePosition, progress);
                knifeTransform.localRotation = Quaternion.Lerp(originalRotation, strikeRotation, progress);
            }
            yield return null;
        }

        progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime * animationSpeed;
            if (knifeTransform != null)
            {
                knifeTransform.localPosition = Vector3.Lerp(strikePosition, originalPosition, progress);
                knifeTransform.localRotation = Quaternion.Lerp(strikeRotation, originalRotation, progress);
            }
            yield return null;
        }
        isAttacking = false;
    }

    // Heavier, slower forward-shoving blunt-force animation behavior
    private IEnumerator AnimateRifleBash()
    {
        isAttacking = true;
        // Shove the weapon root straight forward aggressively along the Z depth axis
        Vector3 strikePosition = originalPosition + new Vector3(0f, 0.1f, 0.5f);
        Quaternion strikeRotation = originalRotation * Quaternion.Euler(-15f, 15f, -10f);

        float progress = 0f;
        // Slower swing speed to give the strike an impactful, heavy mass weight profile
        while (progress < 1f)
        {
            progress += Time.deltaTime * (animationSpeed * 0.7f);
            if (knifeTransform != null)
            {
                knifeTransform.localPosition = Vector3.Lerp(originalPosition, strikePosition, progress);
                knifeTransform.localRotation = Quaternion.Lerp(originalRotation, strikeRotation, progress);
            }
            yield return null;
        }

        progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime * (animationSpeed * 1.2f); // Snaps back quicker
            if (knifeTransform != null)
            {
                knifeTransform.localPosition = Vector3.Lerp(strikePosition, originalPosition, progress);
                knifeTransform.localRotation = Quaternion.Lerp(strikeRotation, originalRotation, progress);
            }
            yield return null;
        }
        isAttacking = false;
    }
}