using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class AutoSaver
{
    #region Public members
    // Flag to enable/disable AutoSaver!
    // Wulfram: off by default (2026-10-10). Opening Playground in the editor marks it dirty - UGUI
    // recomputes layout-driven RectTransforms (TargetInfo rows, game console text, chat dock), and
    // the results aren't even stable between opens - so AutoSaver kept silently rewriting the
    // tracked scene file. Turn it on per machine from AutoSaver's editor window if wanted.
    public static bool IsEnabled = false;

    // Flag to enable/disable debug messages!
    public static bool IsDebugEnabled = true;

    // Flag to enable/disable assets autosaving!
    public static bool AutosaveAssets = true;

    // Flag to enable/disable on play saving!
    public static bool SaveOnPlay = true;

    // The interval in minutes after that AutoSaver will save your scene!
    public static int AutosaveInterval = 2;
    #endregion

    #region Private stuff
    private static DateTime _lastAutosaveTime = DateTime.Now;
    #endregion

    #region Private methods
    /// <summary>
    /// Saves active scene.
    /// </summary>
    private static void SaveScene()
    {
        Scene activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.isDirty)
        {
            EditorSceneManager.SaveScene(activeScene);
            if (AutosaveAssets) AssetDatabase.SaveAssets();
            if (IsDebugEnabled) Debug.Log("AutoSaver: " + activeScene.name + " saved successfully!");
        }
    }

    /// <summary>
    /// Loads editor preferences.
    /// </summary>
    private static void LoadEditorPrefs()
    {
        if (EditorPrefs.HasKey("UNITY_AUTOSAVER_prefsExist"))
        {
            if (EditorPrefs.HasKey("UNITY_AUTOSAVER_isEnabled"))
            {
                AutoSaver.IsEnabled = EditorPrefs.GetBool("UNITY_AUTOSAVER_isEnabled");
            }
            if (EditorPrefs.HasKey("UNITY_AUTOSAVER_isDebugEnabled"))
            {
                AutoSaver.IsDebugEnabled = EditorPrefs.GetBool("UNITY_AUTOSAVER_isDebugEnabled");
            }
            if (EditorPrefs.HasKey("UNITY_AUTOSAVER_autosaveAssets"))
            {
                AutoSaver.AutosaveAssets = EditorPrefs.GetBool("UNITY_AUTOSAVER_autosaveAssets");
            }
            if (EditorPrefs.HasKey("UNITY_AUTOSAVER_autosaveInterval"))
            {
                AutoSaver.AutosaveInterval = EditorPrefs.GetInt("UNITY_AUTOSAVER_autosaveInterval");
            }
        }
    }

    private static void OnUpdate()
    {
        if (_lastAutosaveTime.AddMinutes(AutosaveInterval) <= DateTime.Now)
        {
            SaveScene();
            _lastAutosaveTime = DateTime.Now;
        }
    }

    private static void OnEnterInPlayMode()
    {
        if (SaveOnPlay && !EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            SaveScene();
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Initialize AutoSaver features
    /// </summary>
    public static void Initialize()
    {
        LoadEditorPrefs();
        if(IsEnabled)
            ActivateAutosaver();
    }

    /// <summary>
    /// Register AutoSaver's update to Editor's update
    /// </summary>
    public static void ActivateAutosaver()
    {
        _lastAutosaveTime = DateTime.Now;
        EditorApplication.update += OnUpdate;
        EditorApplication.playmodeStateChanged += OnEnterInPlayMode;
        IsEnabled = true;
        if (IsDebugEnabled) Debug.Log("AutoSaver: ON");
    }

    /// <summary>
    /// Unregister AutoSaver's update to Editor's update
    /// </summary>
    public static void DeactivateAutosaver()
    {
        EditorApplication.update -= OnUpdate;
        EditorApplication.playmodeStateChanged -= OnEnterInPlayMode;
        IsEnabled = false;
        if (IsDebugEnabled) Debug.Log("AutoSaver: OFF");
    }
    #endregion

    #region Constructor
    static AutoSaver()
    {
        Initialize();
    }
    #endregion
}