using System.Collections.Generic;
using UnityEngine;

public class ColorShift : MonoBehaviour
{
    public new SkinnedMeshRenderer renderer;
    public List<Material> materials = new();
    public List<Color> colors;
    public List<int> indexesToAffect;

    private void OnValidate()
    {
        renderer = GetComponent<SkinnedMeshRenderer>();
        renderer.GetMaterials(materials);
    }

    void UpdateColor()
    {
        for (int i = 0; i < colors.Count; i++)
        {
            int index = indexesToAffect[i];
            materials[index].color = colors[i];
        }
    }

    public void SetColor(Color a, Color b, Color c)
    {
        if (indexesToAffect.Count == 1)
            colors[0] = a;
        else if (indexesToAffect.Count == 2)
        {
            colors[0] = b;
            colors[1] = a;
        }
        else if (indexesToAffect.Count == 3)
        {
            colors[0] = a;
            colors[1] = b;
            colors[2] = c;
        }

        UpdateColor();
    }
}

