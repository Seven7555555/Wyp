using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

/// <summary>
/// 高级UI编辑器 - 完善所有区域的工具提示
/// </summary>
[InitializeOnLoad]
public class AdvancedUIEditor : EditorWindow
{
    // 材质和纹理选择
    private Material selectedMaterial;
    private Texture2D selectedTexture;

    // UI对象引用
    private GameObject previewObject;
    private Image previewImage;

    // 旋转和缩放参数
    private float rotationAngle = 0f;
    private float scaleValue = 1f;

    // 保存数据类
    [System.Serializable]
    public class EditorSaveData
    {
        public string materialPath;
        public string texturePath;
        public float rotation;
        public float scale;
    }

    // 菜单项 - 打开编辑器窗口
    [MenuItem("Tools/Advanced UI Editor")]
    public static void OpenWindow()
    {
        GetWindow<AdvancedUIEditor>("Advanced");
    }

    // 编辑器初始化
    private void OnEnable()
    {
        minSize = new Vector2(500, 600); // 设置窗口最小值
        LoadSettings(); // 启动时尝试加载设置（可选）
    }

    // 绘制编辑器界面（带完整工具提示）
    private void OnGUI()
    {
        #region 标题与说明
        GUILayout.Label("Advanced UI Editor", EditorStyles.boldLabel);
        GUILayout.Space(5);
        GUILayout.Label("支持材质/纹理选择、UI创建、变换调整、保存加载", EditorStyles.miniLabel);
        GUILayout.Space(10);
        #endregion

        #region 材质选择区域（带工具提示）
        EditorGUILayout.LabelField(
            new GUIContent("选择材质", "选择要应用的Material资源"),
            EditorStyles.boldLabel
        );
        selectedMaterial = (Material)EditorGUILayout.ObjectField(
            new GUIContent("材质", "拖入或选择Material资源"),
            selectedMaterial,
            typeof(Material),
            false,
            GUILayout.Width(400)
        );
        GUILayout.Space(5);
        #endregion

        #region 纹理选择区域（带工具提示）
        EditorGUILayout.LabelField(
            new GUIContent("选择纹理", "选择要应用的Texture2D资源"),
            EditorStyles.boldLabel
        );
        selectedTexture = (Texture2D)EditorGUILayout.ObjectField(
            new GUIContent("纹理", "拖入或选择Texture2D资源"),
            selectedTexture,
            typeof(Texture2D),
            false,
            GUILayout.Width(400)
        );
        GUILayout.Space(10);
        #endregion

        #region 创建UI区域（带工具提示）
        EditorGUILayout.LabelField(
            new GUIContent("创建UI对象", "生成预览用的UI Image对象"),
            EditorStyles.boldLabel
        );

        if (GUILayout.Button(
            new GUIContent("创建UI对象", "点击后生成UI预览对象"),
            GUILayout.Width(200)
        ))
        {
            CreateUIObject();
        }

        // 预览区域：增加空判断和工具提示
        if (previewObject != null && previewImage != null)
        {
            GUILayout.Label(
                new GUIContent("预览对象", "实时显示当前材质、纹理及变换效果"),
                EditorStyles.boldLabel
            );
            Rect previewRect = GUILayoutUtility.GetRect(100, 100);
            if (selectedTexture != null)
            {
                EditorGUI.DrawPreviewTexture(previewRect, selectedTexture);
            }

            GUILayout.Label(
                new GUIContent($"旋转角度: {rotationAngle:F1}", "当前UI对象的旋转角度"),
                EditorStyles.label
            );
            GUILayout.Label(
                new GUIContent($"缩放大小: {scaleValue:F1}", "当前UI对象的缩放比例"),
                EditorStyles.label
            );
        }
        GUILayout.Space(10);
        #endregion

        #region 旋转和缩放调整区域（带工具提示）
        EditorGUILayout.LabelField(
            new GUIContent("旋转和缩放调整", "实时修改UI对象的变换参数"),
            EditorStyles.boldLabel
        );

        rotationAngle = EditorGUILayout.Slider(
            new GUIContent("旋转角度", "范围：0-360度，调整UI对象的Z轴旋转"),
            rotationAngle, 0f, 360f
        );

        scaleValue = EditorGUILayout.Slider(
            new GUIContent("缩放大小", "范围：0.1-5.0倍，调整UI对象的缩放比例"),
            scaleValue, 0.1f, 5.0f
        );

        if (previewObject != null)
        {
            previewObject.transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
            previewObject.transform.localScale = new Vector3(scaleValue, scaleValue, 1);
        }
        GUILayout.Space(10);
        #endregion

        #region 功能按钮区域（带工具提示）
        EditorGUILayout.LabelField(
            new GUIContent("功能操作", "重置、保存、加载当前设置"),
            EditorStyles.boldLabel
        );

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(
            new GUIContent("重置", "恢复所有设置为默认值"),
            GUILayout.Width(100)
        ))
        {
            ResetAll();
        }

        if (GUILayout.Button(
            new GUIContent("保存", "将当前设置保存到JSON文件"),
            GUILayout.Width(100)
        ))
        {
            SaveSettings();
        }

        if (GUILayout.Button(
            new GUIContent("加载", "从JSON文件恢复之前保存的设置"),
            GUILayout.Width(100)
        ))
        {
            LoadSettings();
        }

        EditorGUILayout.EndHorizontal();
        #endregion
    }

    /// <summary>
    /// 创建UI对象并应用材质和纹理
    /// </summary>
    private void CreateUIObject()
    {
        if (previewObject != null)
        {
            DestroyImmediate(previewObject);
        }

        previewObject = new GameObject("UIPreview");
        RectTransform rectTransform = previewObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 200);

        previewImage = previewObject.AddComponent<Image>();

        if (selectedMaterial != null)
        {
            previewImage.material = selectedMaterial;
        }

        if (selectedTexture != null)
        {
            previewImage.sprite = Sprite.Create(
                selectedTexture,
                new Rect(0, 0, selectedTexture.width, selectedTexture.height),
                new Vector2(0.5f, 0.5f)
            );
        }
    }

    /// <summary>
    /// 重置所有设置为默认值
    /// </summary>
    private void ResetAll()
    {
        selectedMaterial = null;
        selectedTexture = null;
        rotationAngle = 0f;
        scaleValue = 1f;

        if (previewObject != null)
        {
            DestroyImmediate(previewObject);
        }
        previewImage = null;
    }

    /// <summary>
    /// 保存当前设置到JSON文件
    /// </summary>
    private void SaveSettings()
    {
        try
        {
            EditorSaveData data = new EditorSaveData();
            data.materialPath = selectedMaterial != null ?
                AssetDatabase.GetAssetPath(selectedMaterial) : "";
            data.texturePath = selectedTexture != null ?
                AssetDatabase.GetAssetPath(selectedTexture) : "";
            data.rotation = rotationAngle;
            data.scale = scaleValue;

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(EditorSaveData));
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, data);
                ms.Position = 0;
                using (StreamReader reader = new StreamReader(ms))
                {
                    string json = reader.ReadToEnd();
                    string filePath = Application.dataPath + "/EditorSettings.json";
                    File.WriteAllText(filePath, json);
                    Debug.Log("设置已保存到: " + filePath);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("保存设置时出错: " + e.Message);
        }
    }

    /// <summary>
    /// 从JSON文件加载设置
    /// </summary>
    private void LoadSettings()
    {
        try
        {
            string filePath = Application.dataPath + "/EditorSettings.json";
            if (!File.Exists(filePath))
            {
                Debug.LogWarning("设置文件不存在: " + filePath);
                return;
            }

            string json = File.ReadAllText(filePath);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(EditorSaveData));
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                EditorSaveData data = (EditorSaveData)serializer.ReadObject(ms);

                if (!string.IsNullOrEmpty(data.materialPath))
                {
                    selectedMaterial = AssetDatabase.LoadAssetAtPath<Material>(data.materialPath);
                }
                else
                {
                    selectedMaterial = null;
                }

                if (!string.IsNullOrEmpty(data.texturePath))
                {
                    selectedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(data.texturePath);
                }
                else
                {
                    selectedTexture = null;
                }

                rotationAngle = data.rotation;
                scaleValue = data.scale;

                CreateUIObject(); // 加载后重建UI
                Debug.Log("设置已从文件加载: " + filePath);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("加载设置时出错: " + e.Message);
        }
    }
}