using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    public Texture2D crossTexture; 
    public AudioClip shootSound; 
    private AudioSource audioSource;
    private Camera mainCamera;

    void Start()
    {
        Cursor.visible = false;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = shootSound;
        mainCamera = Camera.main;
    }

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -mainCamera.transform.position.z;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePos);
        transform.position = new Vector3(worldPosition.x, worldPosition.y, 0);

        if (Input.GetMouseButtonDown(0)) 
        {
            Shoot();
        }
    }

    void Shoot()
    {
        PlayShootSound();

        Vector2 shootPosition = transform.position;
        float detectionRadius = 1f; 

        Debug.Log($"Disparando en posici�n real: {shootPosition}");

        Debug.DrawLine(shootPosition, shootPosition + Vector2.up * 0.5f, Color.red, 1f);
        Debug.DrawLine(shootPosition, shootPosition + Vector2.right * 0.5f, Color.red, 1f);

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(shootPosition, detectionRadius);

        Debug.Log($"Objetos detectados: {hitColliders.Length}");

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider != null)
            {
                Debug.Log($"Objeto detectado: {hitCollider.gameObject.name} en posici�n: {hitCollider.transform.position}");

                if (hitCollider.CompareTag("Elephant"))
                {
                    Debug.Log("�Elefante golpeado!");
                    Game2Manager.Instance.UpdateScore(20);
                    Destroy(hitCollider.gameObject);
                    break;
                }
                else if (hitCollider.CompareTag("Princess"))
                {
                    Debug.Log("�Princesa golpeada!");
                    Game2Manager.Instance.UpdateScore(-10);
                    Destroy(hitCollider.gameObject);
                    break;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }

    void OnGUI()
    {
        if (crossTexture != null)
        {
            Vector2 mousePosition = Input.mousePosition;
            float size = 50f; 
            Rect rect = new Rect(mousePosition.x - size / 2, Screen.height - mousePosition.y - size / 2, size, size);
            GUI.DrawTexture(rect, crossTexture);
        }
    }

    private void PlayShootSound()
    {
        if (audioSource != null && shootSound != null)
        {
            audioSource.Play();
        }
    }
}