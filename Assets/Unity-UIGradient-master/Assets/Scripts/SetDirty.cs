using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SetDirty : MonoBehaviour {
	[FormerlySerializedAs("m_graphic")] public Graphic mGraphic;
	// Use this for initialization
	void Reset () {
		mGraphic = GetComponent<Graphic>();
	}
	
	// Update is called once per frame
	void Update () {
		mGraphic.SetVerticesDirty();
	}
}
