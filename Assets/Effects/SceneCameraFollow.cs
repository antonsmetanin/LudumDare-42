using UnityEngine;




public class SceneCameraFollow : MonoBehaviour
{
    void Update()
    {
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!enabled) return;
        if (GetComponent<Camera>() == null) return;
        if (UnityEditor.SceneView.currentDrawingSceneView == null || UnityEditor.SceneView.currentDrawingSceneView.camera == null) return;

        Follow(GetComponent<Camera>(), UnityEditor.SceneView.currentDrawingSceneView.camera);
    }
#endif

    static void Follow(Camera follower, Camera target)
    {
        Transform targetTransform = target.transform;
        Transform followerTransform = follower.transform;
        followerTransform.position = targetTransform.position;
        followerTransform.rotation = targetTransform.rotation;
        //follower.fov = target.fov;
    }

}