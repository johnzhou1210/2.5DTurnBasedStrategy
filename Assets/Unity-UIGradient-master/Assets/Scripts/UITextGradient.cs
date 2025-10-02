using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Text Gradient")]
public class UITextGradient : BaseMeshEffect
{
	[FormerlySerializedAs("m_color1")] public Color mColor1 = Color.white;
	[FormerlySerializedAs("m_color2")] public Color mColor2 = Color.white;
	[FormerlySerializedAs("m_angle")] [Range(-180f, 180f)]
	public float mAngle = 0f;

    public override void ModifyMesh(VertexHelper vh)
    {
		if(enabled)
		{
			Rect rect = graphic.rectTransform.rect;
			Vector2 dir = UIGradientUtils.RotationDir(mAngle);
			UIGradientUtils.Matrix2X3 localPositionMatrix = UIGradientUtils.LocalPositionMatrix(new Rect(0f, 0f, 1f, 1f), dir);

			UIVertex vertex = default(UIVertex);
			for (int i = 0; i < vh.currentVertCount; i++) {

				vh.PopulateUIVertex (ref vertex, i);
				Vector2 position = UIGradientUtils.VerticePositions[i % 4];
				Vector2 localPosition = localPositionMatrix * position;
				vertex.color *= Color.Lerp(mColor2, mColor1, localPosition.y);
				vh.SetUIVertex (vertex, i);
			}
		}
    }
}
