using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using RPG.Characters; // TODO - may re-architect
using System;

namespace RPG.CameraUI
{
    public class CameraRaycaster : MonoBehaviour
    {
        [Tooltip("Determines which layers will NOT fade if blocking the player's view")]
        [SerializeField] int[] FadableLayers;

        [SerializeField] Texture2D walkCursor = null;
        [SerializeField] Texture2D targetCursor = null;
        [SerializeField] Vector2 cursorHotspot = new Vector2(4, 4);

        const int WALKABLE_LAYER = 8;
        float maxRaycastDepth = 100f;  // Hard coded value

        private Player player;   // Used for fader
        private int FadeLayerMask = 0;  // Used for fader

        // Delegates allow other class to 'Subscribe' to them
        public delegate void OnMouseOverEnemy(Enemy enemy);     // Declare new delegate type
        public event OnMouseOverEnemy onMouseOverEnemy;         // Instantiate an observer set

        public delegate void OnMouseOverTerrain(Vector3 destination);
        public event OnMouseOverTerrain onMouseOverWalkable;

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
                // TODO Implement UI Imteraction
            }
            else
            {
                PerformRaycasts();
            }
        }

        void PerformRaycasts()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Specify Layer Priorities Here
            if ( RaycastForEnemy(ray) )    { return; }
            if ( RaycastForWalkable(ray) ) { return; }
        }
        
        private bool RaycastForEnemy(Ray ray)
        {
            // TODO - inadvertantly hits audio, stopping enemy from being targettable
            RaycastHit hitInfo;
            Physics.Raycast(ray, out hitInfo, maxRaycastDepth);

            GameObject gameObjectHit = hitInfo.collider.gameObject;
            Enemy enemyHit = gameObjectHit.GetComponent<Enemy>();
            if (enemyHit)
            {
                Cursor.SetCursor(targetCursor, cursorHotspot, CursorMode.Auto);
                onMouseOverEnemy(enemyHit);  // Broadcast Mouse over Enemy
                return true;
            }                
            return false;
        }

        private bool RaycastForWalkable(Ray ray)
        {
            // Raycast for Walkable Layer
            int layerMask = 0;
            layerMask = layerMask | (1 << WALKABLE_LAYER);
            RaycastHit hitInfo;
            bool isWalkable = Physics.Raycast(ray, out hitInfo, maxRaycastDepth, layerMask);

            if (isWalkable)
            {
                Cursor.SetCursor(walkCursor, cursorHotspot, CursorMode.Auto);
                onMouseOverWalkable(hitInfo.point);   // Broadcast Mouse over Walkable
            }
            return isWalkable;            
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
