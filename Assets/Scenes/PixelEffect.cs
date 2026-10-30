using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//On a UI image prefab. flips through the pixel art frames then destroys itself

[RequireComponent(typeof(Image))]
public class PixelEffect : MonoBehaviour
{
    [Header("Inscribed")]
    [Tooltip("Animation frames in order")]
    public Sprite[] frames;

    [Tooltip("Frames shown per second")]
    public float framesPerSecond = 10f;

    [Tooltip("How many times the animation loops")]
    public int loops = 1;

    private Image image;

    //how long the animation lasts so everything else waits
    public float Duration 
    {
        get
        {
            if (frames == null || framesPerSecond <= 0f) return 0f;
            return frames.Length * loops / framesPerSecond;
        }
    }

    void Awake()
    {
        image = GetComponent<Image>();

        //doesnt allow effects to block button presses
        //even though the animations wont normally be that low
        image.raycastTarget = false;
        image.preserveAspect = true;
    }

    void Start()
    {
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        if (frames == null || frames.Length == 0 || framesPerSecond <= 0f)
        {
            Destroy(gameObject);
            yield break;
        }

        float frameTime = 1f / framesPerSecond;

        for (int loop = 0; loop < loops; loop++)
        {
            foreach (Sprite frame in frames)
            {
                image.sprite = frame;
                yield return new WaitForSeconds(frameTime);
            }
        }

        Destroy(gameObject);
    }
}
