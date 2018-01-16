using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;
using RPG.Characters; // TODO - may re-architect

namespace RPG.CameraUI
{
    public class CameraRaycaster : MonoBehaviour
    {
        // INSPECTOR PROPERTIES RENDERED BY CUSTOM EDITOR SCRIPT
        [Tooltip("Layer mouse points to IN ORDER from top of list")]
        [SerializeField] int[] layerPriorities;

        [Tooltip("Determines which layers will NOT fade if blocking the player's view")]
        [SerializeField] int[] FadableLayers;

        float maxRaycastDepth = 100f;  // Hard coded value

        int topPriorityLayerLastFrame = -1; // So get ? from start with Default layer terrain

        private RaycastHit raycastHit;
        private Player player;
        private int FadeLayerMask = 0;

        // Set up the ability for another class to 'Subscribe' to layer-change events
        public delegate void OnCursorLayerChange(int newLayer);      // Declare new delegate type
        public event OnCursorLayerChange notifyLayerChangeObservers; // Instantiate an observer set

        public delegate void OnClickPriorityLayer(RaycastHit raycastHit, int layerHit); // declare new delegate type
        public event OnClickPriorityLayer notifyMouseClickObservers; // instantiate an observer set


        void Start()
        {
            player = FindObjectOfType<Player>();

            // Set up the layermask to ignore
            foreach (int layer in FadableLayers)
            {
                // This line shifts a binary bit of 1 left (int)layer times and
                // does a '|' (binary OR) to merge the bits with the previous - so for each bit,
                // if either or both a '1', the result is a '1'
                FadeLayerMask = FadeLayerMask | (1 << layer);
            }
        }

        void Update()
        {
            FindBlockingObject();

            // Check if pointer is over an interactable UI element
            if (EventSystem.current.IsPointerOverGameObject())
            {
                NotifyObserersIfLayerChanged(5);
                return; // Stop looking for other objects
            }

            // Raycast to max depth, every frame as things can move under mouse
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] raycastHits = Physics.RaycastAll(ray, maxRaycastDepth);

            RaycastHit? priorityHit = FindTopPriorityHit(raycastHits);
            if (!priorityHit.HasValue) // if hit no priority object
            {
                NotifyObserersIfLayerChanged(0); // broadcast default layer
                return;
            }

            // Notify delegates of layer change
            var layerHit = priorityHit.Value.collider.gameObject.layer;
            NotifyObserersIfLayerChanged(layerHit);

            // Notify delegates of highest priority game object under mouse when clicked
            if (Input.GetMouseButton(0))
            {
                notifyMouseClickObservers(priorityHit.Value, layerHit);
            }

        }

        void NotifyObserersIfLayerChanged(int newLayer)
        {
            if (newLayer != topPriorityLayerLastFrame)
            {
                topPriorityLayerLastFrame = newLayer;
                notifyLayerChangeObservers(newLayer);
            }
        }

        RaycastHit? FindTopPriorityHit(RaycastHit[] raycastHits)
        {
            // Form list of layer numbers hit
            List<int> layersOfHitColliders = new List<int>();
            foreach (RaycastHit hit in raycastHits)
            {
                layersOfHitColliders.Add(hit.collider.gameObject.layer);
            }

            // Step through layers in order of priority looking for a gameobject with that layer
            foreach (int layer in layerPriorities)
            {
                foreach (RaycastHit hit in raycastHits)
                {
                    if (hit.collider.gameObject.layer == layer)
                    {
                        return hit; // stop looking
                    }
                }
            }
            return null; // because cannot use GameObject? nullable
        }

        private void FindBlockingObject()
        {
            // Finds objects blocking LoS from Camera to Player

            // Define the Ray to cast - from camera to player
            Ray ray = new Ray(transform.position, player.transform.position - transform.position);
            Debug.DrawRay(transform.position, player.transform.position - transform.position);

            RaycastHit[] hits;
            // the ~ in front of notFadePlayerMask is a binary NOT
            hits = Physics.RaycastAll(ray, maxRaycastDepth, FadeLayerMask);
            foreach (RaycastHit hit in hits)
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
                Debug.Log("Adding Fader to: " + hitRenderer.gameObject.name);
                fader = hitRenderer.gameObject.AddComponent<Fader>();
            }
            fader.BeTransparent();
        }
    }
}
