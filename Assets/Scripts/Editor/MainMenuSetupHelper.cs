#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_URP_2D || true
using UnityEngine.Rendering.Universal;
#endif

[InitializeOnLoad]
public static class MainMenuSetupHelper
{
    static MainMenuSetupHelper()
    {
        EditorApplication.delayCall += () =>
        {
            EnsureBuildSettings();
            if (!Application.isPlaying && !File.Exists("Assets/Scenes/MainMenu.unity"))
            {
                CreateMainMenuScene();
            }
        };
    }

    [MenuItem("Tools/Setup Main Menu Scene & Build Settings")]
    public static void CreateMainMenuScene()
    {
        string scenePath = "Assets/Scenes/MainMenu.unity";
        Scene menuScene;

        if (File.Exists(scenePath))
        {
            menuScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
        else
        {
            menuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        // 1. Camera Setup
        GameObject camObj = GameObject.Find("Main Camera");
        if (camObj == null)
        {
            camObj = new GameObject("Main Camera");
        }
        camObj.transform.position = new Vector3(0f, 0f, -10f);
        Camera cam = camObj.GetComponent<Camera>();
        if (cam == null) cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.0f;
        cam.backgroundColor = new Color(0.08f, 0.12f, 0.18f, 1f);
        cam.clearFlags = CameraClearFlags.SolidColor;

        if (camObj.GetComponent<AudioListener>() == null)
        {
            camObj.AddComponent<AudioListener>();
        }

        // 2. Global Light 2D
        GameObject lightObj = GameObject.Find("Global Light 2D");
        if (lightObj == null)
        {
            lightObj = new GameObject("Global Light 2D");
        }
#if UNITY_URP_2D || true
        Light2D l2d = lightObj.GetComponent<Light2D>();
        if (l2d == null) l2d = lightObj.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Global;
        l2d.intensity = 1.0f;
        l2d.color = new Color(1f, 0.98f, 0.92f, 1f);
#endif

        // 3. Clean Main Menu Manager GameObject
        GameObject menuMgrObj = GameObject.Find("MainMenuManager");
        if (menuMgrObj == null)
        {
            menuMgrObj = new GameObject("MainMenuManager");
        }
        MainMenuManager menuMgr = menuMgrObj.GetComponent<MainMenuManager>();
        if (menuMgr == null) menuMgr = menuMgrObj.AddComponent<MainMenuManager>();

        // Remove old background scenery or castle objects if they exist
        GameObject oldBg = GameObject.Find("Background_Scenery");
        if (oldBg != null) Undo.DestroyObjectImmediate(oldBg);

        GameObject oldCastle = GameObject.Find("MenuCastle");
        if (oldCastle != null) Undo.DestroyObjectImmediate(oldCastle);

        GameObject oldBgCastle = GameObject.Find("Background_Castle");
        if (oldBgCastle != null) Undo.DestroyObjectImmediate(oldBgCastle);

        EditorSceneManager.SaveScene(menuScene, scenePath);
        EnsureBuildSettings();

        Debug.Log("[MainMenuSetupHelper] Scene MainMenu.unity created and configured successfully!");
    }

    public static void EnsureBuildSettings()
    {
        EditorBuildSettingsScene[] currentScenes = EditorBuildSettings.scenes;

        string menuPath = "Assets/Scenes/MainMenu.unity";
        string map1Path = "Assets/Scenes/Map 1.unity";

        EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(menuPath, true),
            new EditorBuildSettingsScene(map1Path, true)
        };

        EditorBuildSettings.scenes = newScenes;
        Debug.Log("[MainMenuSetupHelper] Build Settings synced: (0) MainMenu, (1) Map 1.");
    }
}
#endif
