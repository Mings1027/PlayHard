using System.Collections.Generic;
using DataControl;
using UnityEditor;
using UnityEngine;

public class StageEditorWindow : EditorWindow
{
    private StageData currentStage;
    private BubbleType selectedBubbleType = BubbleType.Red;
    private Vector2 scrollPosition;
    private float bubbleRadius = 15f;
    private float bubbleSpacing = 2f;
    private Color[] bubbleColors;
    private Texture2D circleTex;
    private Vector2 gridStartPos; // 그리드의 시작 위치를 저장
    private float horizontalOffset;

    private bool isDragging = false;
    private HashSet<Vector2Int> selectedBubbles = new HashSet<Vector2Int>();  // 이미 선택된 구슬들 추적


    [MenuItem("Bubble Game/Stage Editor")]
    public static void ShowWindow()
    {
        GetWindow<StageEditorWindow>("Stage Editor");
    }

    private void OnEnable()
    {
        bubbleColors = new Color[]
        {
            Color.clear, // None
            Color.red, // Red
            Color.blue, // Blue
            Color.green, // Green
            Color.yellow, // Yellow
            Color.magenta // Purple
        };

        CreateCircleTexture();
    }

    private void CreateCircleTexture()
    {
        int texSize = (int)(bubbleRadius * 2);
        circleTex = new Texture2D(texSize, texSize);

        for (int y = 0; y < texSize; y++)
        {
            for (int x = 0; x < texSize; x++)
            {
                float distance = Vector2.Distance(
                    new Vector2(x, y),
                    new Vector2(texSize / 2, texSize / 2)
                );

                if (distance < texSize / 2)
                {
                    circleTex.SetPixel(x, y, Color.white);
                }
                else
                {
                    circleTex.SetPixel(x, y, Color.clear);
                }
            }
        }

        circleTex.Apply();
    }

    private void OnGUI()
    {
        if (circleTex == null)
        {
            CreateCircleTexture();
        }

        EditorGUILayout.BeginVertical();

        DrawCurrentStageInfo();
        DrawToolbar();

        if (currentStage != null)
        {
            DrawStageSettings();
            DrawBubbleTypeSelector();
            DrawBubbleGrid();
        }

        EditorGUILayout.EndVertical();

        if (Event.current.isMouse)
        {
            HandleMouseInput(Event.current);
        }

        Repaint();
    }

    private void DrawCurrentStageInfo()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Current Stage:", EditorStyles.boldLabel);
       
        var newStage = EditorGUILayout.ObjectField(currentStage, typeof(StageData), false) as StageData;
        if (newStage != null && newStage != currentStage)
        {
            currentStage = newStage;
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("New Stage", EditorStyles.toolbarButton))
        {
            CreateNewStage();
        }

        if (GUILayout.Button("Load Stage", EditorStyles.toolbarButton))
        {
            LoadStage();
        }

        if (GUILayout.Button("Save Stage", EditorStyles.toolbarButton))
        {
            SaveStage();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawStageSettings()
    {
        EditorGUI.BeginChangeCheck();

        currentStage.width = 11;
        int newHeight = EditorGUILayout.IntSlider("Height", currentStage.height, 1, 100);

        if (EditorGUI.EndChangeCheck())
        {
            if ( newHeight != currentStage.height)
            {
                currentStage.height = newHeight;
                currentStage.InitializeGrid();
            }
        }
    }

    private void DrawBubbleTypeSelector()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Selected Bubble Type:");

        EditorGUILayout.BeginHorizontal();
        for (int i = 1; i < System.Enum.GetValues(typeof(BubbleType)).Length; i++)
        {
            BubbleType type = (BubbleType)i;
            if (DrawBubbleButton(type))
            {
                selectedBubbleType = type;
            }
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);
    }

    private bool DrawBubbleButton(BubbleType type)
    {
        GUI.backgroundColor = bubbleColors[(int)type];
        bool clicked;

        if (selectedBubbleType == type)
        {
            clicked = GUILayout.Button(type.ToString(), new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 17
            }, GUILayout.Height(30));
        }
        else
        {
            clicked = GUILayout.Button(type.ToString(), GUI.skin.button, GUILayout.Height(30));
        }


        GUI.backgroundColor = Color.white;
        return clicked;
    }

    private void DrawBubbleGrid()
    {
        if (currentStage.bubbleGrid == null) return;

        Rect scrollViewRect = GUILayoutUtility.GetRect(position.width, position.height - 100);
        scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, GetTotalGridRect());

        gridStartPos = new Vector2(scrollViewRect.x, scrollViewRect.y);

        Rect totalRect = GetTotalGridRect();
        EditorGUI.DrawRect(totalRect, new Color(0.2f, 0.2f, 0.2f));

        // 구슬 그리기
        for (int y = 0; y < currentStage.height; y++)
        {
            // 현재 행의 실제 너비 계산
            int rowWidth = (y % 2 == 0) ? 11 : 10;

            for (int x = 0; x < rowWidth; x++)
            {
                Vector2 bubblePos = GetBubblePosition(new Vector2Int(x, y));
                DrawBubble(bubblePos, x, y);
            }
        }

        GUI.EndScrollView();
    }

    private Rect GetTotalGridRect()
    {
        float diameter = bubbleRadius * 2 + bubbleSpacing;
        return new Rect(0, 0,
            currentStage.width * diameter + diameter,
            currentStage.height * diameter * 0.866f + diameter);
    }

    private Vector2 GetBubblePosition(Vector2Int gridPos)
    {
        float diameter = bubbleRadius * 2 + bubbleSpacing;
        float xPos = gridPos.x * diameter;
        float yPos = gridPos.y * diameter * 0.866f;

        if (gridPos.y % 2 != 0)
        {
            xPos += diameter * 0.5f;
        }

        return new Vector2(xPos + bubbleRadius, yPos + bubbleRadius);
    }

    private void DrawBubble(Vector2 position, int x, int y)
    {
        BubbleType bubbleType = currentStage.bubbleGrid[x, y];
        Rect bubbleRect = new Rect(
            position.x - bubbleRadius,
            position.y - bubbleRadius,
            bubbleRadius * 2,
            bubbleRadius * 2
        );

        // 비어있는 위치에도 옅은 원 그리기
        if (bubbleType == BubbleType.None)
        {
            GUI.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            GUI.DrawTexture(bubbleRect, circleTex);
        }
        else
        {
            // 구슬 그리기
            GUI.color = bubbleColors[(int)bubbleType];
            GUI.DrawTexture(bubbleRect, circleTex);
        }

        GUI.color = Color.white;
    }


   
    private void HandleMouseInput(Event e)
    {
        if (currentStage == null) return;

        // 마우스 위치 계산
        Vector2 mousePos = e.mousePosition;
        mousePos.y -= gridStartPos.y;
        mousePos += scrollPosition;

        switch (e.type)
        {
            case EventType.MouseDown when e.button == 0:
                isDragging = true;
                selectedBubbles.Clear();
                SelectBubbleAtPosition(mousePos);
                e.Use();
                break;
            
            case EventType.MouseDrag when e.button == 0 && isDragging:
                SelectBubbleAtPosition(mousePos);
                e.Use();
                break;
            
            case EventType.MouseUp when e.button == 0:
                isDragging = false;
                selectedBubbles.Clear();
                e.Use();
                break;
        }
    }
    private void SelectBubbleAtPosition(Vector2 mousePos)
    {
        for (int y = 0; y < currentStage.height; y++)
        {
            int rowWidth = (y % 2 == 0) ? 11 : 10;
   
            for (int x = 0; x < rowWidth; x++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                Vector2 bubblePos = GetBubblePosition(gridPos);
                float distance = Vector2.Distance(mousePos, bubblePos);

                if (distance <= bubbleRadius)
                {
                    // 이미 선택된 구슬인 경우 선택 해제
                    if (!selectedBubbles.Contains(gridPos))
                    {
                        if (currentStage.bubbleGrid[x, y] == selectedBubbleType)
                        {
                            Undo.RecordObject(currentStage, "Deselect Bubble");
                            currentStage.bubbleGrid[x, y] = BubbleType.None;
                        }
                        else
                        {
                            Undo.RecordObject(currentStage, "Select Bubble");
                            currentStage.bubbleGrid[x, y] = selectedBubbleType;
                        }
                        EditorUtility.SetDirty(currentStage);
                        selectedBubbles.Add(gridPos);
                    }
                    return;
                }
            }
        }
    }

    private void CreateNewStage()
    {
        currentStage = CreateInstance<StageData>();
        currentStage.width = 11;
        currentStage.InitializeGrid();
    }

    private void LoadStage()
    {
        string path = EditorUtility.OpenFilePanel("Load Stage", "Assets", "asset");

        if (!string.IsNullOrEmpty(path))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
            currentStage = AssetDatabase.LoadAssetAtPath<StageData>(path);
        }
    }

    private void SaveStage()
    {
        if (currentStage == null) return;

        string path = EditorUtility.SaveFilePanel("Save Stage", "Assets", "NewStage.asset", "asset");

        if (!string.IsNullOrEmpty(path))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
            AssetDatabase.CreateAsset(currentStage, path);
            AssetDatabase.SaveAssets();
        }
    }

    private void OnDestroy()
    {
        if (circleTex != null)
        {
            DestroyImmediate(circleTex);
        }
    }
}