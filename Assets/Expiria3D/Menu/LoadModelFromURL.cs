using UnityEngine;
using TriLibCore;
using System;
using System.Collections.Generic;
using Expiria3DSpace;
/// <summary>
/// Represents a sample that loads a compressed (Zipped) Model.
/// </summary>
public class LoadModelFromURL : MonoBehaviour
{

    static LoadModelFromURL instance;
    public GameObject parentForImageTracker;
    public GameObject parentForWorldTracker;


    public static LoadModelFromURL Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LoadModelFromURL>(true);
            }

            return instance;
        }
    }



    /// <summary>
    /// Cached Asset Loader Options instance.
    /// </summary>
    private AssetLoaderOptions _assetLoaderOptions;


    Vector3 position;
    Vector3 scale;
    Vector3 rotation;

    string lastModelURLRequested;


    public void LoadModel(string modelURL, Vector3 position, Vector3 scale, Vector3 rotation)
    {
        if (lastModelURLRequested == modelURL)
        {
            return;
        }

        lastModelURLRequested = modelURL;
        LoadingAnimation.Instance.PlayLoading();
        this.position = position;
        this.scale = scale;
        this.rotation = rotation;

        //clear all the children of the parent, removing all. This is to be able to show the loading animation again
        if (ARManager.Instance.arProjectType == ARManager.ARProjectType.Image)
        {
            foreach (Transform child in parentForImageTracker.transform)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            foreach (Transform child in parentForWorldTracker.transform)
            {
                Destroy(child.gameObject);
            }
        }


        if (_assetLoaderOptions == null)
        {
            _assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions(false, true);
        }

        var webRequest = AssetDownloader.CreateWebRequest(modelURL);
        AssetDownloader.LoadModelFromUri(webRequest, OnLoad, OnMaterialsLoad, OnProgress, OnError, null, _assetLoaderOptions, modelURL, null, false, false);
    }

    /// <summary>
    /// Called when any error occurs.
    /// </summary>
    /// <param name="obj">The contextualized error, containing the original exception and the context passed to the method where the error was thrown.</param>
    private void OnError(IContextualizedError obj)
    {
        Debug.LogError($"An error occurred while loading your Model: {obj.GetInnerException()}");
    }

    /// <summary>
    /// Called when the Model loading progress changes.
    /// </summary>
    /// <param name="assetLoaderContext">The context used to load the Model.</param>
    /// <param name="progress">The loading progress.</param>
    private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
    {
        //Debug.Log($"Loading Model. Progress: {progress:P}");
    }

    /// <summary>
    /// Called when the Model (including Textures and Materials) has been fully loaded.
    /// </summary>
    /// <remarks>The loaded GameObject is available on the assetLoaderContext.RootGameObject field.</remarks>
    /// <param name="assetLoaderContext">The context used to load the Model.</param>
    private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
    {
        Dictionary<Type, object> customData = assetLoaderContext.CustomData as Dictionary<Type, object>;
        string modelURL = customData[typeof(System.Object)] as string;

        //if the modelURL is not the same as the last modelURL, destroy it and return
        if (modelURL != lastModelURLRequested)
        {
            Destroy(assetLoaderContext.RootGameObject);
            return;
        }

        //Set the unlit texture to the material
        foreach (UnityEngine.Object obj in assetLoaderContext.Allocations)
        {
            if (obj is Material)
            {
                Material material = obj as Material;
                Shader myShader = Resources.Load<Shader>("Shaders/TextureWithShadows");
                if (myShader == null)
                {
                    Debug.LogError("No se encontró el shader en Resources/Shaders/TextureWithShadows");
                }
                else
                {
                    material.shader = myShader;
                }

                material.SetColor("_Color", Color.white);
            }
        }

        //Debug.Log("Materials loaded. Model fully loaded. Name " + assetLoaderContext.RootGameObject.name);
        if (ARManager.Instance.arProjectType == ARManager.ARProjectType.Image)
        {
            assetLoaderContext.RootGameObject.transform.parent = parentForImageTracker.transform;
        }
        else
        {
            assetLoaderContext.RootGameObject.transform.parent = parentForWorldTracker.transform;
        }

        assetLoaderContext.RootGameObject.transform.localPosition = position;
        assetLoaderContext.RootGameObject.transform.localScale = scale;
        assetLoaderContext.RootGameObject.transform.localRotation = Quaternion.Euler(rotation);

        LoadingAnimation.Instance.StopLoading();
    }

    /// <summary>
    /// Called when the Model Meshes and hierarchy are loaded.
    /// </summary>
    /// <remarks>The loaded GameObject is available on the assetLoaderContext.RootGameObject field.</remarks>
    /// <param name="assetLoaderContext">The context used to load the Model.</param>
    private void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        //Debug.Log("Model loaded. Loading materials.");
    }
}
