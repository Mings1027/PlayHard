using System.Collections.Generic;
using DataControl;
using UnityEditor;
using UnityEngine;

public class StageEditorWindow : EditorWindow
{
    private StageData _currentStage;
    private BubblePath _currentPath;

    // private BubbleType _selectedBubbleType = BubbleType.Red;
    private Vector2 _scrollPosition;
    private Texture2D _circleTex;
    private Vector2 _gridStartPos; // 그리드의 시작 위치를 저장
    private float _horizontalOffset;

    private bool _isDragging;

    private const float BubbleRadius = 15f;
    private const float BubbleSpacing = 2f;

    private readonly HashSet<Vector2Int> _selectedBubbles = new(); // 이미 선택된 구슬들 추적
    private readonly Dictionary<BubblePath, Color> _pathColors = new();

    private static readonly Color[] PathColorPalette =
    {
        new(1f, 0.4f, 0.4f), // 빨간색
        new(0.4f, 0.8f, 0.4f), // 초록색
        new(0.4f, 0.4f, 1f), // 파란색
        new(1f, 0.8f, 0.4f), // 노란색
        new(0.8f, 0.4f, 1f), // 보라색
        new(0.4f, 0.8f, 0.8f), // 청록색
    };

    [MenuItem("Bubble Game/Stage Editor")]
    public static void ShowWindow()
    {
        GetWindow<StageEditorWindow>("Stage Editor");
    }

    private void OnEnable()
    {
        CreateCircleTexture();
    }

    private void CreateCircleTexture()
    {
        int texSize = (int)(BubbleRadius * 2);
        _circleTex = new Texture2D(texSize, texSize);

        for (int y = 0; y < texSize; y++)
        {
            for (int x = 0; x < texSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(texSize / 2, texSize / 2));

                if (distance < texSize / 2)
                {
                    _circleTex.SetPixel(x, y, Color.gray);
                }
                else
                {
                    _circleTex.SetPixel(x, y, Color.clear);
                }
            }
        }

        _circleTex.Apply();
    }

    private void OnGUI()
    {
        if (_circleTex == null)
        {
            CreateCircleTexture();
        }

        EditorGUILayout.BeginVertical();

        DrawCurrentStageInfo();
        DrawToolbar();

        if (_currentStage != null)
        {
            DrawStageSettings();
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

        var newStage = EditorGUILayout.ObjectField(_currentStage, typeof(StageData), false) as StageData;
        if (newStage != null && newStage != _currentStage)
        {
            _currentStage = newStage;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox(
            "Left Click + Drag: Create new path\n" +
            "Right Click: Delete path\n" +
            "Clear All Paths: Delete all paths",
            MessageType.Info);

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

        GUILayout.FlexibleSpace(); // 버튼들 사이에 간격 추가

        if (GUILayout.Button("Clear All Paths", EditorStyles.toolbarButton))
        {
            if (_currentStage != null && EditorUtility.DisplayDialog("Clear All Paths",
                    "Are you sure you want to delete all paths?", "Yes", "No"))
            {
                ClearAllPaths();
            }
        }
    }

    private void DrawStageSettings()
    {
        EditorGUI.BeginChangeCheck();
        _currentStage.height = EditorGUILayout.IntSlider("Height", _currentStage.height, 1, 100);

        EditorGUILayout.LabelField($"Total Points: {_currentStage.TotalPoints}");
        _currentStage.totalBubbles = EditorGUILayout.IntSlider("Total Bubbles", _currentStage.totalBubbles, 1, 30);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(_currentStage);
        }
    }


    private void DrawBubbleGrid()
    {
        Rect scrollViewRect = GUILayoutUtility.GetRect(position.width, position.height - 100);
        _scrollPosition = GUI.BeginScrollView(scrollViewRect, _scrollPosition, GetTotalGridRect());

        _gridStartPos = new Vector2(scrollViewRect.x, scrollViewRect.y);

        Rect totalRect = GetTotalGridRect();
        EditorGUI.DrawRect(totalRect, new Color(0.2f, 0.2f, 0.2f));

        // 먼저 모든 경로의 색상을 할당
        EnsurePathColors();

        // 경로에 있는 버블 그리기
        for (int y = 0; y < _currentStage.height; y++)
        {
            int rowWidth = (y % 2 == 0) ? 11 : 10;
            for (int x = 0; x < rowWidth; x++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                Vector2 bubblePos = GetBubblePosition(gridPos);

                var path = _currentStage.GetBubblePathAt(gridPos);
                if (path != null)
                {
                    // 경로에 속한 버블은 해당 경로의 색상으로 그리기
                    DrawBubble(bubblePos, _pathColors[path]);

                    // 순서 번호 표시
                    int index = path.points.IndexOf(gridPos) + 1;
                    DrawBubbleNumber(bubblePos, index);
                }
                else
                {
                    // 경로에 속하지 않은 버블은 회색으로 그리기
                    DrawBubble(bubblePos, Color.gray);
                }
            }
        }

        // 경로 선 그리기
        foreach (var path in _currentStage.bubblePaths)
        {
            if (path.points.Count < 2) continue;

            Color pathColor = _pathColors[path];
            for (int i = 1; i < path.points.Count; i++)
            {
                Vector2 start = GetBubblePosition(path.points[i - 1]);
                Vector2 end = GetBubblePosition(path.points[i]);

                Handles.BeginGUI();
                Handles.color = pathColor;
                Handles.DrawLine(start, end);
                Handles.EndGUI();
            }
        }

        GUI.EndScrollView();
    }

    private void EnsurePathColors()
    {
        // 새로운 경로에 색상 할당
        foreach (var path in _currentStage.bubblePaths)
        {
            if (!_pathColors.ContainsKey(path))
            {
                int colorIndex = _pathColors.Count % PathColorPalette.Length;
                _pathColors[path] = PathColorPalette[colorIndex];
            }
        }
    }

    private Rect GetTotalGridRect()
    {
        float diameter = BubbleRadius * 2 + BubbleSpacing;
        return new Rect(0, 0,
            _currentStage.width * diameter + diameter,
            _currentStage.height * diameter * 0.866f + diameter);
    }

    private Vector2 GetBubblePosition(Vector2Int gridPos)
    {
        float diameter = BubbleRadius * 2 + BubbleSpacing;
        float xPos = gridPos.x * diameter;
        float yPos = gridPos.y * diameter * 0.866f;

        if (gridPos.y % 2 != 0)
        {
            xPos += diameter * 0.5f;
        }

        return new Vector2(xPos + BubbleRadius, yPos + BubbleRadius);
    }

    private void DrawBubbleNumber(Vector2 position, int number)
    {
        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };

        Rect labelRect = new Rect(
            position.x - BubbleRadius,
            position.y - BubbleRadius,
            BubbleRadius * 2,
            BubbleRadius * 2
        );

        GUI.Label(labelRect, number.ToString(), style);
    }

    private void DrawBubble(Vector2 position, Color color)
    {
        Color originalColor = GUI.color;
        GUI.color = color;

        Rect bubbleRect = new Rect(
            position.x - BubbleRadius,
            position.y - BubbleRadius,
            BubbleRadius * 2,
            BubbleRadius * 2
        );
        GUI.DrawTexture(bubbleRect, _circleTex);

        GUI.color = originalColor;
    }

    private void HandleMouseInput(Event e)
    {
        if (_currentStage == null) return;

        Vector2 mousePos = e.mousePosition;
        mousePos.y -= _gridStartPos.y;
        mousePos += _scrollPosition;

        switch (e.type)
        {
            case EventType.MouseDown when e.button == 0:
                _isDragging = true;
                _selectedBubbles.Clear();
                _currentPath = new BubblePath();
                _currentStage.bubblePaths.Add(_currentPath);
                SelectBubbleAtPosition(mousePos);
                _currentStage.UpdateTotalPoints();
                e.Use();
                break;

            case EventType.MouseDown when e.button == 1: // 우클릭
                DeletePathAtPosition(mousePos);
                _currentStage.UpdateTotalPoints();
                e.Use();
                break;

            case EventType.MouseDrag when e.button == 0 && _isDragging:
                SelectBubbleAtPosition(mousePos);
                _currentStage.UpdateTotalPoints();
                e.Use();
                break;

            case EventType.MouseUp when e.button == 0:
                _isDragging = false;
                _selectedBubbles.Clear();
                // 포인트가 없거나 하나뿐인 경로는 제거
                if (_currentPath != null && _currentPath.points.Count <= 1)
                {
                    _currentStage.bubblePaths.Remove(_currentPath);
                    _currentStage.UpdateTotalPoints();
                }

                _currentPath = null;
                e.Use();
                break;
        }
    }

    private void SelectBubbleAtPosition(Vector2 mousePos)
    {
        for (int y = 0; y < _currentStage.height; y++)
        {
            int rowWidth = y % 2 == 0 ? _currentStage.width : _currentStage.width - 1;

            for (int x = 0; x < rowWidth; x++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                Vector2 bubblePos = GetBubblePosition(gridPos);
                float distance = Vector2.Distance(mousePos, bubblePos);

                if (distance <= BubbleRadius)
                {
                    if (_selectedBubbles.Contains(gridPos)) return;

                    EditorUtility.SetDirty(_currentStage);
                    _selectedBubbles.Add(gridPos);

                    // 경로에 포인트 추가
                    _currentPath?.points.Add(gridPos);

                    return;
                }
            }
        }
    }

    private void DeletePathAtPosition(Vector2 mousePos)
    {
        for (int y = 0; y < _currentStage.height; y++)
        {
            int rowWidth = y % 2 == 0 ? _currentStage.width : _currentStage.width - 1;

            for (int x = 0; x < rowWidth; x++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                Vector2 bubblePos = GetBubblePosition(gridPos);
                float distance = Vector2.Distance(mousePos, bubblePos);

                if (distance <= BubbleRadius)
                {
                    BubblePath pathToDelete = _currentStage.GetBubblePathAt(gridPos);
                    if (pathToDelete != null)
                    {
                        // 우클릭한 위치의 인덱스 찾기
                        int clickedIndex = pathToDelete.points.IndexOf(gridPos);
                        if (clickedIndex >= 0)
                        {
                            // 우클릭한 위치 이후의 포인트들만 제거
                            int removeCount = pathToDelete.points.Count - clickedIndex;
                            pathToDelete.points.RemoveRange(clickedIndex, removeCount);

                            // 포인트가 모두 제거된 경우 경로 자체를 제거
                            if (pathToDelete.points.Count == 0)
                            {
                                _currentStage.bubblePaths.Remove(pathToDelete);
                                _pathColors.Remove(pathToDelete);
                            }

                            EditorUtility.SetDirty(_currentStage);
                        }
                    }

                    return;
                }
            }
        }
    }

    private void CreateNewStage()
    {
        _currentStage = CreateInstance<StageData>();
    }

    private void LoadStage()
    {
        string path = EditorUtility.OpenFilePanel("Load Stage", "Assets", "asset");

        if (string.IsNullOrEmpty(path)) return;

        path = "Assets" + path.Substring(Application.dataPath.Length);
        _currentStage = AssetDatabase.LoadAssetAtPath<StageData>(path);
    }

    private void SaveStage()
    {
        if (_currentStage == null) return;

        string path = EditorUtility.SaveFilePanel("Save Stage", "Assets", "NewStage.asset", "asset");

        if (string.IsNullOrEmpty(path)) return;

        path = "Assets" + path.Substring(Application.dataPath.Length);
        AssetDatabase.CreateAsset(_currentStage, path);
        AssetDatabase.SaveAssets();
    }

    private void ClearAllPaths()
    {
        if (_currentStage != null)
        {
            _currentStage.bubblePaths.Clear();
            _currentStage.UpdateTotalPoints();
            _pathColors.Clear();
            _currentPath = null;
            EditorUtility.SetDirty(_currentStage);
        }
    }

    private void OnDestroy()
    {
        if (_circleTex != null)
        {
            DestroyImmediate(_circleTex);
        }
    }
}