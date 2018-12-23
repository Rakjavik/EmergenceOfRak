using UnityEngine.AI;

public class RAKMeshBaker {

    public static NavMeshSurface[] surfaces;

    public static void Bake(NavMeshSurface[] surfaces)
    {
        RAKMeshBaker.surfaces = surfaces;
        for (int i = 0; i < surfaces.Length; i++)
        {
            surfaces[i].BuildNavMesh();
        }
    }
}
