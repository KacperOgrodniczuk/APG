using UnityEngine;
using TMPro;

public class Interactable : MonoBehaviour
{
    public GameObject floatingText;

    private string message = "RB / E to interact";

    private GameObject spawnedText;
    private bool inRange = false;
    private bool interacted = false;

    private float desThickness = 0.002f;

    private Vector3 textOffset = new Vector3(-0.5f, 3, 0);

    Material mat;

    private void Start()
    {
        mat = this.GetComponent<SpriteRenderer>().material;
    }

    private void Update()
    {
        if (!interacted && inRange)
        {
            mat.SetFloat("_Thickness", Mathf.PingPong(Time.time * desThickness, desThickness));
            if (Input.GetButton("Interact"))
            {
                Interact();
                interacted = true;
            }
        }
        else 
        {
            mat.SetFloat("_Thickness", 0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            inRange = true;
            if (!interacted)
            {
                spawnedText = Instantiate(floatingText, transform.position + textOffset, Quaternion.identity, transform);
                spawnedText.GetComponentInChildren<TextMeshPro>().text = message;
            }
            //Debug.Log("In range to interact");
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            inRange = false;
            if (spawnedText != null)
            {
                Destroy(spawnedText);
            }
            //Debug.Log("Out of range");
        }
    }

    public virtual void Interact()
    {
        //Debug.Log("Player has interacted");
    }
}
