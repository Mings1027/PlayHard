using System.Linq;
using DataControl;
using UnityEditor;
using UnityEngine;

public class StageEditorWindow : EditorWindow
{
    private StageData _currentStage;
    private Vector2 _scrollPosition;
    private float _horizontalOffset;

    private const float BubbleSize = 40f;
    private float _horizontalSpacing = 2f;
    private float _verticalSpacing = 2f;

    private BubbleData[] _availableBubbles;
    private BubbleData _selectedBubbleData;
    private BubbleData _randomBubbleData;

    [MenuItem("Bubble Game/Stage Editor")]
    public static void ShowWindow()
    {
        GetWindow<StageEditorWindow>("Stage Editor");
    }

    private void OnEnable()
    {
        LoadBubbleData();
        LoadRandomBubbleData();
    }

    private void LoadBubbleData()
    {
        // Assets/BubbleData 경로에서 모든 BubbleData 에셋을 로드
        _availableBubbles = AssetDatabase
                            .FindAssets("t:BubbleData", new[] { "Assets/BubbleData" })
                            .Select(guid =>
                                AssetDatabase.LoadAssetAtPath<BubbleData>(AssetDatabase.GUIDToAssetPath(guid)))
                            .Where(bubble => bubble != null)
                            .ToArray();
    }

    private void LoadRandomBubbleData()
    {
        // Random Bubble Data 에셋을 찾거나 생성
        _randomBubbleData = AssetDatabase.LoadAssetAtPath<BubbleData>("Assets/BubbleData/RandomBubble.asset");
        if (_randomBubbleData == null)
        {
            _randomBubbleData = CreateInstance<BubbleData>();
            var serializedObject = new SerializedObject(_randomBubbleData);
            var isRandomProperty = serializedObject.FindProperty("isRandomBubble");
            isRandomProperty.boolValue = true;
            serializedObject.ApplyModifiedProperties();

            AssetDatabase.CreateAsset(_randomBubbleData, "Assets/BubbleData/RandomBubble.asset");
            AssetDatabase.SaveAssets();
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        DrawCurrentStageInfo();
        DrawToolbar();
        DrawSpacingControls();

        if (_currentStage != null)
        {
            DrawStageSettings();
            DrawBubbleSelection();
            DrawBubbleGrid();
        }

        EditorGUILayout.EndVertical();
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

    private void DrawSpacingControls()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Grid Spacing Settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        _horizontalSpacing = EditorGUILayout.Slider("Horizontal Spacing", _horizontalSpacing, 0f, 20f);
        _verticalSpacing = EditorGUILayout.Slider("Vertical Spacing", _verticalSpacing, 0f, 20f);
        EditorGUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
        {
            Repaint();
        }
    }

    private void DrawStageSettings()
    {
        EditorGUI.BeginChangeCheck();
        _currentStage.height = EditorGUILayout.IntSlider("Height", _currentStage.height, 1, 100);
        _currentStage.totalBubbles = EditorGUILayout.IntSlider("Total Bubbles", _currentStage.totalBubbles, 1, 30);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(_currentStage);
        }
    }


    private void DrawBubbleSelection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Bubble Selection", EditorStyles.boldLabel);

        if (_availableBubbles == null || _availableBubbles.Length == 0)
        {
            EditorGUILayout.HelpBox("No bubble data found in Assets/BubbleData folder", MessageType.Warning);
            if (GUILayout.Button("Refresh Bubble Data"))
            {
                LoadBubbleData();
            }

            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.BeginHorizontal();

        for (int i = 0; i < _availableBubbles.Length; i++)
        {
            var bubble = _availableBubbles[i];
            if (!bubble.IsRandomBubble) // 랜덤이 아닌 버블만 표시
            {
                if (GUILayout.Button("", GUILayout.Width(40), GUILayout.Height(40)))
                {
                    _selectedBubbleData = bubble;
                }

                var rect = GUILayoutUtility.GetLastRect();
                EditorGUI.DrawRect(rect, Color.gray);

                if (bubble.BubbleSprite != null)
                {
                    GUI.DrawTexture(rect, bubble.BubbleSprite.texture, ScaleMode.ScaleToFit);
                }

                if (_selectedBubbleData == bubble)
                {
                    EditorGUI.DrawRect(new Rect(rect.x - 2, rect.y - 2, rect.width + 4, 2), Color.white);
                    EditorGUI.DrawRect(new Rect(rect.x - 2, rect.y + rect.height, rect.width + 4, 2), Color.white);
                    EditorGUI.DrawRect(new Rect(rect.x - 2, rect.y - 2, 2, rect.height + 4), Color.white);
                    EditorGUI.DrawRect(new Rect(rect.x + rect.width, rect.y - 2, 2, rect.height + 4), Color.white);
                }
            }
        }

        // 랜덤 버튼
        var originalStyle = GUI.skin.button.fontSize;
        GUI.skin.button.fontSize = 25; // 폰트 크기를 25로 설정
        if (GUILayout.Button("?", GUILayout.Width(40), GUILayout.Height(40)))
        {
            _selectedBubbleData = _randomBubbleData;
        }

        GUI.skin.button.fontSize = originalStyle; // 원래 폰트 크기로 복구


        var randomRect = GUILayoutUtility.GetLastRect();
        if (_selectedBubbleData == _randomBubbleData)
        {
            EditorGUI.DrawRect(new Rect(randomRect.x - 2, randomRect.y - 2, randomRect.width + 4, 2), Color.white);
            EditorGUI.DrawRect(new Rect(randomRect.x - 2, randomRect.y + randomRect.height, randomRect.width + 4, 2),
                Color.white);
            EditorGUI.DrawRect(new Rect(randomRect.x - 2, randomRect.y - 2, 2, randomRect.height + 4), Color.white);
            EditorGUI.DrawRect(new Rect(randomRect.x + randomRect.width, randomRect.y - 2, 2, randomRect.height + 4),
                Color.white);
        }

        EditorGUILayout.EndHorizontal();

        if (_selectedBubbleData != null)
        {
            EditorGUILayout.LabelField(
                $"Selected Bubble: {(_selectedBubbleData.IsRandomBubble ? "Random" : _selectedBubbleData.BubbleType.ToString())}");
        }

        if (GUILayout.Button("Refresh Bubble Data"))
        {
            LoadBubbleData();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawBubbleGrid()
    {
        var scrollViewRect = GUILayoutUtility.GetRect(position.width, position.height - 150);
        _scrollPosition = GUI.BeginScrollView(scrollViewRect, _scrollPosition, GetTotalGridRect());

        var totalRect = GetTotalGridRect();
        EditorGUI.DrawRect(totalRect, new Color(0.2f, 0.2f, 0.2f));

        var cellNumber = 1;

        // Draw all bubbles
        for (var y = 0; y < _currentStage.height; y++)
        {
            var rowWidth = y % 2 == 0 ? _currentStage.Width : _currentStage.Width - 1;
            for (var x = 0; x < rowWidth; x++)
            {
                var gridPos = new Vector2Int(x, y);
                var bubblePos = GetBubblePosition(gridPos);

                // Check if a bubble exists at this position
                var bubbleData = _currentStage.GetBubbleDataAt(gridPos);
                if (bubbleData != null)
                {
                    // Draw bubble with its color
                    DrawBubble(bubblePos, Color.gray);

                    if (bubbleData.IsRandomBubble)
                    {
                        // Draw "?" for random bubbles
                        var style = new GUIStyle(GUI.skin.label)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            normal = { textColor = Color.white },
                            fontSize = 25,
                            fontStyle = FontStyle.Bold
                        };

                        var questionMarkRect = new Rect(
                            bubblePos.x - BubbleSize * 0.5f,
                            bubblePos.y - BubbleSize * 0.5f,
                            BubbleSize,
                            BubbleSize
                        );
                        GUI.Label(questionMarkRect, "?", style);
                    }
                    else if (bubbleData.BubbleSprite != null)
                    {
                        // Draw sprite for non-random bubbles
                        var spriteRect = new Rect(
                            bubblePos.x - BubbleSize * 0.5f,
                            bubblePos.y - BubbleSize * 0.5f,
                            BubbleSize,
                            BubbleSize
                        );
                        GUI.DrawTexture(spriteRect, bubbleData.BubbleSprite.texture, ScaleMode.ScaleToFit);
                    }
                }
                else
                {
                    // Draw empty bubble
                    DrawBubble(bubblePos, Color.gray);
                }

                DrawCellNumber(bubblePos, cellNumber);
                cellNumber++;

                // Mouse interaction handling
                var rect = new Rect(bubblePos.x - BubbleSize * 0.5f, bubblePos.y - BubbleSize * 0.5f,
                    BubbleSize, BubbleSize);

                var mousePosition = Event.current.mousePosition + _scrollPosition;
                if (rect.Contains(mousePosition))
                {
                    // Left click: Place bubble
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        if (_selectedBubbleData != null)
                        {
                            _currentStage.AddBubbleData(gridPos, _selectedBubbleData);
                            Event.current.Use();
                            EditorUtility.SetDirty(_currentStage);
                        }
                    }
                    // Right click: Remove bubble
                    else if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
                    {
                        _currentStage.RemoveBubbleData(gridPos);
                        Event.current.Use();
                        EditorUtility.SetDirty(_currentStage);
                    }
                }
            }
        }

        GUI.EndScrollView();
    }

    private Rect GetTotalGridRect()
    {
        var horizontalStep = BubbleSize + _horizontalSpacing;
        var verticalStep = (BubbleSize + _verticalSpacing) * 0.866f;

        return new Rect(0, 0,
            _currentStage.Width * horizontalStep + horizontalStep,
            _currentStage.height * verticalStep + BubbleSize);
    }

    private Vector2 GetBubblePosition(Vector2Int gridPos)
    {
        var horizontalStep = BubbleSize + _horizontalSpacing;
        var verticalStep = (BubbleSize + _verticalSpacing) * 0.866f;

        var xPos = gridPos.x * horizontalStep;
        var yPos = gridPos.y * verticalStep;

        if (gridPos.y % 2 != 0)
        {
            xPos += horizontalStep * 0.5f;
        }

        return new Vector2(xPos + BubbleSize * 0.5f, yPos + BubbleSize * 0.5f);
    }

    private void DrawCellNumber(Vector2 position, int number)
    {
        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft,
            normal = { textColor = Color.white },
            fontSize = 10,
            fontStyle = FontStyle.Bold
        };

        const float halfSize = BubbleSize * 0.5f;
        var labelRect = new Rect(
            position.x - halfSize + 2, // 왼쪽 여백 2픽셀
            position.y - halfSize + 2, // 위쪽 여백 2픽셀
            BubbleSize,
            BubbleSize
        );

        GUI.Label(labelRect, number.ToString(), style);
    }

    private void DrawBubble(Vector2 position, Color color)
    {
        var halfSize = BubbleSize * 0.5f;
        var rect = new Rect(
            position.x - halfSize,
            position.y - halfSize,
            BubbleSize,
            BubbleSize
        );
        EditorGUI.DrawRect(rect, color);
    }

    private void CreateNewStage()
    {
        _currentStage = CreateInstance<StageData>();
    }

    private void LoadStage()
    {
        var path = EditorUtility.OpenFilePanel("Load Stage", "Assets", "asset");

        if (string.IsNullOrEmpty(path)) return;

        path = "Assets" + path.Substring(Application.dataPath.Length);
        _currentStage = AssetDatabase.LoadAssetAtPath<StageData>(path);
    }

    private void SaveStage()
    {
        if (_currentStage == null) return;

        var path = EditorUtility.SaveFilePanel("Save Stage", "Assets", "NewStage.asset", "asset");

        if (string.IsNullOrEmpty(path)) return;

        path = "Assets" + path.Substring(Application.dataPath.Length);
        AssetDatabase.CreateAsset(_currentStage, path);
        AssetDatabase.SaveAssets();
    }
}