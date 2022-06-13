using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using TriLibCore;
using UnityEditor;
// using UnityEditor.VersionControl;
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

    private string testPath = "/data/user/0/com.AllinReality.ARFoundationHitTest/1.glb";

    //import marker

    [SerializeField] GameObject visualObject;

    //import material
    [SerializeField] public Texture2D textureUse;

    //import ar camera
    [SerializeField] private Camera ARCamera;

    public string modelTexture = null;

    //import vector camera
    public Vector3 Axis
    {
        set { axis = value; }
        get { return axis; }
    }

    [SerializeField] private Vector3 axis = Vector3.down;

    private ARPlaneManager aRPlaneManager;

    bool IsRotation = false;

    // public List<GameObject> objectsInside = new List<GameObject>();
    public GameObject objectsInside;
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
        // loadModel(testPath);

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

    private Texture2D BaseColor = null;
    private Texture2D NormalMap = null;
    private Texture2D HeightMap = null;
    private Texture2D SpecularMap = null;
    private Texture2D OcclusionMap = null;
    private Texture2D EmissionMap = null;
    private Texture2D MetallicGlossMap = null;

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
                //spawnedObject = Instantiate(m_PlacedPrefab, s_Hits[0].pose.position, hitPose.rotation);
                var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
                
                
                Debug.Log("testPathOnPlace = " + testPath);
                AssetLoader.LoadModelFromFile(testPath, delegate(AssetLoaderContext assetLoaderContext)
                {
                    Vector3 vec;
                    vec.x = 99999;
                    vec.y = 99999;
                    vec.z = 99999;

                    assetLoaderContext.RootGameObject.transform.position = vec;

                    //set boxcollider
                    if (!assetLoaderContext.RootGameObject.GetComponent<BoxCollider>())
                    {
                        BoxCollider bxcollider2 = assetLoaderContext.RootGameObject.AddComponent<BoxCollider>();
                    }

                    if (!assetLoaderContext.RootGameObject.gameObject.GetComponent<Rigidbody>())
                    {
                        Rigidbody rbody = assetLoaderContext.RootGameObject.gameObject.AddComponent<Rigidbody>();
                        rbody.useGravity = false;
                        rbody.isKinematic = true;
                    }

                    assetLoaderContext.RootGameObject.gameObject.tag = "UnSelected";


                    //loadModel("s");



                }, delegate(AssetLoaderContext assetLoaderContext)
                {


                    if (!assetLoaderContext.RootGameObject.GetComponent<BoxCollider>())
                    {
                        BoxCollider bxcollider = assetLoaderContext.RootGameObject.AddComponent<BoxCollider>();
                    }

                    if (!assetLoaderContext.RootGameObject.gameObject.GetComponent<Rigidbody>())
                    {
                        Rigidbody rbody = assetLoaderContext.RootGameObject.gameObject.AddComponent<Rigidbody>();
                        rbody.useGravity = false;
                        rbody.isKinematic = true;
                    }
                    //spawnedObject = assetLoaderContext.RootGameObject;


                    // Rigidbody idbody22 = assetLoaderContext.RootGameObject.gameObject.AddComponent<Rigidbody>();
                    // idbody22.useGravity = false;
                    // idbody22.isKinematic = true;
                    //
                    // //set boxcollider
                    // BoxCollider collider22 = assetLoaderContext.RootGameObject.gameObject.AddComponent<BoxCollider>();
                    //
                    // //resize collider
                    // MeshRenderer renderer22 = assetLoaderContext.RootGameObject.gameObject.AddComponent<MeshRenderer>();
                    // collider22.center = renderer22.bounds.center;
                    // collider22.size = renderer22.bounds.size;

                    //var pos1 = spawnedObject.transform.position;
                    //var rot1 = spawnedObject.transform.rotation;
                    //Destroy(spawnedObject);
                    assetLoaderContext.RootGameObject.gameObject.tag = "UnSelected";
                    spawnedObject = Instantiate(assetLoaderContext.RootGameObject.gameObject, s_Hits[0].pose.position,
                        hitPose.rotation);
                    spawnedObject.gameObject.tag = "UnSelected";


                    if (!spawnedObject.GetComponent<BoxCollider>())
                    {
                        BoxCollider collider12 = spawnedObject.AddComponent<BoxCollider>();
                    }

                    if (!spawnedObject.gameObject.GetComponent<Rigidbody>())
                    {
                        Rigidbody rbody2 = spawnedObject.gameObject.AddComponent<Rigidbody>();
                        rbody2.useGravity = false;
                        rbody2.isKinematic = true;
                    }

                    spawnedObject.gameObject.tag = "UnSelected";
                    //Debug.Log("texturePathOnPlace = " + modelTexture);
                    //LoadTexture(modelTexture);

                }, null, null, null, assetLoaderOptions, null);
                aRPlaneManager = GetComponent<ARPlaneManager>();
                aRPlaneManager.enabled = false;
                foreach (var plane in aRPlaneManager.trackables)
                {
                    plane.gameObject.SetActive(false);
                }

                spawnedObject.gameObject.tag = "UnSelected";
                //loadTexture

                //disable visual plane
                // aRPlaneManager = GetComponent<ARPlaneManager>();
                // aRPlaneManager.enabled = false;
                // foreach (var plane in aRPlaneManager.trackables)
                // {
                //     plane.gameObject.SetActive(false);
                // }

                //reset boxcollider and rigidbody in spawnedobject
                // BoxCollider collider1 = spawnedObject.AddComponent<BoxCollider>();
                // MeshRenderer renderer1 = spawnedObject.gameObject.AddComponent<MeshRenderer>();
                // collider1.center = renderer1.bounds.center;
                // collider1.size = renderer1.bounds.size;
                // Rigidbody idbody1 = spawnedObject.AddComponent<Rigidbody>();
            }

            placementUpdate.Invoke();
        }

        //move object
        MoveObject();
        if (spawnedObject != null)
        {
            aRPlaneManager = GetComponent<ARPlaneManager>();
            aRPlaneManager.enabled = false;
            foreach (var plane in aRPlaneManager.trackables)
            {
                plane.gameObject.SetActive(false);
            }
        }
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
                //loadModel("s");

                // for (int i = 0; i < spawnedObject.transform.childCount; i++)
                // {
                //     spawnedObject.transform.GetChild(i).GetComponent<MeshRenderer>().material = mat0;
                // }
                for (int i = 0; i < spawnedObject.transform.childCount; i++)
                {
                    spawnedObject.transform.GetChild(i).GetComponent<MeshRenderer>().material
                        .SetTexture("_MainTex", BaseColor);
                    spawnedObject.transform.GetChild(i).GetComponent<MeshRenderer>().material
                        .SetTexture("_BumpMap", NormalMap);
                    spawnedObject.transform.GetChild(i).GetComponent<MeshRenderer>().material
                        .SetTexture("_ParallaxMap", HeightMap);

                }


                Debug.Log("Model textured");

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
                Debug.Log("Rotation move");

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
                Debug.Log("Move model");

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
            Debug.Log("Visual disabled");

        }

        public void ModelTexture(string path)
        {
            modelTexture = path;
            Debug.Log("modelTextureLoad = " + modelTexture);
        }

        public void LoadModel(string filePath)
        {
            testPath = filePath;
            Debug.Log("modelLoad = " + testPath);
        }

        public async void LoadTexture(string allPath)
        {
            BaseColor = null;
            NormalMap = null;
            HeightMap = null;
            // SpecularMap = null;
            OcclusionMap = null;
            EmissionMap = null;
            MetallicGlossMap = null;
            float Glossiness = 0;

            enableMap("_METALLICGLOSSMAP");
            enableMap("_NORMALMAP");
            enableMap("_PARALLAXMAP");

            //_MainTex
            if ((allPath.Split(", ")[0]) != ("null")){

                String texName = "_MainTex";
                
                BaseColor = LoadTextureData(allPath.Split(", ")[0]);
                while (BaseColor == null)
                {
                    Task.Yield();
                }
                SetTexture(texName, BaseColor);
            }
            //_NormalMap
            if (allPath.Split(", ")[1] != "null")
            {

                String texName = "_BumpMap";
                NormalMap = LoadTextureData(allPath.Split(", ")[1]);

                while (NormalMap == null)
                {
                    Task.Yield();
                }
                SetTexture(texName, NormalMap);
            }
            //HeightMap
            if (allPath.Split(", ")[2] != "null")
            {
                String texName = "_ParallaxMap";
                HeightMap = LoadTextureData(allPath.Split(", ")[2]);

                while (HeightMap == null)
                {
                    Task.Yield();
                }
                SetTexture(texName, HeightMap);
            }
            //SpecularMap (no work)
            // if (allPath.Split(", ")[3] != "null")
            // {
            //     String texName = "_ParallaxMap";
            //     SpecularMap = LoadTextureData(allPath.Split(", ")[3]);
            // }
            
            //MetallicGlossMap
            if (allPath.Split(", ")[3] != "null")
            {
                String texName = "_MetallicGlossMap";
                MetallicGlossMap = LoadTextureData(allPath.Split(", ")[3]);

                while (MetallicGlossMap == null)
                {
                    Task.Yield();
                }
                SetTexture(texName, MetallicGlossMap);
            }
            //OcclusionMap
            if (allPath.Split(", ")[4] != "null")
            {
                String texName = "_OcclusionMap";
                OcclusionMap = LoadTextureData(allPath.Split(", ")[4]);

                while (OcclusionMap == null)
                {
                    Task.Yield();
                }
                SetTexture(texName, OcclusionMap);
            }
            //EmissionMap
            if (allPath.Split(", ")[5] != "null")
            {
                String texName = "_EmissionMap";
                EmissionMap = LoadTextureData(allPath.Split(", ")[5]);

                while (EmissionMap == null)
                {
                    Task.Yield();
                }
                SetTexture(texName, EmissionMap);
            }
            //GlossinessMap
            if (allPath.Split(", ")[6] != "null")
            {
                String texName = "_Glossiness";

                Glossiness = float.Parse(allPath.Split(", ")[5]);
                

                while (EmissionMap == null)
                {
                    Task.Yield();
                }
                for (int i = 0; i < spawnedObject.transform.childCount; i++)
                {
                    
                    spawnedObject.transform.GetChild(i).GetComponent<MeshRenderer>().material.SetFloat(texName, Glossiness);
                    
                }
            }
            
            // EXAMPLE: LoadTexture("albedoPath, normalMapPath, heightMapPath, OcclusionMapPath, EmissionMapPath, GlossinessMapPath")
            
        }

        public void enableMap(String name)
        {
            for (int i = 0; i < spawnedObject.transform.childCount; i++)
            {
                spawnedObject.transform.GetChild(i).GetComponent<MeshRenderer>().material.EnableKeyword (name);
            }
        }
        public void SetTexture(String name, Texture2D texture)
        {
            for (int i = 0; i < spawnedObject.transform.childCount; i++)
            {
                //spawnedObject.transform.GetChild(i).GetComponent<MeshRenderer>().material.EnableKeyword ("_METALLICGLOSSMAP");
                spawnedObject.transform.GetChild(i).GetComponent<MeshRenderer>().material.SetTexture(name, texture);
        //        spawnedObject.transform.GetChild(i).GetComponent<MeshRenderer>().material.SetTexture("_BumpMap", NormalMap);
         //       spawnedObject.transform.GetChild(i).GetComponent<MeshRenderer>().material.SetTexture("_ParallaxMap", HeightMap);
            }
        }
        public static Texture2D LoadTextureData(string filePath) {
 
            Texture2D tex = null;
            byte[] fileData;
 
            if (File.Exists(filePath))     {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
            
            return tex;
        }


        static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
        ARRaycastManager m_RaycastManager;
    }
