//----------------------------------------------
//			  NGUI: Next-Gen UI kit
// Copyright В© 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(Transform), true)]
public class NGUITransformInspector : Editor
{
	static public NGUITransformInspector instance;

	SerializedProperty mPos;
	SerializedProperty mRot;
	SerializedProperty mScale;

    public SerializedProperty MPos
    {
        get
        {
            if(mPos == null && target != null)
                mPos = serializedObject.FindProperty("m_LocalPosition");
            return mPos;
        }
    }

    public SerializedProperty MRot
    {
        get
        {
            if(mRot == null && target != null)
                mRot = serializedObject.FindProperty("m_LocalRotation");

            return mRot;
        }
    }

    public SerializedProperty MScale
    {
        get
        {
            if (mScale == null && target != null)
                mScale = serializedObject.FindProperty("m_LocalScale");

            return mScale;
        }
    }

    void OnEnable ()
	{
		instance = this;
	}

	void OnDestroy () { instance = null; }

	/// <summary>
	/// Draw the inspector widget.
	/// </summary>

	public override void OnInspectorGUI ()
	{
		EditorGUIUtility.labelWidth = 15f;

		serializedObject.Update();

		DrawPosition();
		DrawRotation(false);
		DrawScale(false);

		serializedObject.ApplyModifiedProperties();
	}

	void DrawPosition ()
	{
		GUILayout.BeginHorizontal();
		{
			bool reset = GUILayout.Button("P", GUILayout.Width(20f));

			EditorGUILayout.PropertyField(MPos.FindPropertyRelative("x"));
			EditorGUILayout.PropertyField(MPos.FindPropertyRelative("y"));
			EditorGUILayout.PropertyField(MPos.FindPropertyRelative("z"));

			if (reset) MPos.vector3Value = Vector3.zero;
		}
		GUILayout.EndHorizontal();
	}

	void DrawScale (bool isWidget)
	{
		GUILayout.BeginHorizontal();
		{
			bool reset = GUILayout.Button("S", GUILayout.Width(20f));

			if (isWidget) GUI.color = new Color(0.7f, 0.7f, 0.7f);
			EditorGUILayout.PropertyField(MScale.FindPropertyRelative("x"));
			EditorGUILayout.PropertyField(MScale.FindPropertyRelative("y"));
			EditorGUILayout.PropertyField(MScale.FindPropertyRelative("z"));
			if (isWidget) GUI.color = Color.white;

			if (reset) MScale.vector3Value = Vector3.one;
		}
		GUILayout.EndHorizontal();
	}

#region Rotation is ugly as hell... since there is no native support for quaternion property drawing
	enum Axes : int
	{
		None = 0,
		X = 1,
		Y = 2,
		Z = 4,
		All = 7,
	}

	Axes CheckDifference (Transform t, Vector3 original)
	{
		Vector3 next = t.localEulerAngles;

		Axes axes = Axes.None;

		if (Differs(next.x, original.x)) axes |= Axes.X;
		if (Differs(next.y, original.y)) axes |= Axes.Y;
		if (Differs(next.z, original.z)) axes |= Axes.Z;

		return axes;
	}

	Axes CheckDifference (SerializedProperty property)
	{
		Axes axes = Axes.None;

		if (property.hasMultipleDifferentValues)
		{
			Vector3 original = property.quaternionValue.eulerAngles;

			foreach (Object obj in serializedObject.targetObjects)
			{
				axes |= CheckDifference(obj as Transform, original);
				if (axes == Axes.All) break;
			}
		}
		return axes;
	}

	/// <summary>
	/// Draw an editable float field.
	/// </summary>
	/// <param name="hidden">Whether to replace the value with a dash</param>
	/// <param name="greyedOut">Whether the value should be greyed out or not</param>

	static bool FloatField (string name, ref float value, bool hidden, bool greyedOut, GUILayoutOption opt)
	{
		float newValue = value;
		GUI.changed = false;

		if (!hidden)
		{
			if (greyedOut)
			{
				GUI.color = new Color(0.7f, 0.7f, 0.7f);
				newValue = EditorGUILayout.FloatField(name, newValue, opt);
				GUI.color = Color.white;
			}
			else
			{
				newValue = EditorGUILayout.FloatField(name, newValue, opt);
			}
		}
		else if (greyedOut)
		{
			GUI.color = new Color(0.7f, 0.7f, 0.7f);
			float.TryParse(EditorGUILayout.TextField(name, "--", opt), out newValue);
			GUI.color = Color.white;
		}
		else
		{
			float.TryParse(EditorGUILayout.TextField(name, "--", opt), out newValue);
		}

		if (GUI.changed && Differs(newValue, value))
		{
			value = newValue;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Because Mathf.Approximately is too sensitive.
	/// </summary>

	static bool Differs (float a, float b) { return Mathf.Abs(a - b) > 0.0001f; }

	void DrawRotation (bool isWidget)
	{
		GUILayout.BeginHorizontal();
		{
			bool reset = GUILayout.Button("R", GUILayout.Width(20f));

			Vector3 visible = (serializedObject.targetObject as Transform).localEulerAngles;

			visible.x = WrapAngle(visible.x);
			visible.y = WrapAngle(visible.y);
			visible.z = WrapAngle(visible.z);

			Axes changed = CheckDifference(MRot);
			Axes altered = Axes.None;

			GUILayoutOption opt = GUILayout.MinWidth(30f);

			if (FloatField("X", ref visible.x, (changed & Axes.X) != 0, isWidget, opt)) altered |= Axes.X;
			if (FloatField("Y", ref visible.y, (changed & Axes.Y) != 0, isWidget, opt)) altered |= Axes.Y;
			if (FloatField("Z", ref visible.z, (changed & Axes.Z) != 0, false, opt)) altered |= Axes.Z;

			if (reset)
			{
				MRot.quaternionValue = Quaternion.identity;
			}
			else if (altered != Axes.None)
			{
				RegisterUndo("Change Rotation", serializedObject.targetObjects);

				foreach (Object obj in serializedObject.targetObjects)
				{
					Transform t = obj as Transform;
					Vector3 v = t.localEulerAngles;

					if ((altered & Axes.X) != 0) v.x = visible.x;
					if ((altered & Axes.Y) != 0) v.y = visible.y;
					if ((altered & Axes.Z) != 0) v.z = visible.z;

					t.localEulerAngles = v;
				}
			}
		}
		GUILayout.EndHorizontal();
	}

	static private float WrapAngle (float angle)
	{
		while (angle > 180f) angle -= 360f;
		while (angle < -180f) angle += 360f;
		return angle;
	}

	static private void RegisterUndo (string name, params Object[] objects)
	{
		if (objects != null && objects.Length > 0)
		{
			UnityEditor.Undo.RecordObjects(objects, name);

			foreach (Object obj in objects)
			{
				if (obj == null) continue;
				EditorUtility.SetDirty(obj);
			}
		}
	}
#endregion
}
