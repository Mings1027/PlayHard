using System.Collections.Generic;
using System.Linq;
using DataControl;
using HelperFolder;
using UnityEditor;
using UnityEngine;

public class StageEditorWindow : EditorWindow
{
    private StageData _currentStage;
    private Vector2 _scrollPosition;
    private float _horizontalOffset;
    private StageData[] _availableStages;
    private int _currentStageIndex;

    private const float BubbleSize = 40f;
    private const float HorizontalSpacing = 3f;
    private const float VerticalSpacing = 10f;

    private List<BubbleData> _availableBubbles;
    private BubbleData _selectedBubbleData;
    private BubbleData _randomBubbleData;

    [MenuItem("Bubble Game/Stage Editor")]
    public static void ShowWindow()
    {
        GetWindow<StageEditorWindow>("Stage Editor");
    }

    private void OnEnable()
    {
        LoadStageData();
        LoadBubbleData();
        LoadRandomBubbleData();
    }

    private void LoadStageData()
    {
        // Find all StageData assets in the StageData folder
        var guids = AssetDatabase.FindAssets("t:StageData", new[] { "Assets/StageData" });
        _availableStages = guids
                           .Select(guid =>
                               AssetDatabase.LoadAssetAtPath<StageData>(AssetDatabase.GUIDToAssetPath(guid)))
                           .OrderBy(stage =>
                           {
                               string assetPath = AssetDatabase.GetAssetPath(stage);
                               string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                               if (fileName.StartsWith("Level_") &&
                                   int.TryParse(fileName.Substring("Level_".Length), out int level))
                               {
                                   return level;
                               }

                               return int.MaxValue;
                           })
                           .ToArray();

        // Load the first stage if available
        if (_availableStages != null && _availableStages.Length > 0)
        {
            _currentStageIndex = 0;
            _currentStage = _availableStages[_currentStageIndex];
        }
        else
        {
            Debug.LogWarning("No stage data found in Assets/StageData folder");
        }
    }

    private void LoadBubbleData()
    {
        _availableBubbles ??= new List<BubbleData>();
        _availableBubbles.Clear();
        var guids = AssetDatabase.FindAssets("t:BubbleData", new[] { "Assets/BubbleData" });

        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var bubbleData = AssetDatabase.LoadAssetAtPath<BubbleData>(assetPath);

            _availableBubbles.Add(bubbleData);
        }
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

        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        DrawCurrentStageInfo();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);


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
        // 좌우 화살표 버튼과 현재 스테이지 정보 표시
        var buttonStyle = new GUIStyle(EditorStyles.miniButton)
        {
            fixedWidth = 100,
            fixedHeight = 20,
            fontSize = 16,
            fontStyle = FontStyle.Bold
        };

        if (GUILayout.Button("←", buttonStyle))
        {
            NavigateStage(-1);
        }

        GUILayout.Label("Level", GUILayout.Width(50));

        EditorGUI.BeginChangeCheck();

        // 현재 스테이지의 레벨 번호 추출
        int currentLevel = 1;
        if (_currentStage != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(_currentStage);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            if (fileName.StartsWith("Level_"))
            {
                string numberPart = fileName.Substring("Level_".Length);
                int.TryParse(numberPart, out currentLevel);
            }
        }

        // 레벨 번호 입력 필드
        int newLevel = EditorGUILayout.IntField(currentLevel, GUILayout.Width(100));

        if (EditorGUI.EndChangeCheck())
        {
            // 입력된 레벨 번호에 해당하는 스테이지 찾기
            string targetStagePath = $"Level_{newLevel}";
            int newIndex = -1;

            for (int i = 0; i < _availableStages.Length; i++)
            {
                string assetPath = AssetDatabase.GetAssetPath(_availableStages[i]);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

                if (fileName.Equals(targetStagePath))
                {
                    newIndex = i;
                    break;
                }
            }

            // 해당하는 스테이지가 있으면 로드
            if (newIndex != -1)
            {
                _currentStageIndex = newIndex;
                _currentStage = _availableStages[_currentStageIndex];
            }
        }

        if (GUILayout.Button("→", buttonStyle))
        {
            NavigateStage(1);
        }

        if (GUILayout.Button("+", buttonStyle))
        {
            CreateNewStage();
        }
    }

    private void NavigateStage(int direction)
    {
        if (_availableStages == null || _availableStages.Length == 0)
            return;
        _currentStageIndex = (_currentStageIndex + direction + _availableStages.Length) % _availableStages.Length;
        _currentStage = _availableStages[_currentStageIndex];
        GUI.changed = true;
    }

    private void DrawStageSettings()
    {
        EditorGUI.BeginChangeCheck();
        var serializedObject = new SerializedObject(_currentStage);

        var bossStageProperty = serializedObject.FindProperty("bossStage");
        bossStageProperty.boolValue = EditorGUILayout.Toggle("Boss Stage", bossStageProperty.boolValue);

        var maxBubbleHeight = _currentStage.BubbleDataPositions.Count > 0
            ? _currentStage.BubbleDataPositions.Max(b => b.bubblePosition.y) + 1
            : 1;

        var heightProperty = serializedObject.FindProperty("height");
        heightProperty.intValue = EditorGUILayout.IntSlider("Height", heightProperty.intValue, 1, 100);

        if (heightProperty.intValue < maxBubbleHeight)
        {
            heightProperty.intValue = maxBubbleHeight;
        }

        var bubbleAmmoProperty = serializedObject.FindProperty("bubbleAmmo");
        bubbleAmmoProperty.intValue = EditorGUILayout.IntSlider("Bubble Ammo", bubbleAmmoProperty.intValue, 1, 100);

        serializedObject.ApplyModifiedProperties();

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(_currentStage);
        }
    }

    private void DrawBubbleSelection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Bubble Selection", EditorStyles.boldLabel);
        if (_selectedBubbleData != null)
        {
            string bubbleInfo;
            if (_selectedBubbleData.IsBossBubble)
            {
                bubbleInfo = "Boss Bubble";
            }
            else if (_selectedBubbleData.IsRandomBubble)
            {
                bubbleInfo = "Random";
            }
            else if (_selectedBubbleData.IsSpecialBubble)
            {
                bubbleInfo = $"{_selectedBubbleData.SpecialBubbleType}";
            }
            else
            {
                bubbleInfo = $"{_selectedBubbleData.BubbleType}";
            }

            EditorGUILayout.LabelField($"- {bubbleInfo}", EditorStyles.boldLabel);
        }

        EditorGUILayout.EndHorizontal();

        if (_availableBubbles == null || _availableBubbles.Count == 0)
        {
            EditorGUILayout.HelpBox("No bubble data found in BubbleGlobalSetting", MessageType.Warning);
            if (GUILayout.Button("Refresh Bubble Data"))
            {
                LoadBubbleData();
            }

            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.BeginHorizontal();

        for (var i = 0; i < _availableBubbles.Count; i++)
        {
            var bubbleData = _availableBubbles[i];
            if (bubbleData != null)
            {
                if (bubbleData.IsBossBubble && !_currentStage.BossStage)
                    continue;

                if (!bubbleData.IsRandomBubble)
                {
                    if (GUILayout.Button("", GUILayout.Width(40), GUILayout.Height(40)))
                    {
                        _selectedBubbleData = bubbleData;
                    }

                    var rect = GUILayoutUtility.GetLastRect();
                    EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f));

                    if (bubbleData.BubbleSprite != null)
                    {
                        GUI.DrawTexture(rect, bubbleData.BubbleSprite.texture, ScaleMode.ScaleToFit);
                    }

                    if (_selectedBubbleData == bubbleData)
                    {
                        EditorGUI.DrawRect(new Rect(rect.x - 2, rect.y - 2, rect.width + 4, 2), Color.white);
                        EditorGUI.DrawRect(new Rect(rect.x - 2, rect.y + rect.height, rect.width + 4, 2), Color.white);
                        EditorGUI.DrawRect(new Rect(rect.x - 2, rect.y - 2, 2, rect.height + 4), Color.white);
                        EditorGUI.DrawRect(new Rect(rect.x + rect.width, rect.y - 2, 2, rect.height + 4), Color.white);
                    }
                }
            }
        }

        // 랜덤 버튼
        var originalStyle = GUI.skin.button.fontSize;
        GUI.skin.button.fontSize = 25;
        if (GUILayout.Button("?", GUILayout.Width(40), GUILayout.Height(40)))
        {
            _selectedBubbleData = _randomBubbleData;
        }

        GUI.skin.button.fontSize = originalStyle;

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

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh Bubble Data"))
        {
            LoadBubbleData();
        }

        if (GUILayout.Button("Clear"))
        {
            if (EditorUtility.DisplayDialog("Clear All Bubbles", "Clear all bubbles in this stage?", "Yes", "No"))
            {
                _currentStage.BubbleDataPositions.Clear();
                EditorUtility.SetDirty(_currentStage);
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawBubbleGrid()
    {
        var viewRect = GUILayoutUtility.GetRect(position.width, position.height - 150);
        var gridWidth = GetTotalGridWidth();
        float leftMargin = CalculateLeftMargin(viewRect.width, gridWidth);

        var contentRect = SetupScrollView(viewRect);
        EditorGUI.DrawRect(contentRect, new Color(0.2f, 0.2f, 0.2f));

        DrawAllBubbles(leftMargin);
        GUI.EndScrollView();
    }

    private float GetTotalGridWidth()
    {
        var horizontalStep = BubbleSize + HorizontalSpacing;
        return _currentStage.Width * horizontalStep;
    }

    private float CalculateLeftMargin(float viewWidth, float gridWidth)
    {
        float leftMargin = (viewWidth - gridWidth) * 0.5f;
        return Mathf.Max(0, leftMargin);
    }

    private Rect SetupScrollView(Rect viewRect)
    {
        var totalHeight = GetTotalGridHeight();
        var contentRect = new Rect(0, 0, viewRect.width, totalHeight);

        // 스크롤 위치를 y축으로만 제한
        _scrollPosition.x = 0;
        _scrollPosition.y = Mathf.Clamp(_scrollPosition.y, 0, Mathf.Max(0, totalHeight - viewRect.height));

        _scrollPosition = GUI.BeginScrollView(viewRect,
            new Vector2(0, _scrollPosition.y),
            contentRect,
            false,
            true);

        return contentRect;
    }

    private void DrawAllBubbles(float leftMargin)
    {
        var cellNumber = 1;
        for (var y = 0; y < _currentStage.Height; y++)
        {
            var rowWidth = y % 2 == 0 ? _currentStage.Width : _currentStage.Width - 1;
            for (var x = 0; x < rowWidth; x++)
            {
                DrawBubbleCell(new Vector2Int(x, y), leftMargin, cellNumber);
                cellNumber++;
            }
        }
    }

    private void DrawBubbleCell(Vector2Int gridPos, float leftMargin, int cellNumber)
    {
        var bubblePos = GetBubblePosition(gridPos);
        bubblePos.x += leftMargin;

        var rect = GetBubbleRect(bubblePos);
        var isHovered = IsCellHovered(rect);

        DrawBaseBubbleBackground(bubblePos, isHovered);
        DrawBubbleContent(gridPos, bubblePos);
        DrawCellNumber(bubblePos, cellNumber);
        HandleBubbleInteraction(rect, gridPos);
    }

    private Rect GetBubbleRect(Vector2 bubblePos)
    {
        return new Rect(
            bubblePos.x - BubbleSize * 0.5f,
            bubblePos.y - BubbleSize * 0.5f,
            BubbleSize,
            BubbleSize
        );
    }

    private bool IsCellHovered(Rect rect)
    {
        var mousePosition = Event.current.mousePosition;
        return rect.Contains(mousePosition);
    }

    private void DrawBaseBubbleBackground(Vector2 bubblePos, bool isHovered)
    {
        DrawBubble(bubblePos, new Color(0.3f, 0.3f, 0.3f), isHovered);
    }

    private void DrawBubbleContent(Vector2Int gridPos, Vector2 bubblePos)
    {
        var baseBubbleData = _currentStage.GetBaseBubbleDataAt(gridPos);
        if (baseBubbleData != null)
        {
            DrawBaseBubble(bubblePos, baseBubbleData);
            DrawSpecialBubbleOverlay(gridPos, bubblePos);
        }
    }

    private void DrawBaseBubble(Vector2 bubblePos, BubbleData bubbleData)
    {
        var spriteRect = GetBubbleRect(bubblePos);

        if (bubbleData.IsRandomBubble)
        {
            DrawRandomBubble(spriteRect);
        }
        else if (bubbleData.BubbleSprite != null)
        {
            GUI.DrawTexture(spriteRect, bubbleData.BubbleSprite.texture, ScaleMode.ScaleToFit);
        }
    }

    private void DrawRandomBubble(Rect rect)
    {
        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
            fontSize = 25,
            fontStyle = FontStyle.Bold
        };

        GUI.Label(rect, "?", style);
    }

    private void DrawSpecialBubbleOverlay(Vector2Int gridPos, Vector2 bubblePos)
    {
        var specialBubbleData = _currentStage.GetSpecialBubbleDataAt(gridPos);
        if (specialBubbleData?.OverlaySprite != null)
        {
            var overlayRect = GetBubbleRect(bubblePos);
            GUI.DrawTexture(overlayRect, specialBubbleData.OverlaySprite.texture, ScaleMode.ScaleToFit);
        }
    }

    private void HandleBubbleInteraction(Rect rect, Vector2Int gridPos)
    {
        if (!IsCellHovered(rect)) return;

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            HandleLeftClick(gridPos);
        }
        else if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
        {
            HandleRightClick(gridPos);
        }
    }

    private void HandleLeftClick(Vector2Int gridPos)
    {
        if (_selectedBubbleData != null)
        {
            _currentStage.AddBubbleData(gridPos, _selectedBubbleData);
            Event.current.Use();
            EditorUtility.SetDirty(_currentStage);
        }
    }

    private void HandleRightClick(Vector2Int gridPos)
    {
        _currentStage.RemoveBubbleData(gridPos);
        Event.current.Use();
        EditorUtility.SetDirty(_currentStage);
    }

    private float GetTotalGridHeight()
    {
        var verticalStep = (BubbleSize + VerticalSpacing) *
                           Mathf.Sin(BubbleMatchHelper.HexagonAngle);
        return _currentStage.Height * verticalStep + BubbleSize;
    }

    private Vector2 GetBubblePosition(Vector2Int gridPos)
    {
        var horizontalStep = BubbleSize + HorizontalSpacing;
        var verticalStep = (BubbleSize + VerticalSpacing) *
                           Mathf.Sin(BubbleMatchHelper.HexagonAngle);

        var xPos = gridPos.x * horizontalStep;
        var yPos = gridPos.y * verticalStep;

        if (gridPos.y % 2 != 0)
        {
            xPos += horizontalStep * Mathf.Cos(BubbleMatchHelper.HexagonAngle);
        }

        return new Vector2(
            xPos + BubbleSize * Mathf.Cos(BubbleMatchHelper.HexagonAngle),
            yPos + BubbleSize * Mathf.Cos(BubbleMatchHelper.HexagonAngle)
        );
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

    private void DrawBubble(Vector2 position, Color color, bool isHovered)
    {
        var halfSize = BubbleSize * 0.5f;
        var rect = new Rect(
            position.x - halfSize,
            position.y - halfSize,
            BubbleSize,
            BubbleSize
        );

        // 호버 상태일 때 더 밝은 색상 사용
        var bubbleColor = isHovered ? new Color(0.4f, 0.4f, 0.4f) : color;
        EditorGUI.DrawRect(rect, bubbleColor);
    }

    private void CreateNewStage()
    {
        // 새 스테이지 생성
        _currentStage = CreateInstance<StageData>();

        // Assets/StageData 폴더에서 현재 존재하는 가장 큰 레벨 번호 찾기
        int maxLevelNumber = 0;
        var guids = AssetDatabase.FindAssets("Level_", new[] { "Assets/StageData" });

        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

            if (fileName.StartsWith("Level_"))
            {
                string numberPart = fileName.Substring("Level_".Length);
                if (int.TryParse(numberPart, out int levelNumber))
                {
                    maxLevelNumber = Mathf.Max(maxLevelNumber, levelNumber);
                }
            }
        }

        // 다음 레벨 번호 생성
        int nextLevelNumber = maxLevelNumber + 1;

        // Assets/StageData 폴더가 없다면 생성
        if (!AssetDatabase.IsValidFolder("Assets/StageData"))
        {
            AssetDatabase.CreateFolder("Assets", "StageData");
        }

        // 새 스테이지 저장
        string newStagePath = $"Assets/StageData/Level_{nextLevelNumber}.asset";

        // 에셋 생성 및 저장
        AssetDatabase.CreateAsset(_currentStage, newStagePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // _availableStages 배열 업데이트
        if (_availableStages == null || _availableStages.Length == 0)
        {
            _availableStages = new StageData[] { _currentStage };
            _currentStageIndex = 0;
        }
        else
        {
            // 기존 배열에 새 스테이지 추가
            var newArray = new StageData[_availableStages.Length + 1];
            _availableStages.CopyTo(newArray, 0);
            newArray[newArray.Length - 1] = _currentStage;
            _availableStages = newArray;
            _currentStageIndex = _availableStages.Length - 1;
        }

        Debug.Log($"Created new stage: Level_{nextLevelNumber}");
    }
}