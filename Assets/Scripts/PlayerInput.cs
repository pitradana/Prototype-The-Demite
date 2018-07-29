using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    public LayerMask targetLayerMask;
    public AudioSource shootingClip;
    public Slider reloadIndicator;

    private Rect inputRect;
    private float originalPitch;
    private float pitchVariance = 0.1f;
    private Camera cam;

    void Start()
    {
        inputRect = new Rect(Screen.width / 2, 0, Screen.width, Screen.height * 0.75f);
        cam = GetComponentInChildren<Camera>();

        originalPitch = shootingClip.pitch;
    }

    void Enable()
    {
        reloadIndicator.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (GameManager.instance.gameState == GameState.Running)
        {
            TouchInput();

#if UNITY_EDITOR
            KeyboardInput();
#endif
        }
    }

    private void TouchInput()
    {
        if(Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase != TouchPhase.Began)
                    continue;

                if (!inputRect.Contains(touch.position))
                    continue;

                Shoot();
            }
        }
    }

#if UNITY_EDITOR
    private void KeyboardInput()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }
#endif

    private void Shoot()
    {
        //only shoot when has clip finished playing
        if (shootingClip.isPlaying)
            return;

        StartCoroutine(ReloadIndicator());

        //set random pitch and play audio
        shootingClip.pitch = Random.Range(originalPitch - pitchVariance, originalPitch + pitchVariance);
        shootingClip.Play();

        //check for target
        RaycastHit hit;
        if(Physics.Raycast(cam.transform.position, cam.transform.forward,out hit, 100f,targetLayerMask))
        {
            Target target = hit.collider.GetComponentInParent<Target>();
            target.Hit(hit);
        }
    }

    private IEnumerator ReloadIndicator()
    {
        reloadIndicator.gameObject.SetActive(true);

        do
        {
            float t = shootingClip.time / shootingClip.clip.length;
            reloadIndicator.value = t;

            yield return new WaitForEndOfFrame();
        } while (shootingClip.isPlaying);

        reloadIndicator.gameObject.SetActive(false);
    }
}
