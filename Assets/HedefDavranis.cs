using System.Collections;
using UnityEngine;

// Hedef, farklı renkte şişko bir çöp adamdır. Vurulunca yatay devrilir.
public class HedefDavranis : MonoBehaviour
{
    bool devrildi;
    bool sinirli;

    void Awake()
    {
        CopAdamaCevir();
    }

    public void YatayDus(Vector3 yon)
    {
        if (devrildi)
            return;

        devrildi = true;

        yon.y = 0f;
        if (yon.sqrMagnitude < 0.0001f)
            yon = Vector3.forward;
        yon.Normalize();

        Rigidbody govde = GetComponent<Rigidbody>();
        if (govde == null)
            govde = gameObject.AddComponent<Rigidbody>();

        govde.useGravity = true;
        govde.isKinematic = false;
        govde.constraints = RigidbodyConstraints.None;
        govde.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        govde.linearVelocity = Vector3.zero;
        govde.angularVelocity = Vector3.Cross(Vector3.up, -yon) * 8f;
    }

    void CopAdamaCevir()
    {
        Renderer govde = GetComponent<Renderer>();
        if (govde != null)
            govde.enabled = false;

        Vector3 yer = transform.position;
        yer.y = 5f;
        transform.position = yer;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        if (!Gorsel.Karakter(transform, "02"))
        {
            CopAdam adam = GetComponent<CopAdam>();
            if (adam == null)
                adam = gameObject.AddComponent<CopAdam>();
            adam.Kur(new Color(1f, 0.78f, 0.1f), false);
        }

        BoxCollider kutu = GetComponent<BoxCollider>();
        if (kutu != null)
            Destroy(kutu);

        CapsuleCollider kapsul = GetComponent<CapsuleCollider>();
        if (kapsul == null)
            kapsul = gameObject.AddComponent<CapsuleCollider>();

        kapsul.direction = 1;
        kapsul.height = 1.35f;
        kapsul.radius = 0.32f;
        kapsul.center = new Vector3(0f, 0.68f, 0f);
    }

    public void Sinirlen()
    {
        if (devrildi || sinirli)
            return;

        StartCoroutine(Sinir());
    }

    IEnumerator Sinir()
    {
        sinirli = true;
        Ses.Bagir();
        GameObject yazi = BagirYazisi();
        Vector3 yer = transform.position;
        float sure = 0.75f;
        float gecen = 0f;
        while (gecen < sure && !devrildi)
        {
            gecen += Time.deltaTime;
            float sars = Mathf.Sin(gecen * 42f) * 0.07f;
            transform.position = yer + new Vector3(sars, Mathf.Abs(Mathf.Sin(gecen * 16f)) * 0.1f, 0f);
            if (yazi != null && Camera.main != null)
                yazi.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
            yield return null;
        }

        if (!devrildi)
            transform.position = yer;
        if (yazi != null)
            Destroy(yazi);
        sinirli = false;
    }

    GameObject BagirYazisi()
    {
        GameObject obje = new GameObject("Bagiris");
        obje.transform.SetParent(transform, true);
        obje.transform.position = transform.position + new Vector3(0f, 1.7f, 0f);
        TextMesh yazi = obje.AddComponent<TextMesh>();
        yazi.text = "HEY!";
        yazi.anchor = TextAnchor.MiddleCenter;
        yazi.alignment = TextAlignment.Center;
        yazi.color = new Color(0.85f, 0.12f, 0.08f);
        yazi.fontSize = 64;
        yazi.characterSize = 0.16f;
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null)
            yazi.font = font;
        return obje;
    }
}
