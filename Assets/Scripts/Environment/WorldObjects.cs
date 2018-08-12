using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldObjects : MonoBehaviour
{
    private static WorldObjects _instance;
    public static WorldObjects Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<WorldObjects>();
                if (_instance == null)
                {
                    var go = new GameObject("___worldObjects");
                    _instance = go.AddComponent<WorldObjects>();
                }
            }
            return _instance;
        }
    }

    public List<Tree> Trees = new List<Tree>();

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        if (_instance != this)
            Destroy(this);
    }

    private void Start()
    {
        Trees = FindObjectsOfType<Tree>().ToList();
    }

    public GameObject GetRandomObject()
    {
        if (Trees.Count == 0)
            return null;

        var treeIndex = Random.Range(0, Trees.Count);
        var maxCycles = Trees.Count;
        while ((Trees[treeIndex] == null || !Trees[treeIndex].IsAlive) && maxCycles-- >= 0)
        {
            treeIndex = (++treeIndex) % Trees.Count;
        }

        return maxCycles >= 0 ? Trees[treeIndex].gameObject : null;
    }

    public GameObject GetOneOfClosest(Vector3 position, int betterCount)
    {
        if (Trees.Count == 0)
            return null;

        var ordered = Trees.Where(_ => _ != null && _.IsAlive).OrderByDescending(_ => Vector3.Distance(position, _.transform.position)).ToArray();
        if (ordered.Length == 0)
            return null;

        var treeIndex = betterCount >= ordered.Length - 1 ? ordered.Length : betterCount + 1;
        treeIndex = Random.Range(0, treeIndex);
        return ordered[treeIndex].gameObject;
    }

    public GameObject GetClosestObject(Vector3 position)
    {
        if (Trees.Count == 0)
            return null;

        var minDistance = float.MaxValue;
        Tree closestObject = null;
        for (int i = 0; i < Trees.Count; i++)
        {
            if (Trees[i] == null || !Trees[i].IsAlive)
                continue;
            var currentDistance = Vector3.Distance(position, Trees[i].transform.position);
            if (currentDistance >= minDistance)
                continue;

            minDistance = currentDistance;
            closestObject = Trees[i];
        }

        return closestObject == null ? null : closestObject.gameObject;
    }
}
