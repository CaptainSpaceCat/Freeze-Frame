using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipZone : MonoBehaviour
{
    public GameObject tooltipText;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Player") {
            tooltipText.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.tag == "Player") {
            tooltipText.SetActive(false);
        }
    }
}
