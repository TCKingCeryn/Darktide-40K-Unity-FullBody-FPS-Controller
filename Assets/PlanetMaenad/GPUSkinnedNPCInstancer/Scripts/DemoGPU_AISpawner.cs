using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using PlanetMaenad.FPS;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DemoGPU_AISpawner : MonoBehaviour
{
    public Transform SpawnPoint;
    [Space(5)]
    public bool SpawnOnStart;
    public bool SpawnOnEnable;
    [Space(10)]


    public Transform SpawnForceTarget;
    public float SpawnForceDetection = 500f;
    public int SpawnAmount;
    [Space(10)]


    public bool ConstantSpawn;
    public int ConstantMaxAmount = 100;
    public int SpawnBurstAmount = 3;
    public float SpawnRateInSeconds = 1;
    public float SpawnRadius = 15f;
    [Space(10)]



    public GameObject[] SpawnObjectPrefabs;
    [Space(10)]

    public List<GameObject> SpawnedObjects;



    internal WaitForSeconds StartSpawnDelay = new WaitForSeconds(3f);
    internal WaitForSeconds ConstantSpawnRateDelay;



    void OnEnable()
    {
        ConstantSpawnRateDelay = new WaitForSeconds(SpawnRateInSeconds);

        if (SpawnOnEnable && !SpawnOnStart)
        {
            StartCoroutine(StartSpawnObjectsDelay());
        }

        if (ConstantSpawn)
        {
            StartCoroutine(ConstantSpawnDelay());
        }
    }
    void Start()
    {
        ConstantSpawnRateDelay = new WaitForSeconds(SpawnRateInSeconds);

        if (!SpawnOnEnable && SpawnOnStart)
        {
            StartCoroutine(StartSpawnObjectsDelay());
        }

        if (ConstantSpawn)
        {
            StartCoroutine(ConstantSpawnDelay());
        }
    }


    IEnumerator StartSpawnObjectsDelay()
    {
        yield return StartSpawnDelay;

        SpawnObjects(SpawnPoint.transform.position, SpawnRadius, SpawnAmount);
    }
    IEnumerator ConstantSpawnDelay()
    {
        while (ConstantSpawn)
        {
            yield return ConstantSpawnRateDelay;

            CheckEmptySpawnedList();

            if (SpawnedObjects.Count < ConstantMaxAmount)
            {
                SpawnObjects(SpawnPoint.transform.position, SpawnRadius, SpawnBurstAmount);
            }
        }
    }

    public void CheckEmptySpawnedList()
    {
        if (SpawnedObjects.Count > 0)
        {
            for (int i = 0; i < SpawnedObjects.Count; i++)
            {
                if (SpawnedObjects[i] == null)
                {
                    SpawnedObjects.Remove(SpawnedObjects[i]);
                }
            }
        }
    }




    public void SpawnObjects()
    {
        //Maxed out Spawns
        if (SpawnedObjects.Count >= SpawnAmount)
        {
            return;
        }

        //Choose Objects To Spawn 
        foreach (var _ in Enumerable.Range(0, SpawnAmount))
        {
            if (SpawnedObjects.Count < SpawnAmount)
            {
                var RandomSpawnObject = Random.Range(0, SpawnObjectPrefabs.Length);
                var targetObject = SpawnObjectPrefabs[RandomSpawnObject];

                NavMeshHit navMeshHit;
                var randomNavHit = NavMesh.SamplePosition(Vector3.zero + Random.insideUnitSphere * SpawnRadius, out navMeshHit, Mathf.Infinity, NavMesh.AllAreas);
                var randomYRot = new Vector3(0, Random.Range(0f, 360f), 0);

                if (randomNavHit)
                {
                    var go = Instantiate(targetObject, navMeshHit.position, Quaternion.Euler(randomYRot));
                    go.SetActive(true);

                    if (!SpawnedObjects.Contains(go))
                    {
                        SpawnedObjects.Add(go);   
                    }

                    var AIControl = go.GetComponent<DemoGPU_AIEnemy>();
                    if (SpawnForceTarget != null && AIControl) AIControl.CurrentTarget = SpawnForceTarget; AIControl.DetectionRadius = SpawnForceDetection;
                }
            }
        }


    }
    public void SpawnObjects(Vector3 SpawnCenter, float SpawnRange, int SpawnCount)
    {
        //Maxed out Spawns
        if (SpawnedObjects.Count >= SpawnAmount)
        {
            return;
        }

        //Choose Objects To Spawn 
        foreach (var _ in Enumerable.Range(0, SpawnCount))
        {
            if (SpawnedObjects.Count < SpawnAmount)
            {
                var RandomSpawnObject = Random.Range(0, SpawnObjectPrefabs.Length);
                var targetObject = SpawnObjectPrefabs[RandomSpawnObject];

                NavMeshHit navMeshHit;
                var randomNavHit = NavMesh.SamplePosition(SpawnCenter + Random.insideUnitSphere * SpawnRange, out navMeshHit, Mathf.Infinity, NavMesh.AllAreas);
                var randomYRot = new Vector3(0, Random.Range(0f, 360f), 0);

                if (randomNavHit && targetObject)
                {
                    var go = Instantiate(targetObject, navMeshHit.position, Quaternion.Euler(randomYRot));
                    go.SetActive(true);

                    if (!SpawnedObjects.Contains(go))
                    {
                        SpawnedObjects.Add(go);
                    }

                    var AIControl = go.GetComponent<DemoGPU_AIEnemy>();
                    if (SpawnForceTarget != null && AIControl) AIControl.CurrentTarget = SpawnForceTarget; AIControl.DetectionRadius = SpawnForceDetection;
                }
            }
        }


    }

    public void ChangeSpawnAmount(string Count)
    {
        SpawnAmount = int.Parse(Count);
    }
    public void DestroyObjects()
    {
        for (int i = 0; i < SpawnedObjects.Count; i++)
        {
            if (SpawnedObjects[i] != null)
            {
                Destroy(SpawnedObjects[i].gameObject);
            }
        }

        SpawnedObjects.Clear();
    }



}


#if UNITY_EDITOR
[CustomEditor(typeof(DemoGPU_AISpawner))]
public class CustomInspectorDemoGPUSpawner : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DemoGPU_AISpawner DemoGPU_AISpawner = (DemoGPU_AISpawner)target;


        EditorGUILayout.LabelField("_____________________________________________________________________________");

        GUILayout.Space(10);
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Spawn Objects"))
        {
            DemoGPU_AISpawner.SpawnObjects();
        }
        GUILayout.Space(5);
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Destroy Spawned Objects"))
        {
            DemoGPU_AISpawner.DestroyObjects();
        }


    }

}
#endif