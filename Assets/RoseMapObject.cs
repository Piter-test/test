using UnityEngine;
using System.Collections;
//using Revise.Files;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RoseMapObject : MonoBehaviour
{
	public RoseMapObjectData data;
	private RoseMapObjectData oldData;

	private static void DestroyChildren(GameObject go, GameObject butIgnore = null)
	{
		while (go.transform.childCount > 0)
		{
			DestroyChildren(go.transform.GetChild(0).gameObject, butIgnore);
		}
		if (go != butIgnore)
		{
			DestroyImmediate(go);
		}
	}

    public void UpdateModels()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        if (!data) return;

        var subObjs = data.subObjects;
        var lkp = new GameObject[subObjs.Count];

        for (int i = 0; i < subObjs.Count; ++i)
        {
            var subObj = subObjs[i];
            var go = new GameObject("Mesh " + (i + 1));

            go.AddComponent<MeshFilter>().sharedMesh = subObj.mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = subObj.material;

            if (subObj.parent == 0)
            {
                go.transform.SetParent(transform);//, false);
            }
            else
            {
                go.transform.SetParent(lkp[subObj.parent - 1].transform);//, false);
            }

            go.transform.localPosition = subObj.position;
            go.transform.localRotation = subObj.rotation;
            go.transform.localScale = subObj.scale;

            go.name = "Mesh " + (i + 1).ToString();

            if (subObj.colMode == 1)
            {
                //go.AddComponent<MeshCollider>();
            }

            if (subObj.animation != null)
            {
                var an = go.AddComponent<Animation>();
                an.clip = subObj.animation;
            }
            lkp[i] = go;
        }

        // 3. DRUGA PĘTLA (ODDZIELNA): TWORZYMY EFEKTY
        // Ta pętla musi być POZA pętlą 'for (int i...)'
        if (data.effects != null)
        {
            for (int h = 0; h < data.effects.Count; h++)
            {
                
                var eff = data.effects[h];
                //GameObject ptlObj = new GameObject("Ptl_Effect_" + h);

                // Tworzymy NOWY GameObject dla efektu
                GameObject effObj = new GameObject("Effect_Part_" + h);

                // Ustawienie rodzica i pozycji
                if (eff.parent > 0 && eff.parent <= lkp.Length)
                    effObj.transform.SetParent(lkp[eff.parent - 1].transform, false);
                else
                    effObj.transform.SetParent(transform, false);

                effObj.transform.localPosition = eff.position;
                effObj.transform.localRotation = eff.rotation;

                // DODAJEMY PARTICLE SYSTEM I KONFIGURUJEMY GO NA MIEJSCU
                var ps = effObj.AddComponent<ParticleSystem>();
                var main = ps.main;
                var emission = ps.emission;
                var renderer = effObj.GetComponent<ParticleSystemRenderer>();
                var shape = ps.shape;
                var force = ps.forceOverLifetime;
                var tsa = ps.textureSheetAnimation;

                tsa.enabled = true;

                // Rzutowanie float na int, aby naprawić błąd CS0266
                tsa.numTilesX = (int)eff.TextureWidth;
                tsa.numTilesY = (int)eff.TextureHeight;

                // Reszta ustawień animacji
                tsa.mode = ParticleSystemAnimationMode.Grid;
                tsa.timeMode = ParticleSystemAnimationTimeMode.Lifetime;
                tsa.frameOverTime = new ParticleSystem.MinMaxCurve(0f, 1f);

                // 3. TRYB ANIMACJI


                // 4. CYKL ŻYCIA KLATKI
                // Tworzymy krzywą od klatki 0 do ostatniej klatki (numTilesX * numTilesY)
                tsa.frameOverTime = new ParticleSystem.MinMaxCurve(0f, 1f);

                // Jeśli PTL ma zapętlone klatki:
                tsa.cycleCount = 1;
                renderer.material = eff.material;
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                // --- RESZTA USTAWIEŃ RENDERERA ---
                //var renderer = ptlnat.GetComponent<ParticleSystemRenderer>();
                // Bardzo ważne: Texture Sheet Animation działa najlepiej z Billboard lub VerticalBillboard
                //renderer.renderMode = ParticleSystemRenderMode.Billboard;
                // PRZEPISANIE DANYCH Z EFFDATA DO KOMPONENTU
                //main.startLifetime = new ParticleSystem.MinMaxCurve(eff.lifetimeMin, eff.lifetimeMax);
                main.startLifetime = new ParticleSystem.MinMaxCurve(eff.lifetimeMin, eff.lifetimeMax);
                main.maxParticles = eff.particleCount;
                //emission.rateOverTime = new ParticleSystem.MinMaxCurve(eff.emitRateMin, eff.emitRateMax);
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(eff.emitRateMin, eff.emitRateMax);
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.scale = eff.emitRadiusMax; // Uproszczone mapowanie promienia

                // 4. Grawitacja (Force)
                force.enabled = true;
                force.x = new ParticleSystem.MinMaxCurve(eff.gravityMin.x, eff.gravityMax.x);
                force.y = new ParticleSystem.MinMaxCurve(eff.gravityMin.y, eff.gravityMax.y);
                force.z = new ParticleSystem.MinMaxCurve(eff.gravityMin.z, eff.gravityMax.z);
                main.startSpeed = new ParticleSystem.MinMaxCurve(eff.spawnDirMin.magnitude, eff.spawnDirMax.magnitude);
                // Renderowanie i materiał
                renderer.material = eff.material;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.loop = true;

                // Start efektu
                ps.Play();
                /*if (eff.parent > 0 && eff.parent <= lkp.Length)
                {
                    effObj.transform.SetParent(lkp[eff.parent - 1].transform, false);
                }
                else
                {
                effObj.transform.SetParent(transform, false);
                }
                effObj.AddComponent<MeshRenderer>().sharedMaterial = eff.material;
                effObj.transform.localPosition = eff.position;
                effObj.transform.localRotation = eff.rotation;
                effObj.transform.localScale = eff.scale;

                var ps = effObj.AddComponent<ParticleSystem>();
                var renderer = effObj.GetComponent<ParticleSystemRenderer>();

                if (eff.material != null)
                {
                    renderer.material = eff.material;
                }
                else
                {
                    renderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
                }
                var main = ps.main;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.loop = true;
                main.playOnAwake = true;

                ps.Play();*/
            }        
        }
    }
}
		
	


