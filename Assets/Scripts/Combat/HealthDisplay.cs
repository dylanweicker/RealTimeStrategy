using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Health))]
public class HealthDisplay : MonoBehaviour
{
    private Health health = null;
    [SerializeField] private GameObject healthBarParent = null;
    [SerializeField] private Image healthBarImage = null;
    private RectTransform healthBarTransform = null;


    //Runs even if script component is disabled
    private void Awake()
    {
        health = gameObject.GetComponent<Health>();
        health.ClientOnHealthUpdated += HandleHealthUpdated;
        healthBarTransform = healthBarImage.gameObject.GetComponent<RectTransform>();
    }

    private void OnDestroy()
    {
        health.ClientOnHealthUpdated -= HandleHealthUpdated;
    }

    private void HandleHealthUpdated(int currentHealth, int maxHealth)
    {
        healthBarTransform.anchorMax = new Vector2((float) currentHealth / maxHealth, 1f);

        if (currentHealth < maxHealth)
        {
            healthBarParent.SetActive(true);
        }
        else {
            healthBarParent.SetActive(false); 
        }
    }



}
