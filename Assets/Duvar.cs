using UnityEngine;

public enum DuvarTuru
{
    Normal = 0,
    Sayili = 1,
    Kirmizi = 2,
    Hayalet = 3,
    Sari = 4
}

// Duvarın türünü Inspector'dan seç.
// Normal: kahverengi, sadece seker.
// Sayili: yeşil, üstündeki sayı her sekmede iner, 0 olunca duvar kırılır.
// Kirmizi: kapağa değerse oyuncu elenir.
// Hayalet: mavi, başta saydam ve 10. Her duvar çarpışında sayı iner, 0 olunca katı ve mat olur.
public class Duvar : MonoBehaviour
{
    public DuvarTuru tur = DuvarTuru.Normal;
    public int baslangicCan = 10;
    public bool yandakiDuvar;

    int kalanCan;
    float sonSekmeZamani;
    TextMesh yazi;
    bool kirildi;
    bool katilasti;
    Material yuzeyMalzemesi;
    Color maviRenk = new Color(0.2f, 0.45f, 0.95f, 1f);

    void Start()
    {
        SekenYuzeyYap();

        if (tur == DuvarTuru.Hayalet)
        {
            kalanCan = Mathf.Max(1, baslangicCan);
            HayaletBaslat();
            YaziKur();
            YaziyiGuncelle();
            return;
        }

        RengiUygula();

        if (tur != DuvarTuru.Sayili)
            return;

        kalanCan = Mathf.Max(1, baslangicCan);
        YaziKur();
        YaziyiGuncelle();
    }

    void OnCollisionEnter(Collision temas)
    {
        if (kirildi)
            return;

        if (temas.gameObject.GetComponent<KapakFirlatma>() == null)
            return;

        if (tur == DuvarTuru.Kirmizi)
        {
            SekmeHissi(temas);
            MaviSayaciDusur();
            temas.gameObject.GetComponent<KapakFirlatma>().Elendi();
            return;
        }

        // Aynı sekme fizikte birden fazla kez sayılmasın
        if (Time.time - sonSekmeZamani < 0.2f)
            return;

        sonSekmeZamani = Time.time;
        SekmeHissi(temas);

        // Kahverengi, yeşil, kırmızı veya katılaşmış mavi: hepsi mavi sayacı düşürür
        MaviSayaciDusur();

        // Dış sınırdaki kahverengi duvarlar yeşil sayacı da düşürür
        if (tur == DuvarTuru.Normal)
        {
            Duvar[] duvarlar = FindObjectsByType<Duvar>(FindObjectsSortMode.None);
            foreach (Duvar duvar in duvarlar)
            {
                if (duvar.tur == DuvarTuru.Sayili)
                    duvar.SayiyiAzalt();
            }
            return;
        }

        if (tur == DuvarTuru.Sayili)
            SayiyiAzalt();
    }

    void MaviSayaciDusur()
    {
        Duvar[] duvarlar = FindObjectsByType<Duvar>(FindObjectsSortMode.None);
        foreach (Duvar duvar in duvarlar)
        {
            if (duvar.tur == DuvarTuru.Hayalet)
                duvar.SayiyiAzalt();
        }
    }

    void SekmeHissi(Collision temas)
    {
        Rigidbody govde = temas.rigidbody;
        if (govde != null)
            govde.linearVelocity *= 0.9f;

        EkranSallama.HafifSalla();
        Ses.Sekme();
        if (temas.contactCount > 0)
        {
            ContactPoint degme = temas.GetContact(0);
            Gorsel.Kivilcim(degme.point + degme.normal * 0.04f, ParlamaRengi());
        }
    }

    Color ParlamaRengi()
    {
        return tur switch
        {
            DuvarTuru.Sayili => new Color(0.55f, 1f, 0.65f),
            DuvarTuru.Kirmizi => new Color(1f, 0.4f, 0.32f),
            DuvarTuru.Hayalet => new Color(0.55f, 0.78f, 1f),
            DuvarTuru.Sari => new Color(1f, 0.86f, 0.25f),
            _ => new Color(1f, 0.86f, 0.6f)
        };
    }

    public void SayiyiAzalt()
    {
        if (kirildi || katilasti)
            return;

        if (tur != DuvarTuru.Sayili && tur != DuvarTuru.Hayalet)
            return;

        kalanCan--;
        YaziyiGuncelle();

        if (kalanCan > 0)
            return;

        if (tur == DuvarTuru.Sayili)
        {
            Ses.Kiril();
            StartCoroutine(DepremleKir());
        }
        else
            StartCoroutine(Katilastir());
    }

    void SekenYuzeyYap()
    {
        Collider yuzey = GetComponent<Collider>();
        if (yuzey == null)
            return;

        PhysicsMaterial seken = new PhysicsMaterial("AirHockeyDuvar");
        seken.bounciness = 1f;
        seken.dynamicFriction = 0f;
        seken.staticFriction = 0f;
        seken.frictionCombine = PhysicsMaterialCombine.Minimum;
        seken.bounceCombine = PhysicsMaterialCombine.Maximum;
        yuzey.material = seken;
    }

    void RengiUygula()
    {
        Renderer gorunum = GetComponent<Renderer>();
        if (gorunum == null)
            return;

        Color renk = tur switch
        {
            DuvarTuru.Sayili => new Color(0.15f, 0.72f, 0.28f),
            DuvarTuru.Kirmizi => new Color(0.75f, 0.12f, 0.12f),
            DuvarTuru.Sari => new Color(0.95f, 0.78f, 0.12f),
            _ => new Color(0.45f, 0.26f, 0.12f)
        };

        Material malzeme = gorunum.material;
        if (malzeme.HasProperty("_BaseMap"))
            malzeme.SetTexture("_BaseMap", Texture2D.whiteTexture);
        if (malzeme.HasProperty("_BaseColor"))
            malzeme.SetColor("_BaseColor", renk);
        if (malzeme.HasProperty("_Smoothness"))
            malzeme.SetFloat("_Smoothness", 0.12f);
        if (malzeme.HasProperty("_EmissionColor"))
            malzeme.SetColor("_EmissionColor", Color.black);
        malzeme.DisableKeyword("_EMISSION");
        malzeme.color = renk;
    }

    void YaziKur()
    {
        GameObject obje = new GameObject("Sayi");
        obje.transform.SetParent(transform, false);
        // Küpün kameraya bakan ön yüzüne yapışık dursun
        obje.transform.localPosition = new Vector3(0f, 0f, -0.51f);
        obje.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

        Vector3 olcek = transform.lossyScale;
        obje.transform.localScale = new Vector3(
            0.55f / Mathf.Max(0.01f, olcek.x),
            0.5f / Mathf.Max(0.01f, olcek.y),
            0.01f / Mathf.Max(0.01f, olcek.z));

        yazi = obje.AddComponent<TextMesh>();
        yazi.anchor = TextAnchor.MiddleCenter;
        yazi.alignment = TextAlignment.Center;
        yazi.color = Color.white;
        yazi.fontSize = 64;
        yazi.characterSize = 0.35f;
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null)
            yazi.font = font;

        Gorsel.Cercevele(yazi, 0.028f);
    }

    void YaziyiGuncelle()
    {
        if (yazi == null)
            return;

        string metin = kalanCan.ToString();
        yazi.text = metin;
        if (yazi.transform.parent == null)
            return;

        foreach (Transform cocuk in yazi.transform.parent)
        {
            if (cocuk.name != "SayiGolge")
                continue;

            TextMesh golge = cocuk.GetComponent<TextMesh>();
            if (golge != null)
                golge.text = metin;
        }
    }

    void HayaletBaslat()
    {
        Collider yuzey = GetComponent<Collider>();
        if (yuzey != null)
            yuzey.isTrigger = true;

        Renderer gorunum = GetComponent<Renderer>();
        if (gorunum == null)
            return;

        yuzeyMalzemesi = gorunum.material;
        if (yuzeyMalzemesi.HasProperty("_BaseMap"))
            yuzeyMalzemesi.SetTexture("_BaseMap", Texture2D.whiteTexture);
        SaydamYap(0.28f);
    }

    void SaydamYap(float alfa)
    {
        if (yuzeyMalzemesi == null)
            return;

        Color renk = maviRenk;
        renk.a = alfa;
        yuzeyMalzemesi.SetFloat("_Surface", 1f);
        yuzeyMalzemesi.SetFloat("_Blend", 0f);
        yuzeyMalzemesi.SetOverrideTag("RenderType", "Transparent");
        yuzeyMalzemesi.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        yuzeyMalzemesi.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        yuzeyMalzemesi.SetInt("_ZWrite", 0);
        yuzeyMalzemesi.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        yuzeyMalzemesi.renderQueue = 3000;
        yuzeyMalzemesi.SetColor("_BaseColor", renk);
        yuzeyMalzemesi.color = renk;
        yuzeyMalzemesi.DisableKeyword("_EMISSION");
        if (yuzeyMalzemesi.HasProperty("_EmissionColor"))
            yuzeyMalzemesi.SetColor("_EmissionColor", Color.black);
    }

    System.Collections.IEnumerator Katilastir()
    {
        if (katilasti)
            yield break;

        katilasti = true;
        float sure = 0.35f;
        float gecen = 0f;
        while (gecen < sure)
        {
            gecen += Time.deltaTime;
            float oran = Mathf.SmoothStep(0f, 1f, gecen / sure);
            SaydamYap(Mathf.Lerp(0.28f, 1f, oran));
            yield return null;
        }

        yuzeyMalzemesi.SetFloat("_Surface", 0f);
        yuzeyMalzemesi.SetOverrideTag("RenderType", "Opaque");
        yuzeyMalzemesi.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        yuzeyMalzemesi.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        yuzeyMalzemesi.SetInt("_ZWrite", 1);
        yuzeyMalzemesi.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        yuzeyMalzemesi.renderQueue = 2000;
        if (yuzeyMalzemesi.HasProperty("_BaseMap"))
            yuzeyMalzemesi.SetTexture("_BaseMap", Texture2D.whiteTexture);
        yuzeyMalzemesi.SetColor("_BaseColor", maviRenk);
        yuzeyMalzemesi.color = maviRenk;
        if (yuzeyMalzemesi.HasProperty("_Smoothness"))
            yuzeyMalzemesi.SetFloat("_Smoothness", 0.08f);
        yuzeyMalzemesi.DisableKeyword("_EMISSION");

        Collider yuzey = GetComponent<Collider>();
        if (yuzey != null)
            yuzey.isTrigger = false;
    }

    System.Collections.IEnumerator DepremleKir()
    {
        if (kirildi)
            yield break;

        kirildi = true;
        Vector3 merkez = transform.position;
        Quaternion donus = transform.rotation;
        float sure = 0.32f;
        float gecen = 0f;

        while (gecen < sure)
        {
            gecen += Time.deltaTime;
            float oran = Mathf.SmoothStep(0f, 1f, gecen / sure);
            float genlik = Mathf.Lerp(0.07f, 0f, oran);
            float dalga = Mathf.Sin(gecen * 42f);
            transform.position = merkez + transform.right * (dalga * genlik);
            transform.rotation = donus * Quaternion.Euler(0f, 0f, dalga * genlik * 28f);
            yield return null;
        }

        transform.position = merkez;
        transform.rotation = donus;
        ParcalariDok();
        Gorsel.Toz(merkez, new Color(0.15f, 0.72f, 0.28f));

        Renderer gorunum = GetComponent<Renderer>();
        if (gorunum != null)
            gorunum.enabled = false;
        Collider yuzey = GetComponent<Collider>();
        if (yuzey != null)
            yuzey.enabled = false;
        if (yazi != null)
            yazi.gameObject.SetActive(false);

        Destroy(gameObject, 0.85f);
    }

    void ParcalariDok()
    {
        Color yesil = new Color(0.15f, 0.72f, 0.28f);
        for (int i = 0; i < 4; i++)
        {
            GameObject parca = GameObject.CreatePrimitive(PrimitiveType.Cube);
            parca.name = "Kirik";
            parca.transform.position = transform.position + transform.right * ((i - 1.5f) * 0.18f);
            parca.transform.rotation = transform.rotation;
            parca.transform.localScale = transform.lossyScale * 0.42f;

            parca.GetComponent<Renderer>().sharedMaterial = Gorsel.Lit(yesil, 0.12f, 0f);

            Rigidbody govde = parca.AddComponent<Rigidbody>();
            govde.interpolation = RigidbodyInterpolation.Interpolate;
            govde.useGravity = true;
            govde.linearVelocity = transform.right * ((i - 1.5f) * 0.35f) + Vector3.up * 0.15f;
            govde.angularVelocity = new Vector3(0f, 0f, (i - 1.5f) * 1.2f);
            Destroy(parca, 0.8f);
        }
    }
}

// Sarı duvar alçalınca yolu kapatır, yükselince kapağın altından geçmesine izin verir.
public class YukariAsagi : MonoBehaviour
{
    Vector3 dip;
    Rigidbody govde;

    void Start()
    {
        dip = transform.position;
        govde = GetComponent<Rigidbody>();
    }

    public void DipiYenile()
    {
        dip = transform.position;
    }

    void FixedUpdate()
    {
        if (GetComponent<Yikinti>() != null)
        {
            enabled = false;
            return;
        }

        float yukari = (Mathf.Sin(Time.time * 1.5f) + 1f) * 0.5f;
        Vector3 yer = dip;
        yer.y += yukari * 1.2f;
        if (govde != null)
            govde.MovePosition(yer);
        else
            transform.position = yer;
    }
}

// Çamur kapağı durdurmaz, sadece yavaşlatır.
public class Camur : MonoBehaviour
{
    void OnTriggerStay(Collider diger)
    {
        if (diger.GetComponent<KapakFirlatma>() == null)
            return;

        Rigidbody govde = diger.attachedRigidbody;
        if (govde == null)
            return;

        Vector3 hiz = govde.linearVelocity;
        hiz.x *= 0.93f;
        hiz.z *= 0.93f;
        govde.linearVelocity = hiz;
    }
}
