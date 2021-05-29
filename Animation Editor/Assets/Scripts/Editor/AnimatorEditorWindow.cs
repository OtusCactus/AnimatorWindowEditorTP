using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using UnityEditor.SceneManagement;

public class AnimatorEditorWindow : EditorWindow
{
    //General
    private static Animator[] allAnimators = null;
    Animator currentAnimator = null;
    int animatorSelected = 0;
    private static bool sceneHasAnimators = false;

    //Info
    AnimatorController currentAnimatorController = null;
    AnimationClip[] allAnimatorAnimations = null;
    string[] allAnimatorAnimationsNames = null;
    int animationSelected = 0;
    float time = 0;
    bool needToPlay = false;
    bool playOnLoop = false;
    double startTime = 0;

    //Bonus
    float speed = 1;
    int tabSelected = 0;
    bool isInPause = false;
    float timeInPause = 0;
    float previousTime = 0;
    Vector3 objectPosition = Vector3.zero;
    float loopDelay = 0;
    float timeInWait = 0;
    bool isInWait = false;
    string searchString = "";
    Vector2 animatorsScrollPos = Vector2.zero;
    Vector2 windowScrollPos = Vector2.zero;
    static AnimatorEditorWindow window;
    Color baseColor;
    Rect windowRect;


    [MenuItem("Custom Editor/AnimatorWindow")]
    public static void ShowWindow()
    {
        AnimatorEditorWindow window = GetWindow<AnimatorEditorWindow>();
        //EditorWindow.GetWindow(typeof(AnimatorEditorWindow));
        allAnimators = FindAllAnimatorsInScene();
        window.titleContent = new GUIContent("Super Improved Animator");
    }
    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    private void OnHierarchyChanged()
    {
        allAnimators = FindAllAnimatorsInScene();
    }

    private void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        DisableAnimations();
    }

    private void OnPlayModeStateChange(PlayModeStateChange state)
    {
        DisableAnimations();
        allAnimators = FindAllAnimatorsInScene();
    }

    private void DisableAnimations()
    {
        needToPlay = false;
        time = 0.0f;
        AnimationMode.StopAnimationMode();
    }

    private void OnDestroy()
    {
        DisableAnimations(); 
        EditorApplication.playModeStateChanged -= OnPlayModeStateChange; 
        EditorSceneManager.sceneOpened -= OnSceneOpened; 
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
    }

    void OnGUI()
    {
        if (EditorApplication.isPlaying)
        {
            GUILayout.Label("Can't use animator in Play mode", EditorStyles.boldLabel);
        }
        else
        {
            GUIStyle style = GUI.skin.label;
            if (allAnimators == null)
            {
                allAnimators = FindAllAnimatorsInScene();
            }

            windowScrollPos = EditorGUILayout.BeginScrollView(windowScrollPos);
            tabSelected = GUILayout.Toolbar(tabSelected, new string[] { "Animators", "Animations"});
            EditorGUILayout.Separator();
            
            switch (tabSelected)
            {
                case 0:

                    GUILayout.BeginHorizontal(EditorStyles.toolbar);
                    searchString = GUILayout.TextField(searchString);
                    //searchField = new UnityEditor.IMGUI.Controls.SearchField();
                    GUILayout.EndHorizontal();

                    GUILayout.Label("All Animators", EditorStyles.boldLabel);
                    baseColor = GUI.backgroundColor;
                    if (GUILayout.Button("Refresh Animators"))
                    {
                        allAnimators = FindAllAnimatorsInScene();
                    }
                    EditorGUILayout.Separator();

                    List<Animator> searchAnimatorList = new List<Animator>();
                    if (sceneHasAnimators)
                    {
                        if(currentAnimator == null)
                        {
                            currentAnimator = allAnimators[0];
                        }
                        for (int i = 0; i < allAnimators.Length; i++)
                        {
                            searchAnimatorList.Add(allAnimators[i]);
                            if (searchString != "" && !allAnimators[i].gameObject.name.ToLower().Contains(searchString.ToLower()))
                            {
                                searchAnimatorList.Remove(allAnimators[i]);
                            }
                        }

                        //windowRect = window.position;
                        GUILayout.Label("Animators List", EditorStyles.boldLabel);

                        GUILayout.BeginVertical(GUILayout.Height(100/*windowRect.height/2)*/));
                        animatorsScrollPos = EditorGUILayout.BeginScrollView(animatorsScrollPos);

                        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                        buttonStyle.alignment = TextAnchor.MiddleLeft;
                        buttonStyle.onActive.textColor = Color.red;
                        for (int i = 0; i < searchAnimatorList.Count; ++i)
                        {
                            GUI.backgroundColor = new Color(0,0,0,0);
                            if (searchAnimatorList[i] != null)
                            {
                                if (GUILayout.Button(searchAnimatorList[i].gameObject.name, buttonStyle))
                                {
                                    animatorSelected = i;
                                    Selection.activeObject = allAnimators[i];
                                    currentAnimator = allAnimators[i];
                                }
                            }
                        }
                        //animatorSelected = EditorGUILayout.Popup(animatorSelected, allAnim);
                        //Selection.activeObject = allAnimators[animatorSelected];
                        if(currentAnimator != null)
                        {
                            objectPosition = currentAnimator.transform.position;
                        }
                        else
                        {
                            currentAnimator = allAnimators[0];
                        }

                        EditorGUILayout.EndScrollView();
                        GUILayout.EndVertical();


                        GUI.backgroundColor = baseColor;
                        if (GUILayout.Button("Focus on GameObject"))
                        {
                            Selection.activeObject = currentAnimator;
                            EditorGUIUtility.PingObject(Selection.activeObject);
                            SceneView.lastActiveSceneView.FrameSelected();
                        }

                        currentAnimatorController = currentAnimator.runtimeAnimatorController as AnimatorController;


                        GUILayout.Label("Animator Info", EditorStyles.boldLabel);
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.alignment = TextAnchor.UpperLeft; GUILayout.Label("Animator Selected : " + currentAnimator.gameObject.name);
                        if (currentAnimatorController != null)
                        {
                            style.alignment = TextAnchor.UpperLeft; GUILayout.Label("Layers : " + currentAnimator.layerCount);
                            style.alignment = TextAnchor.UpperLeft; GUILayout.Label("Clips : " + currentAnimatorController.animationClips.Length);
                        }
                        else
                        {
                            GUIStyle warning = new GUIStyle(GUI.skin.label);
                            warning.fontStyle = FontStyle.Bold;
                            warning.normal.textColor = new Color(0.5f, 0, 0);
                            GUILayout.Label("Your animator doesn't have a controller", warning);
                        }
                        EditorGUILayout.EndVertical();
                    }
                    else
                    {
                        GUILayout.Label("Your scene doesn't have any animators", EditorStyles.boldLabel);
                    }

                    break;
                case 1:
                    if (currentAnimatorController != null)
                    {
                        GUILayout.Label("All Animations", EditorStyles.boldLabel);
                        allAnimatorAnimations = currentAnimatorController.animationClips;
                        allAnimatorAnimationsNames = new string[currentAnimatorController.animationClips.Length];
                        for (int i = 0; i < currentAnimatorController.animationClips.Length; ++i)
                        {
                            allAnimatorAnimationsNames[i] = currentAnimatorController.animationClips[i].name;
                        }
                        animationSelected = EditorGUILayout.Popup(animationSelected, allAnimatorAnimationsNames);

                        EditorGUILayout.Separator();

                        GUILayout.Label("Animation Info", EditorStyles.boldLabel);
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.alignment = TextAnchor.UpperLeft; GUILayout.Label("Duration : " + allAnimatorAnimations[animationSelected].length + "s");
                        style.alignment = TextAnchor.UpperLeft; GUILayout.Label("Animation loop : " + allAnimatorAnimations[animationSelected].isLooping);
                        style.alignment = TextAnchor.UpperLeft; GUILayout.Label("Animation framerate : " + allAnimatorAnimations[animationSelected].frameRate);
                        //transitions ?
                        if (GUILayout.Button("Go to Animation"))
                        {
                            string path = AssetDatabase.GetAssetPath(allAnimatorAnimations[animationSelected]);
                            Selection.activeObject = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.Separator();
                        EditorGUILayout.Separator();

                        GUILayout.Label("Animation Parameters", EditorStyles.boldLabel);


                        //Time
                        //float startTime = 0.0f;
                        float stopTime = allAnimatorAnimations[animationSelected].length;
                        //time = EditorGUILayout.Slider(time, 0, stopTime);

                        Rect position = EditorGUILayout.GetControlRect(false, 2 * EditorGUIUtility.singleLineHeight);
                        position.height *= 0.5f;
                        previousTime = time;
                        time = EditorGUI.Slider(position, "Animation time", time, 0, stopTime);
                        if (!needToPlay)
                        {
                            if (previousTime != time)
                            {
                                AnimationMode.StartAnimationMode();
                                AnimationMode.SampleAnimationClip(currentAnimator.gameObject, allAnimatorAnimations[animationSelected], time);
                                objectPosition = currentAnimator.transform.position;
                            }
                            else
                            {
                                AnimationMode.StopAnimationMode();
                                currentAnimator.transform.position = objectPosition;
                            }
                        }
                        // Set the rect for the sub-labels
                        position.y += position.height;
                        position.x += EditorGUIUtility.labelWidth;
                        position.width -= EditorGUIUtility.labelWidth + 54;

                        style.alignment = TextAnchor.UpperLeft; EditorGUI.LabelField(position, "0", style);
                        style.alignment = TextAnchor.UpperRight; EditorGUI.LabelField(position, stopTime.ToString(), style);



                        //Speed

                        position = EditorGUILayout.GetControlRect(false, 2 * EditorGUIUtility.singleLineHeight);
                        position.height *= 0.5f;
                        speed = EditorGUI.Slider(position, "Animation speed", speed, 0.1f, 5);
                        // Set the rect for the sub-labels
                        position.y += position.height;
                        position.x += EditorGUIUtility.labelWidth;
                        position.width -= EditorGUIUtility.labelWidth + 54;

                        style.alignment = TextAnchor.UpperLeft; EditorGUI.LabelField(position, "0.1", style);
                        style.alignment = TextAnchor.UpperRight; EditorGUI.LabelField(position, "5", style);

                        loopDelay = EditorGUILayout.FloatField("Delay between loop : ", loopDelay);

                        EditorGUILayout.Space(25);

                        EditorGUILayout.BeginHorizontal(GUILayout.Width(windowRect.width / 2));
                        if (GUILayout.Button((Texture)Resources.Load("EditorPlayButton"), GUILayout.Width(35), GUILayout.Height(35)))
                        {
                            if (isInPause)
                            {
                                playOnLoop = false;
                                isInPause = false;
                            }
                            else
                            {
                                time = -0.33f;
                                AnimationMode.StartAnimationMode();
                                needToPlay = true;
                                startTime = EditorApplication.timeSinceStartup;
                                playOnLoop = false;
                            }
                        }
                        if (GUILayout.Button((Texture)Resources.Load("EditorLoopButton"), GUILayout.Width(35), GUILayout.Height(35)))
                        {
                            if (isInPause)
                            {
                                playOnLoop = true;
                                isInPause = false;
                            }
                            else
                            {
                                time = -0.33f;
                                AnimationMode.StartAnimationMode();
                                needToPlay = true;
                                playOnLoop = true;
                                startTime = EditorApplication.timeSinceStartup;
                            }
                            
                        }
                        EditorGUI.BeginDisabledGroup(needToPlay == false);
                        {
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button((Texture)Resources.Load("EditorPauseButton"), GUILayout.Width(35), GUILayout.Height(35)))
                            {
                                isInPause = !isInPause;
                            }
                            if (GUILayout.Button((Texture)Resources.Load("EditorStopButton"), GUILayout.Width(35), GUILayout.Height(35)))
                            {
                                timeInPause = 0;
                                isInPause = false;
                                playOnLoop = false;
                                DisableAnimations();
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.Label("Your animator doesn't have a controller", EditorStyles.boldLabel);
                    }
                    break;
                default:
                    break;
            }
            EditorGUILayout.EndScrollView();
        }
    }

    void Update()
    {
        Repaint();
        if (!EditorApplication.isPlaying)
        {
            if (Selection.activeGameObject)
            {
                if (Selection.activeGameObject.GetComponent<Animator>() && currentAnimator.gameObject != Selection.activeGameObject)
                {
                    currentAnimator = Selection.activeGameObject.GetComponent<Animator>();
                }
            }
            if (needToPlay && !isInPause && !isInWait)
            {
                AnimationMode.SampleAnimationClip(currentAnimator.gameObject, allAnimatorAnimations[animationSelected], time);

                //SceneView.RepaintAll();
                
                //to change => créer son delta time


                if (time >= allAnimatorAnimations[animationSelected].length)
                {
                    if (playOnLoop)
                    {
                        startTime = EditorApplication.timeSinceStartup;
                        timeInPause = 0;
                        if(loopDelay > 0)
                        {
                            isInWait = true;
                            time = 0;
                        }
                    }
                    else
                    {
                        needToPlay = false;
                        AnimationMode.StopAnimationMode();
                        time = 0;
                        timeInPause = 0;
                    }
                }

                time = (float)(((EditorApplication.timeSinceStartup) - startTime) - timeInPause) * speed;
            }
            else if (isInPause)
            {
                timeInPause = (float)((EditorApplication.timeSinceStartup - startTime) - time);
            }
            else if (isInWait)
            {
                timeInWait = (float)(EditorApplication.timeSinceStartup - startTime);
                if(timeInWait >= loopDelay)
                {
                    startTime = EditorApplication.timeSinceStartup;
                    timeInWait = 0;
                    isInWait = false;
                }
            }
        }
        else
        {
            //n'aura plus besoin de le faire + tard
            DisableAnimations();
        }
    }

    private static Animator[] FindAllAnimatorsInScene()
    {
        //pensez à faire bool => si liste vide, afficher message
        List<Animator> animatorList = new List<Animator>();
        foreach (Animator animator in Resources.FindObjectsOfTypeAll(typeof(Animator)) as Animator[])
        {
            if (!EditorUtility.IsPersistent(animator.transform.root.gameObject) && !(animator.hideFlags == HideFlags.NotEditable || animator.hideFlags == HideFlags.HideAndDontSave))
            {
                animatorList.Add(animator);
            }
        }
        Animator[] allAnimatorsInScene = animatorList.ToArray();
        if(allAnimatorsInScene.Length > 0)
        {
            sceneHasAnimators = true;
        }
        else
        {
            sceneHasAnimators = false;
        }
        return allAnimatorsInScene;
    }

}
