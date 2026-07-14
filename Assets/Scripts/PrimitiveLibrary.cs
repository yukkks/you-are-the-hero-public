using UnityEngine;

// Editor-only APIs like GetBuiltinResource and CreatePrimitive are unreliable
// in WebGL players (stripping / missing built-ins). This scene object holds
// editor-assigned references to the primitive meshes and a known-included
// particle material, so runtime spawners (beacons, tap ripples, confetti)
// always have real assets to use.
public class PrimitiveLibrary : MonoBehaviour
{
    public static PrimitiveLibrary Instance { get; private set; }

    public Mesh sphere;
    public Mesh cylinder;
    public Material particleMaterial;

    void Awake() { Instance = this; }

    public static Mesh Sphere => Instance != null ? Instance.sphere : null;
    public static Mesh Cylinder => Instance != null ? Instance.cylinder : null;
    public static Material Particle => Instance != null ? Instance.particleMaterial : null;
}
