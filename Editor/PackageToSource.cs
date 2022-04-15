using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace PackageToSource
{
    public class PackageToSourceWindow : EditorWindow
    {
        private static PackageToSourceWindow instance;

        private PackageListRequest packageListRequest = null;
        private RemovePackageRequest removePackageRequest = null;
        //private EmbedPackageRequest embedPackageRequest = null;
        private AddPackageRequest addPackageRequest = null;

        private class GitCloneRequest
        {
            private Package _package = null;
            private bool _error = false;
            public bool hasError
            {
                get { return _error; }
            }

            public GitCloneRequest(Package package)
            {
                _package = package;
                _error = false;

                string path = Application.dataPath + "/../Packages/" + _package.displayName;

                if (FileIO.DirectoryExists(path) && FileIO.IsEmptyDirectory(path))
                {
                    FileIO.DeleteDirectory(path);
                }

                string gitCloneOutput = Git.Clone(Application.dataPath + "/../Packages/", _package.url);
                Logger.Log(gitCloneOutput, Log.Warning);
                if (gitCloneOutput.Contains("fatal"))
                {
                    _error = true;
                }

                string gitCheckoutOutput = Git.Checkout(path, _package.hash);
                Logger.Log(gitCloneOutput, Log.Warning);
                if (gitCheckoutOutput.Contains("fatal"))
                {
                    _error = true;
                }

                if (_error) // Retry
                {
                    Logger.Log("Retry Clone");

                    gitCloneOutput = Git.Clone(Application.dataPath + "/../Packages/", _package.url);
                    Logger.Log(gitCloneOutput, Log.Warning);
                    if (gitCloneOutput.Contains("fatal"))
                    {
                        _error = true;
                    }

                    _error = false;

                    gitCheckoutOutput = Git.Checkout(path, _package.hash);
                    Logger.Log(gitCloneOutput, Log.Warning);
                    if (gitCheckoutOutput.Contains("fatal"))
                    {
                        _error = true;
                    }
                }

                Logger.Log("CloneStarted");
            }

            public bool Update()
            {
                string path = Application.dataPath + "/../Packages/" + _package.displayName;
                if (FileIO.DirectoryExists(path) && FileIO.DirectoryExists(path + "/.git") && FileIO.DirectoryExists(path + "/package.json"))
                {
                    _error = false;
                    Logger.Log("Cloned");
                    AssetDatabase.ImportAsset("Packages/" + _package.displayName);
                    Logger.Log("Imported");
                    return true;
                }
                if (_error)
                {
                    return true;
                }
                return false;
            }
        }
        private GitCloneRequest gitCloneRequest = null;

        private enum PackageToSourceStep { Nothing, RemoveStarted, RefreshDone, EmbedStarted };
        private enum SourceToPackageStep { Nothing, RemoveStarted, RefreshDone, AddStarted };

        [System.Serializable]
        private class TransientInfo
        {
            public PackageToSourceStep packageToSourceStep;
            public SourceToPackageStep sourceToPackageStep;
            public Package package;
            // TODO : Cache info about current local(/distant?) packages ?
        }

        private Package changedPackage = null;
        private PackageToSourceStep packageToSourceStep = PackageToSourceStep.Nothing;
        private SourceToPackageStep sourceToPackageStep = SourceToPackageStep.Nothing;

        private List<Package> distantPackages = new List<Package>();
        private List<Package> localPackages = new List<Package>();

        private Vector2 gitScrollPos;
        private Vector2 localScrollPos;


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
                if (GUILayout.Button("Refresh", GUILayout.Width(60f)))
                {
                    localPackages.Clear();
                    RefreshPackageList();
                }
                if (GUILayout.Button("Clone repo test"))
                {
                    Client.Add("https://github.com/Prybh/TestUnityPackage.git");
                }

                if (packageListRequest != null)
                {
                    GUILayout.Label("Refreshing...");
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

                    /*if (changePackageRequest != null && changePackageRequest.waitingRefresh)
                    {
                        GUILayout.Label("Waiting for Refresh...");
                    }
                    else if (changePackageRequest != null)
                    {
                        GUILayout.Label("Processing...");
                    }
                    else if (refreshPackagesRequest != null)
                    {
                        GUILayout.Label("Refreshing...");
                    }
                    else*/
                    {
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
                                    removePackageRequest = new RemovePackageRequest(packageInfo);
                                }
                                if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.LastKey", "Embed " + packageInfo.name + " locally")))
                                {
                                    changedPackage = packageInfo;
                                    localPackages.Add(packageInfo);

                                    AssetDatabase.DisallowAutoRefresh();

                                    gitCloneRequest = new GitCloneRequest(changedPackage);
                                }
                            }
                        }
                    }
                
                    GUILayout.EndScrollView();
                }

                using (var secondColumn = new GUILayout.VerticalScope("Box", GUILayout.MinWidth(394)))
                {
                    GUILayout.Label("Embedded Packages", EditorStyles.boldLabel);

                    localScrollPos = GUILayout.BeginScrollView(localScrollPos);
                    GUI.skin.label.padding.left = 5;

                    /*if (changePackageRequest != null && changePackageRequest.waitingRefresh)
                    {
                        GUILayout.Label("Waiting for Refresh...");
                    }
                    else if (changePackageRequest != null)
                    {
                        GUILayout.Label("Processing...");
                    }
                    else*/
                    {
                        for (int i = localPackages.Count - 1; i >= 0; --i)
                        {
                            var packageInfo = localPackages[i];
                            using (var packageScope = new GUILayout.HorizontalScope())
                            {
                                string path = Application.dataPath + "/../Packages/" + changedPackage.displayName;

                                if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.FirstKey", "Back to distant package")))
                                {
                                    changedPackage = packageInfo;
                                    localPackages.RemoveAt(i);

                                    AssetDatabase.DisallowAutoRefresh();

                                    Logger.Log("DeleteStarted");
                                    FileIO.DeleteDirectory(path);
                                    Logger.Log("Deleted");

                                    removePackageRequest = new RemovePackageRequest(packageInfo);
                                    packageToSourceStep = PackageToSourceStep.Nothing;
                                    sourceToPackageStep = SourceToPackageStep.RemoveStarted;
                                    Logger.Log("SourceToPackageStep.RemoveStarted");
                                }

                                //string tagName = Git.GetTagName(path);
                                //string branchName = Git.GetBranchName(path);

                                GUILayout.Label(new GUIContent(packageInfo.displayName + " ()"));
                                if (GUILayout.Button(EditorGUIUtility.IconContent("d_Project", "Show In Explorer")))
                                {
                                    EditorUtility.RevealInFinder(Application.dataPath + "/../Packages/" + packageInfo.displayName + "/package.json");
                                }
                                if (GUILayout.Button(new GUIContent("X", "Remove package")))
                                {
                                    FileIO.DeleteDirectory(path);
                                }
                                GUILayout.FlexibleSpace();
                            }
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
                instance.packageListRequest = new PackageListRequest();
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
                if (gitCloneRequest.hasError)
                {
                    Logger.Log("Can't clone. Cancelling package to source", Log.Error);
                    changedPackage = null;
                    packageToSourceStep = PackageToSourceStep.Nothing;
                    sourceToPackageStep = SourceToPackageStep.Nothing;
                }
                else
                {
                    removePackageRequest = new RemovePackageRequest(changedPackage);
                    packageToSourceStep = PackageToSourceStep.RemoveStarted;
                    sourceToPackageStep = SourceToPackageStep.Nothing;
                    Logger.Log("PackageToSourceStep.RemoveStarted");
                }

                gitCloneRequest = null;
            }
            else if (removePackageRequest != null && removePackageRequest.Update())
            {
                if (changedPackage != null)
                {
                    Logger.Log("AllowRefresh");
                    AssetDatabase.AllowAutoRefresh();
                    Logger.Log("ForceRefresh");
                    AssetDatabase.Refresh();
                }
                else
                {
                    Logger.Log("changedPackage should not be null here", Log.Error);
                }
                removePackageRequest = null;
            }
            else if (packageToSourceStep == PackageToSourceStep.RefreshDone && changedPackage != null)
            {
                Logger.Log("PackageToSourceStep.Nothing");
                packageToSourceStep = PackageToSourceStep.Nothing;
                /*
                Logger.Log("PackageToSourceStep.EmbedStarted");
                packageToSourceStep = PackageToSourceStep.EmbedStarted;
                embedPackageRequest = new EmbedPackageRequest(changedPackage);
                */
            }
            /*
            else if (embedPackageRequest != null && embedPackageRequest.Update())
            {
                changedPackage = null;
                Logger.Log("PackageToSourceStep.Nothing");
                packageToSourceStep = PackageToSourceStep.Nothing;
                embedPackageRequest = null;
            }
            */
            else if (sourceToPackageStep == SourceToPackageStep.RefreshDone && changedPackage != null)
            {
                Logger.Log("SourceToPackageStep.AddStarted");
                sourceToPackageStep = SourceToPackageStep.AddStarted;
                addPackageRequest = new AddPackageRequest(changedPackage);
            }
            else if (addPackageRequest != null && addPackageRequest.Update())
            {
                changedPackage = null;
                Logger.Log("SourceToPackageStep.Nothing");
                sourceToPackageStep = SourceToPackageStep.Nothing;
                addPackageRequest = null;
            }
        }
    }
}
