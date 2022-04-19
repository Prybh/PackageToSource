using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.PackageManager;
using UnityEngine;

namespace PackageToSource
{
    public class PackageToSourceWindow : EditorWindow
    {
        private static PackageToSourceWindow instance;

        private PackageListRequest packageListRequest = null;
        private RemovePackageRequest removePackageRequest = null;
        private AddPackageRequest addPackageRequest = null;
        private GitCloneRequest gitCloneRequest = null;

        private enum PackageToSourceStep { Nothing, RemoveStarted, RefreshDone, AddStarted };
        private enum SourceToPackageStep { Nothing, RemoveStarted, RefreshDone, AddStarted };

        [Serializable]
        private class TransientInfo
        {
            // Transition
            public PackageToSourceStep packageToSourceStep;
            public SourceToPackageStep sourceToPackageStep;
            public Package package;

            // Settings
            public string gitProjectsPath;
            public string shellName;
            public bool debug;
        }

        private Package changedPackage = null;
        private PackageToSourceStep packageToSourceStep = PackageToSourceStep.Nothing;
        private SourceToPackageStep sourceToPackageStep = SourceToPackageStep.Nothing;

        private List<Package> distantPackages = new List<Package>();
        private List<Package> localPackages = new List<Package>();

        private Vector2 gitScrollPos;
        private Vector2 localScrollPos;
        private Rect settingsButtonRect;

        private void OnEnable()
        {
            Logger.Log("OnEnable");

            if (EditorPrefs.HasKey("PackageToSource"))
            {
                string json = EditorPrefs.GetString("PackageToSource");
                Logger.Log(json);

                TransientInfo info = JsonUtility.FromJson<TransientInfo>(json);
                packageToSourceStep = info.packageToSourceStep;
                sourceToPackageStep = info.sourceToPackageStep;
                changedPackage = info.package;
                Settings.gitProjectsPath = info.gitProjectsPath;
                Settings.shellName = info.shellName;
                Settings.debugLogger = info.debug;

                if (packageToSourceStep == PackageToSourceStep.RemoveStarted)
                {
                    Logger.Log("PackageToSourceStep.RefreshDone");
                    packageToSourceStep = PackageToSourceStep.RefreshDone;
                }
                if (sourceToPackageStep == SourceToPackageStep.RemoveStarted)
                {
                    Logger.Log("SourceToPackageStep.RefreshDone");
                    sourceToPackageStep = SourceToPackageStep.RefreshDone;
                }
            }
        }

        private void OnDisable()
        {
            Logger.Log("OnDisable");

            TransientInfo info = new TransientInfo();
            info.packageToSourceStep = packageToSourceStep;
            info.sourceToPackageStep = sourceToPackageStep;
            info.package = changedPackage;
            info.gitProjectsPath = Settings.gitProjectsPath;
            info.shellName = Settings.shellName;
            info.debug = Settings.debugLogger;

            string json = JsonUtility.ToJson(info, false);
            Logger.Log(json);
            EditorPrefs.SetString("PackageToSource", json);
        }

        protected PackageToSourceWindow()
        {
            instance = this;

            titleContent = new GUIContent("PackageToSource");
            minSize = new Vector2(800, 100);

            if (!Git.IsPresent())
            {
                Logger.Log("Git is not installed or is not in your PATH variable", Log.Warning);
            }

            EditorApplication.update += EditorUpdate;
        }

        [MenuItem("Window/PackageToSource")]
        private static void OpenPackageToSourceWindow()
        {
            if (instance == null)
            {
                instance = GetWindow<PackageToSourceWindow>();
            }
            RefreshPackageList();
            instance.Show();
        }

        private void OnGUI()
        {
            using (var toolbarScope = new GUILayout.HorizontalScope(GUILayout.MinWidth(800)))
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh", "Refresh")))
                {
                    RefreshPackageList();
                }
                if (GUILayout.Button(EditorGUIUtility.IconContent("_Popup", "Settings")))
                {
                    PopupWindow.Show(settingsButtonRect, new SettingsWindow());
                }
                if (Event.current.type == EventType.Repaint)
                    settingsButtonRect = GUILayoutUtility.GetLastRect();

                if (packageListRequest != null)
                {
                    GUILayout.Label("Refreshing...");
                }
                else if (removePackageRequest != null || addPackageRequest != null)
                {
                    GUILayout.Label("Switching package...");
                }

                GUILayout.FlexibleSpace();
            }

            using (var horizontalScope = new GUILayout.HorizontalScope(GUILayout.MinWidth(800)))
            {
                using (var firstColumn = new GUILayout.VerticalScope("Box", GUILayout.MinWidth(394)))
                {
                    GUILayout.Label("Distant Packages", EditorStyles.boldLabel);

                    gitScrollPos = GUILayout.BeginScrollView(gitScrollPos);
                    GUI.skin.label.padding.left = 5;

                    for (int i = distantPackages.Count - 1; i >= 0; --i)
                    {
                        var packageInfo = distantPackages[i];
                        using (var packageScope = new GUILayout.HorizontalScope())
                        {
                            string labelText = packageInfo.displayName;
                            if (packageInfo.version.Length > 0)
                            {
                                labelText += " (" + packageInfo.version + ")";
                            }

                            string labelTag = "";
                            if (packageInfo.tag.Length > 0)
                            {
                                labelTag = "\n" + packageInfo.tag;
                            }

                            GUILayout.Label(new GUIContent(labelText, packageInfo.name + "\n" + packageInfo.url + labelTag));
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(new GUIContent("X", "Remove package")))
                            {
                                if (EditorUtility.DisplayDialog("Remove package?", "Are you sure you want to remove " + packageInfo.name + " from the project?", "Remove", "Keep"))
                                {
                                    removePackageRequest = new RemovePackageRequest(packageInfo);
                                }
                            }
                            if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.LastKey", "Embed " + packageInfo.name + " locally")))
                            {
                                changedPackage = packageInfo;
                                gitCloneRequest = new GitCloneRequest(changedPackage);
                            }
                        }
                    }
                
                    GUILayout.EndScrollView();
                }

                using (var secondColumn = new GUILayout.VerticalScope("Box", GUILayout.MinWidth(394)))
                {
                    GUILayout.Label("Local Packages", EditorStyles.boldLabel);

                    localScrollPos = GUILayout.BeginScrollView(localScrollPos);
                    GUI.skin.label.padding.left = 5;

                    for (int i = localPackages.Count - 1; i >= 0; --i)
                    {
                        var packageInfo = localPackages[i];
                        using (var packageScope = new GUILayout.HorizontalScope())
                        {
                            string path = Application.dataPath + "/../Packages/" + packageInfo.displayName;

                            if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.FirstKey", "Back to distant package")))
                            {
                                changedPackage = packageInfo;

                                if (Settings.deleteOnUnused)
                                {
                                    Logger.Log("DeleteStarted");
                                    FileIO.DeleteDirectory(path);
                                    Logger.Log("Deleted");
                                }

                                removePackageRequest = new RemovePackageRequest(packageInfo);
                                packageToSourceStep = PackageToSourceStep.Nothing;
                                sourceToPackageStep = SourceToPackageStep.RemoveStarted;
                                Logger.Log("SourceToPackageStep.RemoveStarted");
                            }

                            string extraInfo = packageInfo.tag;
                            if (extraInfo.Length == 0)
                            {
                                extraInfo = packageInfo.branch;
                            }
                            if (extraInfo.Length > 0)
                            {
                                extraInfo = " (" + extraInfo + ")";
                            }

                            string modif = "";
                            if (packageInfo.filesChanged > 0)
                            {
                                modif = " [" + packageInfo.filesChanged + " modif.]";
                            }

                            GUILayout.Label(new GUIContent(packageInfo.displayName + extraInfo + modif));

                            if (GUILayout.Button(EditorGUIUtility.IconContent("Project", "Show In Explorer")))
                            {
                                EditorUtility.RevealInFinder(packageInfo.resolvedPath + "/package.json");
                            }
                            if (GUILayout.Button(new GUIContent("X", "Remove package")))
                            {
                                if (EditorUtility.DisplayDialog("Remove package?",  "Are you sure you want to remove " + packageInfo.name  + " from the project?", "Remove", "Keep"))
                                {
                                    if (Settings.deleteOnUnused)
                                    {
                                        Logger.Log("DeleteStarted");
                                        FileIO.DeleteDirectory(path);
                                        Logger.Log("Deleted");
                                    }

                                    removePackageRequest = new RemovePackageRequest(packageInfo);
                                }
                            }
                            GUILayout.FlexibleSpace();
                        }
                    }

                    GUILayout.EndScrollView();
                }
            }
        }

        private static void RefreshPackageList()
        {
            if (instance != null)
            {
                if (instance.packageListRequest == null)
                {
                    instance.packageListRequest = new PackageListRequest();
                }
            }
        }

        [DidReloadScripts]
        private static void EditorRefresh()
        {
            RefreshPackageList();
        }

        [InitializeOnLoadMethod]
        private static void RegisterToPackageManagerRefresh()
        {
            Events.registeredPackages += RegisteredPackagesEventHandler;
        }

        private static void RegisteredPackagesEventHandler(PackageRegistrationEventArgs packageRegistrationEventArgs)
        {
            RefreshPackageList();
        }

        private void EditorUpdate()
        {
            if (packageListRequest != null && packageListRequest.Update())
            {
                distantPackages = packageListRequest.distantPackages;
                localPackages = packageListRequest.localPackages;
                packageListRequest = null;
            }

            if (gitCloneRequest != null && gitCloneRequest.Update())
            {
                removePackageRequest = new RemovePackageRequest(changedPackage);
                packageToSourceStep = PackageToSourceStep.RemoveStarted;
                sourceToPackageStep = SourceToPackageStep.Nothing;
                Logger.Log("PackageToSourceStep.RemoveStarted");
                gitCloneRequest = null;
            }
            else if (removePackageRequest != null && removePackageRequest.Update())
            {
                removePackageRequest = null;
            }
            if (packageToSourceStep == PackageToSourceStep.RefreshDone && changedPackage != null)
            {
                Logger.Log("Add Package");
                packageToSourceStep = PackageToSourceStep.AddStarted;
                addPackageRequest = AddPackageRequest.AddLocalPackage(changedPackage);
            }
            else if (sourceToPackageStep == SourceToPackageStep.RefreshDone && changedPackage != null)
            {
                Logger.Log("Add Package");
                sourceToPackageStep = SourceToPackageStep.AddStarted;
                addPackageRequest = AddPackageRequest.AddDistantPackage(changedPackage);
            }
            else if (addPackageRequest != null && addPackageRequest.Update())
            {
                changedPackage = null;
                Logger.Log("Package Added");
                sourceToPackageStep = SourceToPackageStep.Nothing;
                addPackageRequest = null;
            }
        }
    }
}
