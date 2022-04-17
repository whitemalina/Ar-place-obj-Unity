using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
    public class PlaceOnPlane : MonoBehaviour
    {
        
        [SerializeField] GameObject m_PlacedPrefab;

        UnityEvent placementUpdate;

        [SerializeField] GameObject visualObject;

        /// <summary>
        /// The prefab to instantiate on touch.
        /// </summary>

        [SerializeField] private Camera ARCamera;
        public Vector3 Axis { set { axis = value; } get { return axis; } } [SerializeField] private Vector3 axis = Vector3.down;
        
        private ARPlaneManager aRPlaneManager;
        
        bool IsRotation = false;
        
        private GameObject SelectetObject;
        public GameObject placedPrefab
        {
            get { return m_PlacedPrefab; }
            set { m_PlacedPrefab = value; }
        }
 
        public GameObject spawnedObject { get; private set; }
        
        void Awake()
        {
            m_RaycastManager = GetComponent<ARRaycastManager>();
            
            if (placementUpdate == null)
                placementUpdate = new UnityEvent();
            
            placementUpdate.AddListener(DiableVisual);
        }
        
        bool TryGetTouchPosition(out Vector2 touchPosition)
        {
            if (Input.touchCount > 0)
            {
                touchPosition = Input.GetTouch(0).position;
                return true;
            }
            IsRotation = false;
            
            touchPosition = default;
            return false;
        }

        void Update()
        {
            if (!TryGetTouchPosition(out Vector2 touchPosition))
                return;

            if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
            {
                var hitPose = s_Hits[0].pose;
            
                if (spawnedObject == null)
                {
                    List<ARRaycastManager> hits = new List<ARRaycastManager>();
                    
                    m_RaycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), s_Hits,
                        TrackableType.Planes);
                    
                    spawnedObject = Instantiate(m_PlacedPrefab, s_Hits[0].pose.position, hitPose.rotation);
                    
                    aRPlaneManager = GetComponent<ARPlaneManager>();

                    aRPlaneManager.enabled = false;
                    
                    foreach (var plane in aRPlaneManager.trackables)
                    {
                        plane.gameObject.SetActive(false);
                    }
                }
                placementUpdate.Invoke();
            }

            MoveObject();
        }
        
        void MoveObject()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                var touchPosition = touch.position;
                
                if (touch.phase == TouchPhase.Began)
                {
                    Ray ray = ARCamera.ScreenPointToRay(touch.position);
                    RaycastHit hitObject;
        
                    if (Physics.Raycast(ray, out hitObject))
                    {
                        if (hitObject.collider.CompareTag("UnSelected"))
                        {
                            hitObject.collider.gameObject.tag = "Selected";
                        }
                    }    
                }

                if (Input.touchCount == 2)
                {
                    var finger = Lean.Touch.LeanTouch.Fingers; 
                    var twistDegrees = Lean.Touch.LeanGesture.GetTwistDegrees(finger) * 1;
                    Touch touch1 = Input.touches[0];
                    Touch touch2 = Input.touches[1];
                    
                    m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.Planes);
                    SelectetObject = GameObject.FindWithTag("Selected");
                    SelectetObject.transform.Rotate(axis, twistDegrees);

                    IsRotation = true;
                }
                
                if (touch.phase == TouchPhase.Moved && Input.touchCount == 1 && IsRotation == false)
                {
                    m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.Planes);
                    SelectetObject = GameObject.FindWithTag("Selected");
                    SelectetObject.transform.position = s_Hits[0].pose.position;
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    if (SelectetObject.CompareTag("Selected"))
                    {
                        SelectetObject.tag = "UnSelected";
                    }
                }
            }
        }

        public void DiableVisual()
        {
            visualObject.SetActive(false);
        }

        static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
        ARRaycastManager m_RaycastManager;
    }