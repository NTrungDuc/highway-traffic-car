using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static HPS_TrafficPooling;

[ExecuteInEditMode]
public class HPS_Instancer : MonoBehaviour
{
    public HPS_TrafficCars trafficCars;

    private List<List<Matrix4x4>> batches = new();
    private List<Mesh> meshes = new();
    private List<Material> materials = new();

    void OnValidate()
    {
        batches.Clear();
        meshes.Clear();
        materials.Clear();
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < trafficCars.fragments.Length; i++)
        {
            List<Matrix4x4> Fragments = new();
            Fragments.Add(Matrix4x4.TRS(transform.position + trafficCars.fragments[i].Position, Quaternion.Euler(trafficCars.fragments[i].Rotation.x, trafficCars.fragments[i].Rotation.y, trafficCars.fragments[i].Rotation.z), trafficCars.fragments[i].Scale));
            batches.Add(Fragments);
            meshes.Add(trafficCars.fragments[i].Mesh);
            materials.Add(trafficCars.fragments[i].Material);
        }

        List<Matrix4x4> Wheels = new();
        for (int i = 0; i < trafficCars.wheels.Positions.Length; i++)
            Wheels.Add(Matrix4x4.TRS(transform.position + trafficCars.wheels.Positions[i], Quaternion.Euler(trafficCars.wheels.Rotations[i]), trafficCars.wheels.Scales[i]));

        batches.Add(Wheels);
        meshes.Add(trafficCars.wheels.Mesh);
        materials.Add(trafficCars.wheels.Material);

    }

    // Update is called once per frame
    void Update()
    {
        RenderBatches();
    }

    private void RenderBatches()
    {
        if (batches.Count <= 0)
        {
            Start();
            return;
        }

        for (int i = 0; i < batches.Count; i++)
        {
            if (i < batches.Count - 1)
            {
                for (int j = 0; j < trafficCars.fragments.Length; j++)
                {
                    if (transform.position + trafficCars.fragments[j].Position != (Vector3)batches[j][0].GetColumn(3))
                        batches[j][0] = Matrix4x4.TRS(transform.position + trafficCars.fragments[i].Position, Quaternion.Euler(trafficCars.fragments[i].Rotation.x, trafficCars.fragments[i].Rotation.y, trafficCars.fragments[i].Rotation.z), trafficCars.fragments[i].Scale);
                }
            }
            else
            {
                for (int j = 0; j < trafficCars.wheels.Positions.Length; j++)
                {
                    if (transform.position + trafficCars.wheels.Positions[j] != (Vector3)batches[i][j].GetColumn(3))
                        batches[i][j] = Matrix4x4.TRS(transform.position + trafficCars.wheels.Positions[j], Quaternion.Euler(trafficCars.wheels.Rotations[j].x, trafficCars.wheels.Rotations[j].y, trafficCars.wheels.Rotations[j].z), trafficCars.wheels.Scales[j]);
                }
            }

            Graphics.DrawMeshInstanced(meshes[i], 0, materials[i], batches[i]);
        }
    }
}
