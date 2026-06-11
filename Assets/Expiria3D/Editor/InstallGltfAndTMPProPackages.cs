using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.Collections.Generic;
using System.IO; // For file and path operations

public class InstallGltfAndTMPProPackages
{
    static InstallGltfAndTMPProPackages _instance;

    public static InstallGltfAndTMPProPackages Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new InstallGltfAndTMPProPackages();
            }

            return _instance;
        }
    }

    private ListRequest listRequest;
    private AddRequest currentAddRequest;
    private Queue<string> packagesToInstall = new Queue<string>();
    private bool isInstalling = false;

    private bool isGltfFastInstalled = false;
    private bool isTextMeshProInstalled = false;
    private bool is2DSpriteInstalled = false;
    private bool packageCheckCompleted = false; // To know if the packages have been checked

    public void Initialize()
    {
        // Initialize the state variables
        isGltfFastInstalled = false;
        isTextMeshProInstalled = false;
        is2DSpriteInstalled = false;
        packageCheckCompleted = false;
        isInstalling = false;

        // Start checking installed packages when the window opens
        CheckInstalledPackages();
    }

    /// <summary>
    /// Returns true if the packages are being installed, false otherwise.
    /// </summary>
    /// <returns></returns>
    public bool InstallIfNeeded()
    {
        if (!packageCheckCompleted)
        {
            // Still checking installed packages...
            return false;
        }

        if (isInstalling)
        {
            // Packages are being installed...
            return true;
        }
        else
        {
            if (isGltfFastInstalled && isTextMeshProInstalled && is2DSpriteInstalled)
            {
                // All packages are already installed.
                return false;
            }
            else
            {
                InstallPackages();
                return true;
            }
        }
    }

    void CheckInstalledPackages()
    {
        listRequest = Client.List(); // List all the project packages
        EditorApplication.update += PackageListProgress;
    }

    void PackageListProgress()
    {
        if (listRequest.IsCompleted)
        {
            if (listRequest.Status == StatusCode.Success)
            {
                var installedPackages = listRequest.Result;

                // Check if GLTFast, TextMeshPro, and 2D Sprite are installed
                foreach (var package in installedPackages)
                {
                    //Commented because TL already imports it
                    //if (package.name == "com.unity.cloud.gltfast")
                    // {
                    isGltfFastInstalled = true;
                    // }
                    // else
                    if (package.name == "com.unity.textmeshpro")
                    {
                        isTextMeshProInstalled = true;
                    }
                    else if (package.name == "com.unity.2d.sprite")
                    {
                        is2DSpriteInstalled = true;
                    }

                    // If all packages are installed, we can exit the loop
                    if (isGltfFastInstalled && isTextMeshProInstalled && is2DSpriteInstalled)
                    {
                        break;
                    }
                }
            }
            else
            {
                Debug.LogError("Error listing packages: " + listRequest.Error.message);
                EditorUtility.DisplayDialog("Error", "Could not retrieve the list of packages:\n" + listRequest.Error.message, "Close");
            }

            packageCheckCompleted = true;
            EditorApplication.update -= PackageListProgress;
        }
    }

    void InstallPackages()
    {
        if (isInstalling)
        {
            return;
        }

        isInstalling = true;

        // Add packages that are not installed to the queue
        if (!isTextMeshProInstalled)
        {
            packagesToInstall.Enqueue("com.unity.textmeshpro");
        }

        if (!isGltfFastInstalled)
        {
            packagesToInstall.Enqueue("com.unity.cloud.gltfast");
        }

        if (!is2DSpriteInstalled)
        {
            packagesToInstall.Enqueue("com.unity.2d.sprite");
        }

        if (packagesToInstall.Count > 0)
        {
            InstallNextPackage();
            EditorApplication.update += InstallProgress;
        }
        else
        {
            isInstalling = false;
            // All packages are already installed.
        }
    }

    void InstallProgress()
    {
        if (currentAddRequest != null && currentAddRequest.IsCompleted)
        {
            if (currentAddRequest.Status == StatusCode.Success)
            {
                // Package installed successfully

                // Update the installed package status
                if (currentAddRequest.Result.name == "com.unity.textmeshpro")
                {
                    isTextMeshProInstalled = true;

                    // Import TMP Essential Resources
                    ImportTMPEssentialResources();
                }
                else if (currentAddRequest.Result.name == "com.unity.cloud.gltfast")
                {
                    isGltfFastInstalled = true;
                }
                else if (currentAddRequest.Result.name == "com.unity.2d.sprite")
                {
                    is2DSpriteInstalled = true;
                }
            }
            else if (currentAddRequest.Status >= StatusCode.Failure)
            {
                Debug.LogError("Error installing package: " + currentAddRequest.Error.message);
                EditorUtility.DisplayDialog("Error", "Could not install the package:\n" + currentAddRequest.Error.message, "Close");
            }

            currentAddRequest = null;

            if (packagesToInstall.Count > 0)
            {
                InstallNextPackage();
            }
            else
            {
                isInstalling = false;
                EditorApplication.update -= InstallProgress;
                // All packages have been installed.
            }
        }
    }

    void InstallNextPackage()
    {
        string packageName = packagesToInstall.Dequeue();
        // Starting the installation of the package
        currentAddRequest = Client.Add(packageName);
    }

    void ImportTMPEssentialResources()
    {
        // Path to the TextMeshPro package in the Packages folder
        string packageFullPath = "Packages/com.unity.textmeshpro";
        // Path to the TMP Essential Resources.unitypackage file
        string unityPackagePath = Path.Combine(packageFullPath, "Package Resources", "TMP Essential Resources.unitypackage");

        if (File.Exists(unityPackagePath))
        {
            // Import the package silently (without user interaction)
            AssetDatabase.ImportPackage(unityPackagePath, false);
        }
        else
        {
            Debug.LogWarning("Could not find 'TMP Essential Resources.unitypackage' at path: " + unityPackagePath);
        }
    }
}