using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class KapakFirlatma : MonoBehaviour
{
    public float gucCarpani = 3f; // Atış gücünü Unity'den değiştirebilirsin
    
    private Rigidbody rb;
    private LineRenderer cizgi;
    private Vector3 baslangicNoktasi;
    private Vector3 firlatmaYonu;
    private bool firlatildi = false;
    private bool bitti = false;
    private Transform[] nisanNoktalari;
    private const int NoktaSayisi = 7;
    private CopAdam atici;
    private bool oyunBasladi;
    private bool basliyor;
    private GameObject menu;
    private TrailRenderer iz;
    private bool obrukVar;
    private Vector3 obrukMerkez;
    private float obrukYaricap;
    private bool obrukAtlandi;
    private bool dusuyor;
    private bool zipliyor;
    private bool dusmeEkraniAcildi;
    private bool hedefeDokunuldu;
    private float masaY = 5.12f;

    void Awake()
    {
        // Bu uyarı oyun başlamadan da konsola düşer; en başta kapat
        MeshCollider govde = GetComponent<MeshCollider>();
        if (govde != null && !govde.convex)
            govde.enabled = false;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cizgi = GetComponent<LineRenderer>(); // Yörünge çizgisi için

        YerCekiminiDuzelt();
        Gorsel.SahneyiKur();
        iz = Gorsel.KapagiGiydir(gameObject);
        AticiyiKoy();
        HedefiHazirla();
        MaviDuvarKoy();
        if (AnaMenu.Bolum() == OyunBolumu.Test)
            TestBolumuKur();
        else if (AnaMenu.Bolum() == OyunBolumu.Rastgele)
            RastgeleBolumKur();
        else
        {
            DuvarlariDiz();
            CamurlariKoy();
        }
        menu = AnaMenu.Goster(this);
        if (AnaMenu.DirektOynaIste())
            BolumuAc();
        NisanNoktalariniKur();

        // Eski pembe çubuk bir daha görünmesin
        if (cizgi != null)
            cizgi.enabled = false;
    }

    void NisanNoktalariniKur()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        Material beyaz = shader != null ? new Material(shader) : new Material(Shader.Find("Standard"));
        beyaz.color = Color.white;
        if (beyaz.HasProperty("_BaseColor"))
            beyaz.SetColor("_BaseColor", Color.white);
        if (beyaz.HasProperty("_Smoothness"))
            beyaz.SetFloat("_Smoothness", 0.15f);

        nisanNoktalari = new Transform[NoktaSayisi];
        for (int i = 0; i < NoktaSayisi; i++)
        {
            GameObject nokta = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nokta.name = "Nisan Nokta";
            Destroy(nokta.GetComponent<Collider>());
            nokta.transform.localScale = Vector3.one * 0.5f;

            Renderer gorunum = nokta.GetComponent<Renderer>();
            gorunum.sharedMaterial = beyaz;
            gorunum.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            gorunum.receiveShadows = false;

            nokta.SetActive(false);
            nisanNoktalari[i] = nokta.transform;
        }
    }

    // Kapak yere düşmesin ve zeminin içine gömülüp sıkışmasın
    void YerCekiminiDuzelt()
    {
        // Rigidbody'li objede içbükey MeshCollider fizik motorunu bozar, kapak zeminden geçer
        MeshCollider govde = GetComponent<MeshCollider>();
        if (govde != null && !govde.convex)
            govde.enabled = false;

        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        // Air hockey: masada kayar, sürtünme neredeyse yok. Yavaşlama duvara çarpınca olur.
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.35f;

        PhysicsMaterial kaygan = new PhysicsMaterial("AirHockeyKapak");
        kaygan.bounciness = 1f;
        kaygan.dynamicFriction = 0.01f;
        kaygan.staticFriction = 0.01f;
        kaygan.frictionCombine = PhysicsMaterialCombine.Minimum;
        kaygan.bounceCombine = PhysicsMaterialCombine.Maximum;

        foreach (Collider parca in GetComponents<Collider>())
            parca.material = kaygan;

        PhysicsMaterial masa = new PhysicsMaterial("AirHockeyMasa");
        masa.bounciness = 0f;
        masa.dynamicFriction = 0.02f;
        masa.staticFriction = 0.02f;
        masa.frictionCombine = PhysicsMaterialCombine.Minimum;
        masa.bounceCombine = PhysicsMaterialCombine.Minimum;

        // Sadece masada kayar: aşağı düşmez, yan yatmaz
        rb.constraints =
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        float yariYukseklik = 0.1f;
        CapsuleCollider kapsul = GetComponent<CapsuleCollider>();
        if (kapsul != null)
            yariYukseklik = kapsul.height * 0.5f * Mathf.Abs(transform.lossyScale.y);

        float zeminUstu = 5f;
        GameObject zemin = GameObject.Find("Zemin");
        if (zemin != null)
        {
            Collider zeminCarpisma = zemin.GetComponent<Collider>();
            if (zeminCarpisma != null)
            {
                zeminUstu = zeminCarpisma.bounds.max.y;
                zeminCarpisma.material = masa;
            }
        }

        Vector3 konum = transform.position;
        konum.y = zeminUstu + yariYukseklik + 0.02f;
        masaY = konum.y;
        transform.position = konum;
        rb.position = konum;
    }

    void AticiyiKoy()
    {
        // Kapağın yanında, masanın üstünde, hedefe bakar.
        Vector3 ayak = transform.position;
        ayak.x -= 1.05f;
        ayak.y = 5f;

        GameObject obje = new GameObject("Atici");
        obje.transform.position = ayak;
        atici = obje.AddComponent<CopAdam>();
        if (!Gorsel.Karakter(obje.transform, "01"))
            atici.Kur(new Color(0.05f, 0.62f, 0.95f), true);
    }

    public void BolumuAc()
    {
        if (basliyor || oyunBasladi)
            return;

        basliyor = true;
        if (menu != null)
        {
            AnaMenu ana = menu.GetComponent<AnaMenu>();
            if (ana != null)
                ana.OkuGoster();
            menu.SetActive(false);
        }
        StartCoroutine(OyunuBaslat());
    }

    public void TesteGec()
    {
        AnaMenu.BolumSec(OyunBolumu.Test);
        TestBolumuKur();
        BolumuAc();
    }

    public void RastgeleGec()
    {
        AnaMenu.BolumSec(OyunBolumu.Rastgele);
        RastgeleBolumKur();
        BolumuAc();
    }

    IEnumerator OyunuBaslat()
    {

        Transform kamera = Camera.main.transform;
        Vector3 bas = kamera.position;
        Quaternion basDonus = kamera.rotation;

        Vector3 ense = atici != null ? atici.transform.position + new Vector3(0.35f, 1.05f, 0f) : bas;
        Vector3 hedef;
        Quaternion hedefDonus;
        if (AnaMenu.Bolum() == OyunBolumu.Test)
        {
            Camera.main.fieldOfView = 62f;
            hedef = new Vector3(0f, 32f, -9f);
            hedefDonus = Quaternion.Euler(55f, 0f, 0f);
        }
        else
        {
            hedef = Vector3.Lerp(bas, ense + new Vector3(0f, 1.4f, -2.4f), 0.42f);
            hedefDonus = Quaternion.Euler(34f, 0f, 0f);
        }

        float sure = 0.45f;
        float gecen = 0f;
        while (gecen < sure)
        {
            gecen += Time.deltaTime;
            float oran = Mathf.SmoothStep(0f, 1f, gecen / sure);
            kamera.position = Vector3.Lerp(bas, hedef, oran);
            kamera.rotation = Quaternion.Slerp(basDonus, hedefDonus, oran);
            yield return null;
        }

        kamera.position = hedef;
        kamera.rotation = hedefDonus;

        EkranSallama salla = kamera.GetComponent<EkranSallama>();
        if (salla != null)
            salla.KonumuKaydet();

        oyunBasladi = true;
    }

    void MaviDuvarKoy()
    {
        if (GameObject.Find("Mavi Duvar") != null)
            return;

        GameObject kutu = GameObject.CreatePrimitive(PrimitiveType.Cube);
        kutu.name = "Mavi Duvar";
        kutu.transform.position = new Vector3(1.55f, 5.4f, 8.6f);
        kutu.transform.localScale = new Vector3(1.7f, 0.8f, 0.35f);
        kutu.transform.rotation = Quaternion.Euler(0f, -20f, 0f);

        Duvar duvar = kutu.AddComponent<Duvar>();
        duvar.tur = DuvarTuru.Hayalet;
        duvar.baslangicCan = 10;
    }

    void DuvarlariDiz()
    {
        Yerlestir("Engel 1", new Vector3(-1.4f, 5.4f, 5.5f), new Vector3(2.2f, 0.8f, 0.32f), 22f);
        Yerlestir("Engel 2", new Vector3(1.05f, 5.4f, 10.8f), new Vector3(2.6f, 0.8f, 0.3f), -35f);
        Yerlestir("Mavi Duvar", new Vector3(-0.35f, 5.4f, 8.2f), new Vector3(1.8f, 0.8f, 0.32f), 14f);
        SariDuvarKoy();
    }

    void Yerlestir(string ad, Vector3 yer, Vector3 olcek, float donus)
    {
        GameObject duvar = GameObject.Find(ad);
        if (duvar == null)
            return;

        duvar.transform.position = yer;
        duvar.transform.localScale = olcek;
        duvar.transform.rotation = Quaternion.Euler(0f, donus, 0f);
        duvar.SetActive(true);
        YukariAsagi inis = duvar.GetComponent<YukariAsagi>();
        if (inis != null)
            inis.DipiYenile();
    }

    void TestBolumuKur()
    {
        MasayiGenislet();
        MaviDuvarKoy();
        SariDuvarKoy();
        CamurTemizle();
        Yerlestir("Engel 1", new Vector3(-3.6f, 5.4f, 11.2f), new Vector3(2.2f, 0.8f, 0.32f), 16f);
        Yerlestir("Mavi Duvar", new Vector3(3.4f, 5.4f, 11.2f), new Vector3(2f, 0.8f, 0.32f), -12f);
        Yerlestir("Sari Duvar", new Vector3(0f, 5.4f, 14.4f), new Vector3(2.2f, 0.8f, 0.3f), 0f);
        Yerlestir("Engel 2", new Vector3(3.6f, 5.4f, 17.2f), new Vector3(2.4f, 0.8f, 0.3f), 20f);
        CamurKoy(new Vector3(-3.2f, 5.04f, 3.4f), new Vector3(2.6f, 0.05f, 1.6f));

        GameObject hedef = GameObject.Find("Hedef");
        if (hedef != null)
        {
            Vector3 yer = hedef.transform.position;
            yer.x = 0f;
            yer.z = 18.6f;
            hedef.transform.position = yer;
        }

        ObrukKoy(new Vector3(0f, 5f, 7.4f), 1.25f);
    }

    void RastgeleBolumKur()
    {
        MaviDuvarKoy();
        SariDuvarKoy();
        CamurTemizle();
        obrukVar = false;

        bool yesil = false;
        bool kirmizi = false;
        bool mavi = false;
        bool sari = false;
        bool camur = false;
        int secilen = 0;
        while (secilen < 1)
        {
            yesil = Random.value > 0.45f;
            kirmizi = Random.value > 0.45f;
            mavi = Random.value > 0.45f;
            sari = Random.value > 0.5f;
            camur = Random.value > 0.45f;
            secilen = (yesil ? 1 : 0) + (kirmizi ? 1 : 0) + (mavi ? 1 : 0) + (sari ? 1 : 0) + (camur ? 1 : 0);
            if (secilen == 5)
            {
                int birak = Random.Range(0, 5);
                if (birak == 0) yesil = false;
                else if (birak == 1) kirmizi = false;
                else if (birak == 2) mavi = false;
                else if (birak == 3) sari = false;
                else camur = false;
                secilen = 4;
            }
        }

        float z = 4.2f;
        if (yesil)
        {
            Yerlestir("Engel 1", new Vector3(Random.Range(-1.6f, 0.4f), 5.4f, z), new Vector3(Random.Range(1.6f, 2.2f), 0.8f, 0.32f), Random.Range(-28f, 28f));
            z += Random.Range(2.1f, 3f);
        }
        else
            Gizle("Engel 1");

        if (mavi)
        {
            Yerlestir("Mavi Duvar", new Vector3(Random.Range(-0.4f, 1.5f), 5.4f, z), new Vector3(Random.Range(1.5f, 2f), 0.8f, 0.32f), Random.Range(-20f, 20f));
            z += Random.Range(2.1f, 3f);
        }
        else
            Gizle("Mavi Duvar");

        if (sari)
        {
            Yerlestir("Sari Duvar", new Vector3(Random.Range(-1.1f, 1.1f), 5.4f, z), new Vector3(1.8f, 0.8f, 0.3f), Random.Range(-16f, 16f));
            z += Random.Range(2.1f, 3f);
        }
        else
            Gizle("Sari Duvar");

        if (kirmizi)
        {
            Yerlestir("Engel 2", new Vector3(Random.Range(-0.3f, 1.5f), 5.4f, z), new Vector3(2.2f, 0.8f, 0.3f), Random.Range(-35f, 35f));
            z += Random.Range(2.1f, 3f);
        }
        else
            Gizle("Engel 2");

        if (camur)
            CamurKoy(new Vector3(Random.Range(-1f, 1f), 5.04f, Mathf.Min(z, 14.5f)), new Vector3(2f, 0.05f, 1.3f));
    }

    void Gizle(string ad)
    {
        GameObject obje = GameObject.Find(ad);
        if (obje != null)
            obje.SetActive(false);
    }

    void MasayiGenislet()
    {
        OlcekliKoy("Zemin", new Vector3(0f, 4.95f, 10f), new Vector3(14.2f, 0.1f, 20f));
        OlcekliKoy("Sol Duvar", new Vector3(-7.05f, 5.5f, 10f), new Vector3(0.35f, 1f, 20f));
        OlcekliKoy("Sağ Duvar", new Vector3(7.05f, 5.5f, 10f), new Vector3(0.35f, 1f, 20f));
        OlcekliKoy("Arka Duvar", new Vector3(0f, 5.5f, 20.15f), new Vector3(14.6f, 1f, 0.35f));
        OlcekliKoy("Ön Duvar", new Vector3(0f, 5.5f, -0.15f), new Vector3(14.6f, 1f, 0.35f));

        GameObject oda = GameObject.Find("KenneyOda");
        if (oda == null)
            return;

        foreach (Transform parca in oda.transform)
        {
            if (parca.name == "floorFull")
                continue;
            bool disDuvar = Mathf.Abs(parca.position.x) > 6.6f || parca.position.z > 19.6f;
            if (!disDuvar)
                parca.gameObject.SetActive(false);
        }
    }

    void OlcekliKoy(string ad, Vector3 yer, Vector3 olcek)
    {
        GameObject obje = GameObject.Find(ad);
        if (obje == null)
            return;

        obje.transform.position = yer;
        obje.transform.localScale = olcek;
    }

    void CamurTemizle()
    {
        Camur[] camurlar = Object.FindObjectsByType<Camur>();
        foreach (Camur camur in camurlar)
            Destroy(camur.gameObject);
    }

    void ObrukKoy(Vector3 merkez, float yaricap)
    {
        obrukVar = true;
        obrukMerkez = merkez;
        obrukYaricap = yaricap;
        ZeminiDel(merkez, yaricap);
        if (GameObject.Find("Obruk") != null)
            return;

        GameObject kuyu = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        kuyu.name = "Obruk";
        Collider kuyuCarpma = kuyu.GetComponent<Collider>();
        if (kuyuCarpma != null)
            Destroy(kuyuCarpma);
        kuyu.transform.position = new Vector3(merkez.x, 2.9f, merkez.z);
        kuyu.transform.localScale = new Vector3(yaricap * 2f, 2f, yaricap * 2f);
        kuyu.GetComponent<Renderer>().sharedMaterial = Gorsel.Lit(new Color(0.04f, 0.03f, 0.025f), 0.02f, 0f);

        GameObject oda = GameObject.Find("KenneyOda");
        if (oda == null)
            return;

        foreach (Transform karo in oda.transform)
        {
            if (karo.name != "floorFull")
                continue;
            Vector3 orta = karo.position + new Vector3(-0.6f, 0f, 0.6f);
            float dx = orta.x - merkez.x;
            float dz = orta.z - merkez.z;
            if (dx * dx + dz * dz < yaricap * yaricap)
                karo.gameObject.SetActive(false);
        }
    }

    void ZeminiDel(Vector3 merkez, float yaricap)
    {
        if (GameObject.Find("ObrukZemin") != null)
            return;

        GameObject zemin = GameObject.Find("Zemin");
        if (zemin != null)
        {
            Collider tam = zemin.GetComponent<Collider>();
            if (tam != null)
                tam.enabled = false;
        }

        float x0 = -7.05f;
        float x1 = 7.05f;
        float z0 = 0f;
        float z1 = 20f;
        float sol = merkez.x - yaricap;
        float sag = merkez.x + yaricap;
        float on = merkez.z - yaricap;
        float arka = merkez.z + yaricap;

        GameObject kok = new GameObject("ObrukZemin");
        ZeminSeridi(kok.transform, (x0 + x1) * 0.5f, (z0 + on) * 0.5f, x1 - x0, on - z0);
        ZeminSeridi(kok.transform, (x0 + x1) * 0.5f, (arka + z1) * 0.5f, x1 - x0, z1 - arka);
        ZeminSeridi(kok.transform, (x0 + sol) * 0.5f, (on + arka) * 0.5f, sol - x0, arka - on);
        ZeminSeridi(kok.transform, (sag + x1) * 0.5f, (on + arka) * 0.5f, x1 - sag, arka - on);
    }

    void ZeminSeridi(Transform ebeveyn, float x, float z, float en, float boy)
    {
        if (en < 0.05f || boy < 0.05f)
            return;

        GameObject serit = GameObject.CreatePrimitive(PrimitiveType.Cube);
        serit.name = "ZeminSerit";
        serit.transform.SetParent(ebeveyn, false);
        serit.transform.position = new Vector3(x, 4.95f, z);
        serit.transform.localScale = new Vector3(en, 0.1f, boy);

        Renderer gorunum = serit.GetComponent<Renderer>();
        if (gorunum != null)
            gorunum.enabled = false;

        Collider yuzey = serit.GetComponent<Collider>();
        if (yuzey == null)
            return;

        PhysicsMaterial masa = new PhysicsMaterial("AirHockeyMasaParcasi");
        masa.bounciness = 0f;
        masa.dynamicFriction = 0.02f;
        masa.staticFriction = 0.02f;
        masa.frictionCombine = PhysicsMaterialCombine.Minimum;
        masa.bounceCombine = PhysicsMaterialCombine.Minimum;
        yuzey.material = masa;
    }

    bool ObrukIcinde()
    {
        float dx = transform.position.x - obrukMerkez.x;
        float dz = transform.position.z - obrukMerkez.z;
        return dx * dx + dz * dz <= obrukYaricap * obrukYaricap;
    }

    void ObrukKontrol()
    {
        if (!obrukVar || dusuyor || zipliyor || obrukAtlandi)
            return;
        if (!ObrukIcinde())
            return;

        Vector3 hiz = rb.linearVelocity;
        float yatay = Mathf.Sqrt(hiz.x * hiz.x + hiz.z * hiz.z);
        if (yatay >= 6.5f)
        {
            zipliyor = true;
            obrukAtlandi = true;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            hiz.y = 4.2f;
            rb.linearVelocity = hiz;
            return;
        }

        ObrugaDus();
    }

    void ZiplamaInis()
    {
        if (ObrukIcinde())
        {
            if (transform.position.y < masaY - 0.35f)
            {
                zipliyor = false;
                ObrugaDus();
            }
            return;
        }

        if (transform.position.y > masaY + 0.25f)
            return;

        Vector3 yer = rb.position;
        yer.y = masaY;
        rb.position = yer;
        transform.position = yer;
        Vector3 hiz = rb.linearVelocity;
        hiz.y = 0f;
        rb.linearVelocity = hiz;
        rb.useGravity = false;
        rb.constraints =
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;
        zipliyor = false;
        obrukAtlandi = false;
    }

    void DususuIzle()
    {
        if (dusmeEkraniAcildi || transform.position.y > 3.1f)
            return;

        dusmeEkraniAcildi = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        AnaMenu.DusmeEkrani();
    }

    void ObrugaDus()
    {
        if (dusuyor)
            return;

        dusuyor = true;
        bitti = true;
        firlatildi = true;
        NisanNoktalariniGizle();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void SariDuvarKoy()
    {
        if (GameObject.Find("Sari Duvar") != null)
            return;

        GameObject kutu = GameObject.CreatePrimitive(PrimitiveType.Cube);
        kutu.name = "Sari Duvar";
        kutu.transform.position = new Vector3(0.15f, 5.4f, 13.2f);
        kutu.transform.localScale = new Vector3(2.3f, 0.8f, 0.3f);
        kutu.transform.rotation = Quaternion.Euler(0f, 8f, 0f);

        Rigidbody govde = kutu.AddComponent<Rigidbody>();
        govde.isKinematic = true;
        govde.useGravity = false;

        Duvar duvar = kutu.AddComponent<Duvar>();
        duvar.tur = DuvarTuru.Sari;
        kutu.AddComponent<YukariAsagi>();
    }

    void CamurlariKoy()
    {
        CamurKoy(new Vector3(0.15f, 5.04f, 4.15f), new Vector3(2.5f, 0.05f, 1.65f));
        CamurKoy(new Vector3(-0.45f, 5.04f, 12.15f), new Vector3(2.1f, 0.05f, 1.45f));
    }

    void CamurKoy(Vector3 yer, Vector3 olcek)
    {
        GameObject camur = GameObject.CreatePrimitive(PrimitiveType.Cube);
        camur.name = "Camur";
        camur.transform.position = yer;
        camur.transform.localScale = olcek;

        Collider yuzey = camur.GetComponent<Collider>();
        if (yuzey != null)
            yuzey.isTrigger = true;

        Renderer gorunum = camur.GetComponent<Renderer>();
        gorunum.sharedMaterial = Gorsel.Lit(new Color(0.32f, 0.24f, 0.12f), 0.04f, 0f);
        gorunum.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        camur.AddComponent<Camur>();
    }

    void HedefiHazirla()
    {
        GameObject hedef = GameObject.Find("Hedef");
        if (hedef != null && hedef.GetComponent<HedefDavranis>() == null)
            hedef.AddComponent<HedefDavranis>();
    }

    void Update()
    {
        if (!oyunBasladi)
            return;

        if (zipliyor)
            ZiplamaInis();
        if (dusuyor)
            DususuIzle();

        if (firlatildi && !dusuyor && !bitti)
            ObrukKontrol();

        Pointer isaretci = Pointer.current;
        if (isaretci != null && !bitti && isaretci.press.wasPressedThisFrame && HedefeTiklandi(isaretci.position.ReadValue()))
        {
            hedefeDokunuldu = true;
            GameObject hedefAdam = GameObject.Find("Hedef");
            if (hedefAdam != null)
            {
                HedefDavranis davranis = hedefAdam.GetComponent<HedefDavranis>();
                if (davranis != null)
                    davranis.Sinirlen();
            }
        }

        if (firlatildi || bitti) return;

        if (isaretci == null)
            return;

        if (hedefeDokunuldu)
        {
            if (isaretci.press.wasReleasedThisFrame)
                hedefeDokunuldu = false;
            return;
        }

        Vector2 ekran = isaretci.position.ReadValue();

        // Ekrana ilk dokunulduğunda / Tıklandığında
        if (isaretci.press.wasPressedThisFrame)
            baslangicNoktasi = EkraniDunyayaCevir(ekran);

        // Parmağı / Fareyi ekranda sürüklerken
        if (isaretci.press.isPressed)
        {
            Vector3 guncelNokta = EkraniDunyayaCevir(ekran);
            firlatmaYonu = baslangicNoktasi - guncelNokta; // Geriye çektiğimiz için yönü ters alıyoruz
            firlatmaYonu.y = 0; // Kapağın havaya kalkmasını engelle, sadece masada gitsin
            NisanNoktalariniGuncelle();
        }

        // Parmağı / Fareyi bıraktığında fırlat
        if (isaretci.press.wasReleasedThisFrame)
        {
            NisanNoktalariniGizle();

            if (atici != null)
                atici.HizliAt();

            if (firlatmaYonu.magnitude > 0.08f)
                Ses.Firlat();

            // Fiziğe gücü uygula (Impulse: anlık patlama gücü)
            rb.AddForce(firlatmaYonu * gucCarpani, ForceMode.Impulse);
            firlatildi = true;
            if (iz != null)
                iz.emitting = true;
        }
    }

    // Çarpışma Kontrolü (Öğretmeni Vurma)
    void OnCollisionEnter(Collision temas)
    {
        if (bitti)
            return;

        if (temas.gameObject.CompareTag("Hedef"))
        {
            bitti = true;
            HedefDavranis hedef = temas.gameObject.GetComponent<HedefDavranis>();
            if (hedef == null)
                hedef = temas.gameObject.AddComponent<HedefDavranis>();

            Vector3 yon = rb.linearVelocity;
            yon.y = 0f;
            if (yon.sqrMagnitude < 0.01f)
                yon = firlatmaYonu;
            yon.y = 0f;
            hedef.YatayDus(yon);
            Ses.Kazan();
            Debug.Log("Öğretmen Vuruldu! Ağır çekim devrede.");
            rb.linearVelocity = Vector3.zero; // Çarptığı an kapağı durdur (yapışsın)
            
            // Ağır çekim ve bölüm yeniden başlatma sürecini başlat
            StartCoroutine(AgirCekimVeYenidenBaslat());
        }
    }

    // Ağır Çekim ve Yeniden Başlatma Senaryosu
    IEnumerator AgirCekimVeYenidenBaslat()
    {
        Time.timeScale = 0.2f; // Zamanı %20 hızına düşür (Slow motion)
        
        // Gerçek zamanla 2 saniye bekle (Oyun yavaşlasa bile biz 2 saniye bekleyeceğiz)
        yield return new WaitForSecondsRealtime(2f);
        
        Time.timeScale = 1f; // Zamanı normale döndür
        
        // Mevcut bölümü baştan yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Kırmızı duvara değince çağrılır
    public void Elendi()
    {
        if (bitti)
            return;

        bitti = true;
        firlatildi = true;
        NisanNoktalariniGizle();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        Debug.Log("Elendin!");
        ElendinYazisi();
        Ses.Kaybedince();
        StartCoroutine(Cehennem());
        StartCoroutine(KisaBekleVeYenile());
    }

    void ElendinYazisi()
    {
        GameObject obje = new GameObject("Elendin");
        TextMesh yazi = obje.AddComponent<TextMesh>();
        yazi.text = "yarra yedin";
        yazi.anchor = TextAnchor.MiddleCenter;
        yazi.alignment = TextAlignment.Center;
        yazi.color = new Color(1f, 0.32f, 0.05f);
        yazi.fontSize = 48;
        yazi.characterSize = 0.11f;
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null)
            yazi.font = font;

        if (Camera.main != null)
        {
            obje.transform.SetParent(Camera.main.transform, false);
            obje.transform.localPosition = new Vector3(0f, 0.55f, 6f);
            obje.transform.localRotation = Quaternion.identity;
        }

        Gorsel.Cercevele(yazi, 0.016f);
    }

    IEnumerator Cehennem()
    {
        if (Camera.main != null)
            Camera.main.backgroundColor = new Color(0.18f, 0.02f, 0.01f);

        RenderSettings.ambientLight = new Color(0.45f, 0.08f, 0.02f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.35f, 0.05f, 0.01f);
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.045f;

        Light[] isiklar = Object.FindObjectsByType<Light>();
        foreach (Light isik in isiklar)
        {
            if (isik.type != LightType.Directional)
                continue;
            isik.color = new Color(1f, 0.28f, 0.05f);
            isik.intensity = 1.6f;
        }

        EkranSallama.Deprem(14f);
        AlevleriYak();
        LavlariCikar();
        YikimiBaslat();
        StartCoroutine(GokyuzuKizar());
        yield return AzrailKacirsin();
    }

    IEnumerator GokyuzuKizar()
    {
        float bas = Time.unscaledTime;
        while (Time.unscaledTime - bas < 14f)
        {
            float nabiz = 0.5f + Mathf.Abs(Mathf.Sin(Time.unscaledTime * 7f)) * 0.5f;
            RenderSettings.fogDensity = 0.06f + nabiz * 0.05f;
            RenderSettings.ambientLight = Color.Lerp(new Color(0.25f, 0.02f, 0.01f), new Color(0.7f, 0.12f, 0.02f), nabiz);
            if (Camera.main != null)
                Camera.main.backgroundColor = Color.Lerp(new Color(0.08f, 0f, 0f), new Color(0.45f, 0.05f, 0f), nabiz);
            yield return null;
        }
    }

    void LavlariCikar()
    {
        Material lav = Gorsel.Lit(new Color(1f, 0.28f, 0.02f), 0.15f, 0f);
        lav.EnableKeyword("_EMISSION");
        if (lav.HasProperty("_EmissionColor"))
            lav.SetColor("_EmissionColor", new Color(1f, 0.35f, 0.02f) * 3.5f);

        GameObject kok = new GameObject("Lavlar");
        for (int i = 0; i < 16; i++)
        {
            GameObject catlak = GameObject.CreatePrimitive(PrimitiveType.Cube);
            catlak.name = "Lav";
            Collider carpma = catlak.GetComponent<Collider>();
            if (carpma != null)
            {
                carpma.enabled = false;
                Destroy(carpma);
            }

            float x = Random.Range(-6.2f, 6.2f);
            float z = Random.Range(-1.2f, 18.5f);
            catlak.transform.SetParent(kok.transform, false);
            catlak.transform.position = new Vector3(x, 4.72f, z);
            catlak.transform.localScale = new Vector3(Random.Range(0.35f, 0.9f), 0.12f, Random.Range(1.1f, 2.8f));
            catlak.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 180f), 0f);
            catlak.GetComponent<Renderer>().sharedMaterial = lav;
            catlak.AddComponent<Lav>();
        }

        GameObject isikObje = new GameObject("LavIsigi");
        isikObje.transform.position = new Vector3(0f, 6.2f, 9f);
        Light isik = isikObje.AddComponent<Light>();
        isik.type = LightType.Point;
        isik.color = new Color(1f, 0.22f, 0.02f);
        isik.intensity = 6f;
        isik.range = 22f;
    }

    void YikimiBaslat()
    {
        GameObject oda = GameObject.Find("KenneyOda");
        if (oda != null)
        {
            foreach (Transform cocuk in oda.transform)
                cocuk.gameObject.AddComponent<Yikinti>().Kur(Random.Range(0.6f, 4.2f));
        }

        Duvar[] duvarlar = Object.FindObjectsByType<Duvar>();
        foreach (Duvar duvar in duvarlar)
            duvar.gameObject.AddComponent<Yikinti>().Kur(Random.Range(0.8f, 3.4f));
    }

    void AlevleriYak()
    {
        Material alev = Gorsel.Lit(new Color(1f, 0.35f, 0.05f), 0.05f, 0f);
        alev.EnableKeyword("_EMISSION");
        if (alev.HasProperty("_EmissionColor"))
            alev.SetColor("_EmissionColor", new Color(1f, 0.22f, 0.02f) * 2.2f);

        for (int i = 0; i < 42; i++)
        {
            GameObject dil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dil.name = "Alev";
            Collider carpma = dil.GetComponent<Collider>();
            if (carpma != null)
            {
                carpma.enabled = false;
                Destroy(carpma);
            }

            dil.transform.position = new Vector3(Random.Range(-6.4f, 6.4f), 5.12f, Random.Range(-1f, 19f));
            dil.GetComponent<Renderer>().sharedMaterial = alev;
            AlevDil alevDil = dil.AddComponent<AlevDil>();
            alevDil.kalinlik = Random.Range(0.16f, 0.55f);
            alevDil.yukseklik = Random.Range(0.7f, 2.4f);
        }

        for (int i = 0; i < 24; i++)
        {
            GameObject kor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            kor.name = "Kor";
            Collider korCarpma = kor.GetComponent<Collider>();
            if (korCarpma != null)
            {
                korCarpma.enabled = false;
                Destroy(korCarpma);
            }

            kor.transform.position = new Vector3(Random.Range(-5f, 5f), 5.2f, Random.Range(0f, 17f));
            kor.transform.localScale = Vector3.one * Random.Range(0.06f, 0.16f);
            kor.GetComponent<Renderer>().sharedMaterial = alev;
            kor.AddComponent<AlevDil>().kor = true;
        }
    }

    IEnumerator AzrailKacirsin()
    {
        if (atici == null)
            yield break;

        GameObject azrail = AzrailYap();
        Vector3 adam = atici.transform.position + new Vector3(0.2f, 0.2f, 0.6f);
        Vector3 bas = adam + new Vector3(0.8f, 1.6f, 1.8f);
        Quaternion basDonus = Quaternion.Euler(12f, 8f, -4f);
        Quaternion yakalama = Quaternion.Euler(4f, 0f, 2f);
        azrail.transform.position = bas;
        azrail.transform.rotation = basDonus;
        azrail.transform.localScale = Vector3.one * 0.55f;

        float sure = 3.6f;
        float gecen = 0f;
        while (gecen < sure)
        {
            gecen += Time.unscaledDeltaTime;
            float oran = Mathf.SmoothStep(0f, 1f, gecen / sure);
            azrail.transform.position = Vector3.Lerp(bas, adam, oran);
            azrail.transform.rotation = Quaternion.Slerp(basDonus, yakalama, oran);
            azrail.transform.localScale = Vector3.one * Mathf.Lerp(0.55f, 0.95f, oran);
            yield return null;
        }

        atici.transform.SetParent(azrail.transform, true);

        Vector3 tutus = azrail.transform.position;
        Vector3 kacis = adam + new Vector3(0.15f, 2.2f, 0.8f);
        Quaternion kacisDonus = Quaternion.Euler(-8f, 12f, 4f);
        sure = 4.2f;
        gecen = 0f;
        while (gecen < sure)
        {
            gecen += Time.unscaledDeltaTime;
            float oran = Mathf.SmoothStep(0f, 1f, gecen / sure);
            azrail.transform.position = Vector3.Lerp(tutus, kacis, oran);
            azrail.transform.rotation = Quaternion.Slerp(yakalama, kacisDonus, oran);
            azrail.transform.localScale = Vector3.one * Mathf.Lerp(0.95f, 1.15f, oran);
            yield return null;
        }
    }

    GameObject AzrailYap()
    {
        GameObject kok = new GameObject("Azrail");
        Material cüppe = Gorsel.Lit(new Color(0.07f, 0.06f, 0.07f), 0.18f, 0f);
        Material astar = Gorsel.Lit(new Color(0.22f, 0.04f, 0.03f), 0.12f, 0f);
        Material kemik = Gorsel.Lit(new Color(0.84f, 0.79f, 0.68f), 0.28f, 0.04f);
        Material bosluk = Gorsel.Lit(new Color(0.02f, 0.01f, 0.01f), 0.05f, 0f);
        Material kanat = Gorsel.Lit(new Color(0.1f, 0.09f, 0.1f), 0.22f, 0f);
        Material metal = Gorsel.Lit(new Color(0.55f, 0.52f, 0.48f), 0.55f, 0.7f);

        IblisParca(PrimitiveType.Capsule, kok.transform, new Vector3(0f, 1.15f, 0f), new Vector3(0.72f, 0.95f, 0.48f), cüppe);
        IblisParca(PrimitiveType.Cube, kok.transform, new Vector3(0f, 0.28f, 0.02f), new Vector3(1.35f, 0.55f, 0.85f), cüppe);
        IblisParca(PrimitiveType.Cube, kok.transform, new Vector3(0f, 0.55f, -0.22f), new Vector3(0.7f, 0.9f, 0.08f), astar);

        Transform kafa = IblisParca(PrimitiveType.Sphere, kok.transform, new Vector3(0f, 2.15f, 0.02f), new Vector3(0.62f, 0.7f, 0.58f), cüppe);
        IblisParca(PrimitiveType.Sphere, kafa, new Vector3(0f, -0.08f, -0.28f), new Vector3(0.42f, 0.5f, 0.36f), kemik);
        IblisParca(PrimitiveType.Sphere, kafa, new Vector3(-0.08f, 0.02f, -0.42f), new Vector3(0.09f, 0.12f, 0.06f), bosluk);
        IblisParca(PrimitiveType.Sphere, kafa, new Vector3(0.08f, 0.02f, -0.42f), new Vector3(0.09f, 0.12f, 0.06f), bosluk);
        IblisParca(PrimitiveType.Cube, kafa, new Vector3(0f, -0.1f, -0.4f), new Vector3(0.05f, 0.08f, 0.04f), bosluk);
        IblisParca(PrimitiveType.Cube, kafa, new Vector3(0f, -0.2f, -0.38f), new Vector3(0.16f, 0.03f, 0.03f), bosluk);

        IblisParca(PrimitiveType.Capsule, kok.transform, new Vector3(-0.42f, 1.35f, -0.15f), new Vector3(0.14f, 0.55f, 0.14f), cüppe)
            .localRotation = Quaternion.Euler(55f, 20f, 18f);
        IblisParca(PrimitiveType.Capsule, kok.transform, new Vector3(0.48f, 1.45f, -0.05f), new Vector3(0.14f, 0.48f, 0.14f), cüppe)
            .localRotation = Quaternion.Euler(70f, -30f, -20f);

        KanatTak(kok.transform, kanat, kemik, 1f);
        KanatTak(kok.transform, kanat, kemik, -1f);

        Transform tirpan = IblisParca(PrimitiveType.Capsule, kok.transform, new Vector3(0.85f, 1.7f, -0.1f), new Vector3(0.06f, 1.15f, 0.06f), metal);
        tirpan.localRotation = Quaternion.Euler(0f, 0f, 18f);
        IblisParca(PrimitiveType.Cube, tirpan, new Vector3(0.35f, 0.85f, 0f), new Vector3(0.7f, 0.08f, 0.04f), metal);
        IblisParca(PrimitiveType.Cube, tirpan, new Vector3(0.62f, 0.62f, 0f), new Vector3(0.08f, 0.42f, 0.04f), metal);
        return kok;
    }

    void KanatTak(Transform kok, Material kanat, Material kemik, float yon)
    {
        for (int i = 0; i < 5; i++)
        {
            float aci = 18f + i * 16f;
            float boy = 0.55f + i * 0.12f;
            if (i == 4)
                boy = 0.7f;
            Transform tuy = IblisParca(PrimitiveType.Cube, kok, new Vector3(yon * (0.55f + i * 0.28f), 1.85f + i * 0.08f, 0.12f), new Vector3(boy, 0.08f, 0.22f), kanat);
            tuy.localRotation = Quaternion.Euler(8f, 0f, yon * aci);
            IblisParca(PrimitiveType.Capsule, tuy, new Vector3(yon * 0.15f, 0f, 0f), new Vector3(0.04f, boy * 0.45f, 0.04f), kemik);
        }
    }

    Transform IblisParca(PrimitiveType tip, Transform ebeveyn, Vector3 yer, Vector3 olcek, Material malzeme)
    {
        GameObject parca = GameObject.CreatePrimitive(tip);
        parca.name = "IblisParca";
        Collider carpma = parca.GetComponent<Collider>();
        if (carpma != null)
        {
            carpma.enabled = false;
            Destroy(carpma);
        }

        parca.transform.SetParent(ebeveyn, false);
        parca.transform.localPosition = yer;
        parca.transform.localScale = olcek;
        Renderer gorunum = parca.GetComponent<Renderer>();
        gorunum.sharedMaterial = malzeme;
        gorunum.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return parca.transform;
    }

    IEnumerator KisaBekleVeYenile()
    {
        yield return new WaitForSecondsRealtime(14f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Telefon ekranındaki dokunmayı 3D masanın üzerindeki bir noktaya çeviren matematiksel formül
    Vector3 EkraniDunyayaCevir(Vector2 ekranKonumu)
    {
        if (Camera.main == null)
            return transform.position;

        Plane masaHizasi = new Plane(Vector3.up, transform.position);
        Ray isin = Camera.main.ScreenPointToRay(ekranKonumu);
        float mesafe;
        
        if (masaHizasi.Raycast(isin, out mesafe))
        {
            return isin.GetPoint(mesafe);
        }
        return transform.position;
    }

    bool HedefeTiklandi(Vector2 ekran)
    {
        if (Camera.main == null)
            return false;

        Ray isin = Camera.main.ScreenPointToRay(ekran);
        if (!Physics.Raycast(isin, out RaycastHit temas, 80f))
            return false;

        return temas.collider.GetComponentInParent<HedefDavranis>() != null;
    }

    // Çekiş uzadıkça noktaların arası açılır
    void NisanNoktalariniGuncelle()
    {
        float cekis = firlatmaYonu.magnitude;
        if (cekis < 0.08f)
        {
            NisanNoktalariniGizle();
            return;
        }

        // Nokta çapı kapağa yakın. İlki kapağın hemen önünden başlar, çekiş arttıkça aralık açılır.
        float aralik = 0.15f + cekis * 0.22f;
        float ilkMesafe = 0.6f;
        Vector3 ileri = firlatmaYonu.normalized;
        Vector3 bas = transform.position;
        bas.y += 0.08f;

        for (int i = 0; i < NoktaSayisi; i++)
        {
            nisanNoktalari[i].gameObject.SetActive(true);
            nisanNoktalari[i].position = bas + ileri * (ilkMesafe + aralik * i);
        }
    }

    void NisanNoktalariniGizle()
    {
        if (nisanNoktalari == null)
            return;

        for (int i = 0; i < nisanNoktalari.Length; i++)
        {
            if (nisanNoktalari[i] != null)
                nisanNoktalari[i].gameObject.SetActive(false);
        }
    }
}

public class AlevDil : MonoBehaviour
{
    public float kalinlik = 0.22f;
    public float yukseklik = 1.1f;
    public bool kor;
    Vector3 dip;
    float faz;

    void Start()
    {
        dip = transform.position;
        faz = Random.Range(0f, 6f);
    }

    void Update()
    {
        if (kor)
        {
            transform.position += Vector3.up * Time.unscaledDeltaTime * (1.1f + faz * 0.25f);
            transform.localScale *= 1f - Time.unscaledDeltaTime * 0.35f;
            if (transform.localScale.x < 0.02f)
                Destroy(gameObject);
            return;
        }

        float t = Time.unscaledTime * 11f + faz;
        float boy = yukseklik * (0.45f + Mathf.Abs(Mathf.Sin(t)) * 0.7f);
        transform.position = dip + Vector3.up * boy * 0.45f;
        transform.localScale = new Vector3(kalinlik, boy, kalinlik);
    }
}

public class Lav : MonoBehaviour
{
    Vector3 basKonum;
    Vector3 basOlcek;
    float baslangic;

    void Start()
    {
        basKonum = transform.position;
        basOlcek = transform.localScale;
        baslangic = Time.unscaledTime;
    }

    void Update()
    {
        float gecen = Time.unscaledTime - baslangic;
        float yuksel = Mathf.Clamp01(gecen / 8.5f);
        float kabarma = 1f + Mathf.Sin(Time.unscaledTime * 6f + baslangic) * 0.08f;
        transform.position = basKonum + Vector3.up * (yuksel * 0.85f);
        transform.localScale = new Vector3(basOlcek.x * (1f + yuksel * 1.6f) * kabarma, basOlcek.y + yuksel * 0.7f, basOlcek.z * (1f + yuksel * 0.8f));
    }
}

public class Yikinti : MonoBehaviour
{
    Vector3 basKonum;
    Quaternion basDonus;
    Vector3 hedef;
    Vector3 eksen;
    float bekle;
    float sure;
    float aci;
    float baslangic;

    public void Kur(float bekle)
    {
        basKonum = transform.position;
        basDonus = transform.rotation;
        this.bekle = bekle;
        hedef = basKonum + new Vector3(Random.Range(-1.1f, 1.1f), Random.Range(-2.4f, -0.5f), Random.Range(-1.1f, 1.1f));
        eksen = Random.onUnitSphere;
        sure = Random.Range(2.2f, 4.2f);
        aci = Random.Range(40f, 130f);
        baslangic = Time.unscaledTime;
    }

    void Update()
    {
        float gecen = Time.unscaledTime - baslangic;
        if (gecen < bekle)
        {
            float sars = Mathf.Sin(gecen * 46f) * 0.06f;
            transform.position = basKonum + new Vector3(sars, Mathf.Abs(sars) * 0.3f, sars * 0.7f);
            return;
        }

        float oran = Mathf.SmoothStep(0f, 1f, (gecen - bekle) / sure);
        transform.position = Vector3.Lerp(basKonum, hedef, oran);
        transform.rotation = basDonus * Quaternion.AngleAxis(aci * oran, eksen);
    }
}