using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
    public class PlaceOnPlane : MonoBehaviour
    {
        // import model
        [SerializeField] GameObject m_PlacedPrefab;
        UnityEvent placementUpdate;

        //import marker
        [SerializeField] GameObject visualObject;

        //import material
        [SerializeField] public Texture2D textureUse;
        
        //import ar camera
        [SerializeField] private Camera ARCamera;
        
        //import vector camera
        public Vector3 Axis { set { axis = value; } get { return axis; } } [SerializeField] private Vector3 axis = Vector3.down;
        
        private ARPlaneManager aRPlaneManager;
        
        bool IsRotation = false;
        
        private GameObject SelectetObject;
        public GameObject placedPrefab
        {
            get { return m_PlacedPrefab; }
            set { m_PlacedPrefab = value; }
        }
        //spawned object
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
                // if object not placed
                if (spawnedObject == null)
                {
                    //create hits
                    List<ARRaycastManager> hits = new List<ARRaycastManager>();
                    
                    //set tag unselected
                    m_PlacedPrefab.gameObject.tag = "UnSelected";
                    
                    //set rigidbody
                    Rigidbody idbody = m_PlacedPrefab.gameObject.AddComponent<Rigidbody>();
                    idbody.useGravity = false;
                    idbody.isKinematic = true;
                    
                    //set boxcollider
                    BoxCollider collider = m_PlacedPrefab.gameObject.AddComponent<BoxCollider>();
                    
                    //resize collider
                    MeshRenderer renderer = m_PlacedPrefab.gameObject.AddComponent<MeshRenderer>();
                    collider.center = renderer.bounds.center;
                    collider.size = renderer.bounds.size;
                    
                    //center ray to place
                    m_RaycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), s_Hits,
                        TrackableType.Planes);
                    spawnedObject = Instantiate(m_PlacedPrefab, s_Hits[0].pose.position, hitPose.rotation);
                    
                    //disable visual plane
                    aRPlaneManager = GetComponent<ARPlaneManager>();
                    aRPlaneManager.enabled = false;
                    foreach (var plane in aRPlaneManager.trackables)
                    {
                        plane.gameObject.SetActive(false);
                    }

                    //reset boxcollider and rigidbody in spawnedobject
                    BoxCollider collider1 = spawnedObject.AddComponent<BoxCollider>();
                    MeshRenderer renderer1 = spawnedObject.gameObject.AddComponent<MeshRenderer>();
                    collider1.center = renderer1.bounds.center;
                    collider1.size = renderer1.bounds.size;
                    Rigidbody idbody1 = spawnedObject.AddComponent<Rigidbody>();
                }
                placementUpdate.Invoke();
            }
            //move object
            MoveObject();
        }
        
        void MoveObject()
        {
            if (Input.touchCount > 0)
            {
                //get touch count and position
                Touch touch = Input.GetTouch(0);
                var touchPosition = touch.position;
                
               
                
                
                //if phase began
                if (touch.phase == TouchPhase.Began)
                {
                    //test texturing
                    Material mat0 = Resources.Load<Material>("Materials/Velour_BaseColor");
                    for (int i = 0; i < spawnedObject.transform.childCount; i++)
                    {
                        spawnedObject.transform.GetChild(i).GetComponent<MeshRenderer>().material = mat0;
                    }

                    //create ray hit
                    Ray ray = ARCamera.ScreenPointToRay(touch.position);
                    RaycastHit hitObject;
                    
                    //if ray hit a object
                    if (Physics.Raycast(ray, out hitObject))
                    {
                        //set tag
                        if (hitObject.collider.CompareTag("UnSelected"))
                        {
                            hitObject.collider.gameObject.tag = "Selected";
                        }
                    }    
                }
                
                // move rotation
                if (Input.touchCount == 2)
                {
                    //get finger and angle
                    var finger = Lean.Touch.LeanTouch.Fingers; 
                    var twistDegrees = Lean.Touch.LeanGesture.GetTwistDegrees(finger) * 1;
                    Touch touch1 = Input.touches[0];
                    Touch touch2 = Input.touches[1];
                    
                    //rotate object
                    m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.Planes);
                    SelectetObject = GameObject.FindWithTag("Selected");
                    SelectetObject.transform.Rotate(axis, twistDegrees);
                    
                    //block set position while rotated
                    IsRotation = true;
                }
                
                //move
                if (touch.phase == TouchPhase.Moved && Input.touchCount == 1 && IsRotation == false)
                {
                    //get object and transform position
                    m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.Planes);
                    SelectetObject = GameObject.FindWithTag("Selected");
                    SelectetObject.transform.position = s_Hits[0].pose.position;
                }
                
                //set tag "UnSelected"
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
