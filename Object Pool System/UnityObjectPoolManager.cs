using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

public class UnityObjectPoolManager : MonoBehaviour
{   //Unity Object Pool Classi.
    ObjectPool<EnemyBlock> _pool;
    [SerializeField] int _poolCount = 0;
    [SerializeField] int _poolMax = 100;
    [SerializeField] EnemyBlock _pooledGoTr;

    [Range(0.0005f, 10f)]
    [SerializeField] float _generationTimeInterval = 0.5f;
    float _generatorTimeMeter = 0;

    [HideInInspector] public int NumberOfPooled;
    [HideInInspector] public int NumberOfActive;
    [HideInInspector] public int NumberOfInactive;

    private Action _onStack;

    private int _indexer;

    void Start() {
        _pool = new ObjectPool<EnemyBlock>(
            CreateElement, 
            GetFromPool, 
            Return2Pool, 
            DestroyWhenOverCapacity, 
            false, 
            _poolCount, 
            _poolMax);

        GenerateByCount();
        //GenerateByAid();
        SensorDetails();
    }

    public EnemyBlock CreateElement() {
        //Prefab orneklerken cagrilan callback.
        EnemyBlock  scr = Instantiate(_pooledGoTr);
        scr.name = _indexer.ToString();
        _indexer++;
        NumberOfPooled++;
        scr.AssignPool(_pool);
        return scr;
    }

    public void Return2Pool(EnemyBlock scr) {
        //Aktiften pasife cekerken cagrilan callback.
        scr.gameObject.SetActive(false);
    }

    public void GetFromPool(EnemyBlock scr) {
        //Havuzdan ornek isterken cagrilan callback.
        scr.gameObject.SetActive(true);
        scr.transform.position = transform.position;
        scr.SetSpanTime(3f);
    }

    public void DestroyWhenOverCapacity(EnemyBlock scr) {
        //Limit fazlasinda yok edilen prefab icin cagrilan callback.
        Debug.LogAssertion("Obje otomatik olarak yok edildi!");
        scr.gameObject.SetActive(true);
        Destroy(scr.gameObject);
        NumberOfPooled--;
    }

    void Update() {
        GenarateByTime();
        SensorDetails();
    }   

    private void GenarateByTime() {
        //Belirli zaman araliklarinda GO olusturulur.
        _generatorTimeMeter += Time.deltaTime;
        if (_generatorTimeMeter > _generationTimeInterval) {
            _pool.Get();
            _generatorTimeMeter = 0;
        }
    }
    private void GenerateByCount() {
        //Stack Alma 1. Yontem Tum GO lar olusturuldugunda hepsi bir anda bir callback ile queuee ye islenir.
        for(int i = 0; i < _poolCount; i++) {
            EnemyBlock eb = _pool.Get();
            _onStack += eb.PushBack2Pool;
        }
        _onStack.Invoke();
        _onStack = null;
        Debug.LogWarning("OnStack Tamamlandi!");
    }

    private void GenerateByAid() {
        //Stack Alma 2. Yontem : Belirli sayida GO uretilir. Start esnasinda bir kere cagirarak sensorlerden gelen verileri daha duzgun sekilde okunur.
        List<EnemyBlock> li = new List<EnemyBlock>();
        for (int i = 0; i < _poolCount; i++) {
            EnemyBlock eb = _pool.Get();
            li.Add(eb);
        }

        for (int i = li.Count - 1; i >= 0; i--) {
            li[i].PushBack2Pool();
            li.RemoveAt(i);
        }
        Debug.Log("Liste uzunlugu : " +  li.Count);
    }

    private void SensorDetails() {
        //Olusturulan GO lar hakkinda detaylar. Dahili sistem verilerinde hata meydana geldiginde ek olarak olusturuldu.
        NumberOfInactive = _pool.CountInactive;
        NumberOfActive = NumberOfPooled - NumberOfInactive;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UnityObjectPoolManager), true)]
public class UnityObjectPoolManagerEditor : Editor {
    //OnInspectorGUI ile Inspectorda alanlari gorunmez yapmak icin ayni class uzerinde Editor classi kullanildi.
    private UnityObjectPoolManager _uopm;
    private SerializedProperty _numberOfPooled;
    private SerializedProperty _numberOfActive;
    private SerializedProperty _numberOfInactive;
    private GUIStyle _title0;

    public void OnEnable() {
        _uopm = (UnityObjectPoolManager)target;
        _numberOfPooled = serializedObject.FindProperty(nameof(_uopm.NumberOfPooled));
        _numberOfActive = serializedObject.FindProperty(nameof(_uopm.NumberOfActive));
        _numberOfInactive = serializedObject.FindProperty(nameof(_uopm.NumberOfInactive));

        _title0 = new();
        _title0.normal.textColor = Color.green;
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        base.OnInspectorGUI();
        EditorGUILayout.LabelField("Pool Details", _title0);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(_numberOfPooled);
        EditorGUILayout.PropertyField(_numberOfActive);
        EditorGUILayout.PropertyField(_numberOfInactive);
        EditorGUI.EndDisabledGroup();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif