using NaughtyAttributes;
using System.Xml.Linq;
using UnityEngine;

public class TestValues : MonoBehaviour
{
    [SerializeField] private Vector3 i;
    [SerializeField] private Vector3 j;
    [SerializeField] private Vector4 a;
    [SerializeField] private Vector4 b;
    [SerializeField] private float scale = 10;

    [Button]
    public void Print()
    {
        float x0 = SimplexNoise3D.SNoise(i);
        Debug.Log("Final Result :" + x0.ToString());
        // 12, -6, 21
    }
}
