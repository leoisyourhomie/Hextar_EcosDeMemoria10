using System.Collections;
using System.Collections.Generic;
using Expiria3DSpace;
using UnityEngine;
using UnityEngine.UI;

public class ScanMePanel : MonoBehaviour
{
    public Transform animatedLine;
    public RectTransform squareCenter;
    public GameObject panel;
    float animationSpeed = 0.4f; // Controls the speed of the animation

    private float topY;
    private float bottomY;

    ImageTracker imageTracker;

    void OnEnable()
    {
        Image image = squareCenter.GetComponent<Image>();

        imageTracker = FindObjectOfType<ImageTracker>(true);
        if (imageTracker != null)
        {
            Transform child = imageTracker.transform.Find("_TargetTrackingImagePreview");
            if (child != null)
            {
                image.sprite = child.GetComponent<Image>().sprite;
            }
        }

        // Calculate the top and bottom Y positions of the squareCenter
        Vector3[] corners = new Vector3[4];
        squareCenter.GetWorldCorners(corners);
        topY = corners[1].y; // Top-left corner's Y
        bottomY = corners[0].y; // Bottom-left corner's Y
    }

    void Update()
    {
        if (imageTracker == null)
            return;

        //if the imageTracker is enabled, disable the panel
        if (imageTracker.gameObject.activeInHierarchy)
        {
            panel.SetActive(false);
        }
        else
        {
            panel.SetActive(true);
        }

        // Calculate the interpolation factor using PingPong
        float t = Mathf.PingPong(Time.time * animationSpeed, 1f);
        // Interpolate the Y position between top and bottom
        float currentY = Mathf.Lerp(bottomY, topY, t);

        // Update the animated line's position
        animatedLine.position = new Vector3(
            animatedLine.position.x,
            currentY,
            animatedLine.position.z
        );
    }
}