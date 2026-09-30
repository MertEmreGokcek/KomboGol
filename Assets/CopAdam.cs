using System.Collections;
using UnityEngine;

// MSN logosundaki gibi yuvarlak gövdeli, hafif şişko bir çöp adam.
public class CopAdam : MonoBehaviour
{
    Transform atanKol;
    Quaternion kolHazir;
    Coroutine atis;

    public void Kur(Color renk, bool atabilsin)
    {
        if (transform.Find("Govde") != null)
            return;

        Material malzeme = Malzeme(renk);

        Parca(PrimitiveType.Sphere, "Govde", new Vector3(0f, 0.7f, 0f), new Vector3(0.52f, 0.44f, 0.44f), malzeme, transform);
        Parca(PrimitiveType.Sphere, "Kafa", new Vector3(0f, 1.12f, 0.02f), new Vector3(0.36f, 0.36f, 0.36f), malzeme, transform);
        Transform kafa = transform.Find("Kafa");
        GozEkle(kafa, new Vector3(-0.16f, 0.16f, -0.46f));
        GozEkle(kafa, new Vector3(0.16f, 0.16f, -0.46f));
        Parca(PrimitiveType.Capsule, "BacakSol", new Vector3(-0.13f, 0.2f, 0f), new Vector3(0.15f, 0.16f, 0.15f), malzeme, transform);
        Parca(PrimitiveType.Capsule, "BacakSag", new Vector3(0.13f, 0.2f, 0f), new Vector3(0.15f, 0.16f, 0.15f), malzeme, transform);

        Transform sol = KolEkle("KolSol", new Vector3(-0.34f, 0.86f, 0f), malzeme);
        sol.localRotation = Quaternion.Euler(8f, 0f, 20f);

        Transform sag = KolEkle("KolSag", new Vector3(0.34f, 0.86f, 0f), malzeme);
        if (atabilsin)
        {
            atanKol = sag;
            kolHazir = Quaternion.Euler(-30f, 0f, -8f);
            atanKol.localRotation = kolHazir;
        }
        else
        {
            sag.localRotation = Quaternion.Euler(8f, 0f, -20f);
        }
    }

    // Atışla aynı anda biter. Bekletmez.
    public void HizliAt()
    {
        if (atanKol == null)
            return;

        if (atis != null)
            StopCoroutine(atis);

        atis = StartCoroutine(KoluSavur());
    }

    IEnumerator KoluSavur()
    {
        Quaternion bitis = Quaternion.Euler(78f, 0f, -8f);
        float sure = 0.08f;
        float gecen = 0f;

        while (gecen < sure)
        {
            gecen += Time.deltaTime;
            float oran = Mathf.Clamp01(gecen / sure);
            atanKol.localRotation = Quaternion.Slerp(kolHazir, bitis, oran);
            yield return null;
        }

        atanKol.localRotation = bitis;
    }

    Transform KolEkle(string ad, Vector3 yer, Material malzeme)
    {
        GameObject omuz = new GameObject(ad);
        omuz.transform.SetParent(transform, false);
        omuz.transform.localPosition = yer;
        Parca(PrimitiveType.Capsule, "Pazu", new Vector3(0f, -0.18f, 0f), new Vector3(0.11f, 0.15f, 0.11f), malzeme, omuz.transform);
        return omuz.transform;
    }

    static void Parca(PrimitiveType tip, string ad, Vector3 yer, Vector3 olcek, Material malzeme, Transform ebeveyn)
    {
        GameObject parca = GameObject.CreatePrimitive(tip);
        parca.name = ad;
        parca.transform.SetParent(ebeveyn, false);
        parca.transform.localPosition = yer;
        parca.transform.localScale = olcek;

        Collider carpma = parca.GetComponent<Collider>();
        if (carpma != null)
        {
            carpma.enabled = false;
            Destroy(carpma);
        }

        Renderer gorunum = parca.GetComponent<Renderer>();
        gorunum.sharedMaterial = malzeme;
        gorunum.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        gorunum.receiveShadows = true;
    }

    static void GozEkle(Transform kafa, Vector3 yer)
    {
        Material beyaz = Malzeme(Color.white);
        Material gozbebek = Malzeme(new Color(0.08f, 0.08f, 0.1f));

        GameObject goz = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        goz.name = "Goz";
        goz.transform.SetParent(kafa, false);
        goz.transform.localPosition = yer;
        goz.transform.localScale = Vector3.one * 0.24f;
        Collider carpma = goz.GetComponent<Collider>();
        if (carpma != null)
        {
            carpma.enabled = false;
            Destroy(carpma);
        }
        Renderer gorunum = goz.GetComponent<Renderer>();
        gorunum.sharedMaterial = beyaz;
        gorunum.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        GameObject bebek = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bebek.name = "Gozbebek";
        bebek.transform.SetParent(goz.transform, false);
        bebek.transform.localPosition = new Vector3(0f, 0f, -0.42f);
        bebek.transform.localScale = Vector3.one * 0.48f;
        Collider bebekCarpma = bebek.GetComponent<Collider>();
        if (bebekCarpma != null)
        {
            bebekCarpma.enabled = false;
            Destroy(bebekCarpma);
        }
        bebek.GetComponent<Renderer>().sharedMaterial = gozbebek;
        bebek.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    static Material Malzeme(Color renk)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        Material malzeme = shader != null ? new Material(shader) : new Material(Shader.Find("Standard"));
        malzeme.color = renk;
        if (malzeme.HasProperty("_BaseColor"))
            malzeme.SetColor("_BaseColor", renk);
        if (malzeme.HasProperty("_Smoothness"))
            malzeme.SetFloat("_Smoothness", 0.35f);
        return malzeme;
    }
}
