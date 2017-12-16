using UnityEngine;

public class CameraRaycaster : MonoBehaviour
{
    public Layer[] layerPriorities = {
        Layer.Enemy,
        Layer.Walkable
    };

    [SerializeField] float distanceToBackground = 100f;

    public Layer[] unFadableLayers = {
        Layer.Enemy,
        Layer.Walkable,
        Layer.Player
    };

    

    private Camera viewCamera;
    private RaycastHit raycastHit;
    private Player player;
    private float cameraToPlayerDistance;
    private int notFadeLayerMask = 0;


    public RaycastHit hit
    {
        get { return raycastHit; }
    }

    Layer layerHit;
    public Layer currentLayerHit
    {
        get { return layerHit; }
    }

    // Set up the ability for another class to 'Subscribe' to layer-change events
    public delegate void OnLayerChange(Layer newLayer);      // Declare new delegate type
    public event OnLayerChange layerChangeObservers; // Instantiate an observer set

    void Start() 
    {
        viewCamera = Camera.main;
        player = FindObjectOfType<Player>();
        cameraToPlayerDistance = (player.transform.position - transform.position).magnitude;

        // Set up the layermask to ignore
        foreach (Layer layer in unFadableLayers)
        {
            // This line shifts a binary bit of 1 left (int)layer times and
            // does a '|' (binary OR) to merge the bits with the previous - so for each bit,
            // if either or both a '1', the result is a '1'
            notFadeLayerMask = notFadeLayerMask | (1 << (int)layer);
        }
    }

    void Update()
    {
        FindBlockingObject();

        // Look for and return priority layer hit
        foreach (Layer layer in layerPriorities)
        {
            var hit = RaycastForLayer(layer);
            if (hit.HasValue)
            {
                raycastHit = hit.Value;
                if (layerHit != layer)
                {
                    layerHit = layer;
                    layerChangeObservers(layerHit);      // Call ALL of the Delegates (ie broadcast to subscribers) 
                }
                return;
            }
        }

        // Otherwise return background hit
        raycastHit.distance = distanceToBackground;
        layerHit = Layer.RaycastEndStop;
    }

    RaycastHit? RaycastForLayer(Layer layer)
    {
        int layerMask = 1 << (int)layer; // See Unity docs for mask formation
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit; // used as an out parameter
        bool hasHit = Physics.Raycast(ray, out hit, distanceToBackground, layerMask);
        if (hasHit)
        {
            return hit;
        }
        return null;
    }

    private void FindBlockingObject()
    {
        // Ray ray = viewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        // Ray ray = viewCamera.ViewportPointToRay(player.transform.position);

        // Define the Ray to cast - from camera to player
        Ray ray = new Ray(transform.position, player.transform.position - transform.position);
        Debug.DrawRay(transform.position, player.transform.position - transform.position);

        RaycastHit[] hits;
        // the ~ in front of notFadePlayerMask is a binary NOT
        hits = Physics.RaycastAll(ray, distanceToBackground, ~notFadeLayerMask);
        foreach(RaycastHit hit in hits)
        {
            HandleFade(hit);
        }
    }

    private static void HandleFade(RaycastHit hit)
    {
        Renderer hitRenderer = hit.transform.gameObject.GetComponent<Renderer>();

        if (hitRenderer == null) { return; } // skip if no renderer present

        Fader fader = hitRenderer.GetComponent<Fader>();
        if (fader == null) // fader script not attached to object hit
        {
            fader = hitRenderer.gameObject.AddComponent<Fader>();
        }
        fader.BeTransparent();
    }
}
