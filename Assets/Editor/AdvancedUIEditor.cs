using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

/// <summary>
/// �߼�UI�༭�� - ������������Ĺ�����ʾ
/// </summary>
[InitializeOnLoad]
public class AdvancedUIEditor : EditorWindow
{
    // ���ʺ�����ѡ��
    private Material selectedMaterial;
    private Texture2D selectedTexture;

    // UI��������
    private GameObject previewObject;
    private Image previewImage;

    // ��ת�����Ų���
    private float rotationAngle = 0f;
    private float scaleValue = 1f;

    // ����������
    [System.Serializable]
    public class EditorSaveData
    {
        public string materialPath;
        public string texturePath;
        public float rotation;
        public float scale;
    }

    // �˵��� - �򿪱༭������
    [MenuItem("Tools/Advanced UI Editor")]
    public static void OpenWindow()
    {
        GetWindow<AdvancedUIEditor>("Advanced");
    }

    // �༭����ʼ��
    private void OnEnable()
    {
        minSize = new Vector2(500, 600); // ���ô�����Сֵ
        LoadSettings(); // ����ʱ���Լ������ã���ѡ��
    }

    // ���Ʊ༭�����棨������������ʾ��
    private void OnGUI()
    {
        #region ������˵��
        GUILayout.Label("Advanced UI Editor", EditorStyles.boldLabel);
        GUILayout.Space(5);
        GUILayout.Label("֧�ֲ���/����ѡ��UI�������任�������������", EditorStyles.miniLabel);
        GUILayout.Space(10);
        #endregion

        #region ����ѡ�����򣨴�������ʾ��
        EditorGUILayout.LabelField(
            new GUIContent("ѡ�����", "ѡ��ҪӦ�õ�Material��Դ"),
            EditorStyles.boldLabel
        );
        selectedMaterial = (Material)EditorGUILayout.ObjectField(
            new GUIContent("����", "�����ѡ��Material��Դ"),
            selectedMaterial,
            typeof(Material),
            false,
            GUILayout.Width(400)
        );
        GUILayout.Space(5);
        #endregion

        #region ����ѡ�����򣨴�������ʾ��
        EditorGUILayout.LabelField(
            new GUIContent("ѡ������", "ѡ��ҪӦ�õ�Texture2D��Դ"),
            EditorStyles.boldLabel
        );
        selectedTexture = (Texture2D)EditorGUILayout.ObjectField(
            new GUIContent("����", "�����ѡ��Texture2D��Դ"),
            selectedTexture,
            typeof(Texture2D),
            false,
            GUILayout.Width(400)
        );
        GUILayout.Space(10);
        #endregion

        #region ����UI���򣨴�������ʾ��
        EditorGUILayout.LabelField(
            new GUIContent("����UI����", "����Ԥ���õ�UI Image����"),
            EditorStyles.boldLabel
        );

        if (GUILayout.Button(
            new GUIContent("����UI����", "���������UIԤ������"),
            GUILayout.Width(200)
        ))
        {
            CreateUIObject();
        }

        // Ԥ���������ӿ��жϺ͹�����ʾ
        if (previewObject != null && previewImage != null)
        {
            GUILayout.Label(
                new GUIContent("Ԥ������", "ʵʱ��ʾ��ǰ���ʡ������任Ч��"),
                EditorStyles.boldLabel
            );
            Rect previewRect = GUILayoutUtility.GetRect(100, 100);
            if (selectedTexture != null)
            {
                EditorGUI.DrawPreviewTexture(previewRect, selectedTexture);
            }

            GUILayout.Label(
                new GUIContent($"��ת�Ƕ�: {rotationAngle:F1}", "��ǰUI�������ת�Ƕ�"),
                EditorStyles.label
            );
            GUILayout.Label(
                new GUIContent($"���Ŵ�С: {scaleValue:F1}", "��ǰUI��������ű���"),
                EditorStyles.label
            );
        }
        GUILayout.Space(10);
        #endregion

        #region ��ת�����ŵ������򣨴�������ʾ��
        EditorGUILayout.LabelField(
            new GUIContent("��ת�����ŵ���", "ʵʱ�޸�UI����ı任����"),
            EditorStyles.boldLabel
        );

        rotationAngle = EditorGUILayout.Slider(
            new GUIContent("��ת�Ƕ�", "��Χ��0-360�ȣ�����UI�����Z����ת"),
            rotationAngle, 0f, 360f
        );

        scaleValue = EditorGUILayout.Slider(
            new GUIContent("���Ŵ�С", "��Χ��0.1-5.0��������UI��������ű���"),
            scaleValue, 0.1f, 5.0f
        );

        if (previewObject != null)
        {
            previewObject.transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
            previewObject.transform.localScale = new Vector3(scaleValue, scaleValue, 1);
        }
        GUILayout.Space(10);
        #endregion

        #region ���ܰ�ť���򣨴�������ʾ��
        EditorGUILayout.LabelField(
            new GUIContent("���ܲ���", "���á����桢���ص�ǰ����"),
            EditorStyles.boldLabel
        );

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(
            new GUIContent("����", "�ָ���������ΪĬ��ֵ"),
            GUILayout.Width(100)
        ))
        {
            ResetAll();
        }

        if (GUILayout.Button(
            new GUIContent("����", "����ǰ���ñ��浽JSON�ļ�"),
            GUILayout.Width(100)
        ))
        {
            SaveSettings();
        }

        if (GUILayout.Button(
            new GUIContent("����", "��JSON�ļ��ָ�֮ǰ���������"),
            GUILayout.Width(100)
        ))
        {
            LoadSettings();
        }

        EditorGUILayout.EndHorizontal();
        #endregion
    }

    /// <summary>
    /// ����UI����Ӧ�ò��ʺ�����
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
    /// ������������ΪĬ��ֵ
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
    /// ���浱ǰ���õ�JSON�ļ�
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
                    Debug.Log("�����ѱ��浽: " + filePath);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("��������ʱ����: " + e.Message);
        }
    }

    /// <summary>
    /// ��JSON�ļ���������
    /// </summary>
    private void LoadSettings()
    {
        try
        {
            string filePath = Application.dataPath + "/EditorSettings.json";
            if (!File.Exists(filePath))
            {
                Debug.LogWarning("�����ļ�������: " + filePath);
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

                CreateUIObject(); // ���غ��ؽ�UI
                Debug.Log("�����Ѵ��ļ�����: " + filePath);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("��������ʱ����: " + e.Message);
        }
    }
}