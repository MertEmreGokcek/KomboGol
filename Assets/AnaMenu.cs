using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum OyunBolumu
{
    Salon,
    Test,
    Rastgele
}

// Oyun açılınca bölümün bulanık resmi üstüne menüyü koyar.
public class AnaMenu : MonoBehaviour
{
    const string SesAnahtar = "KomboGOL_Ses";
    static bool direktOyna;
    static OyunBolumu bolum;

    public static OyunBolumu Bolum()
    {
        return bolum;
    }

    public static void BolumSec(OyunBolumu deger)
    {
        bolum = deger;
    }
    Text sesYazi;
    float ses = 1f;
    GameObject okKanvas;

    public static GameObject Goster(KapakFirlatma oyun)
    {
        GameObject kok = new GameObject("AnaMenu");
        AnaMenu menu = kok.AddComponent<AnaMenu>();
        menu.Kur(oyun);
        return kok;
    }

    void Kur(KapakFirlatma oyun)
    {
        ses = PlayerPrefs.GetFloat(SesAnahtar, 1f);
        AudioListener.volume = ses;
        OlaySistemi();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Sprite kutu = YuvarlakKutu();

        Canvas tuval = gameObject.AddComponent<Canvas>();
        tuval.renderMode = RenderMode.ScreenSpaceOverlay;
        tuval.sortingOrder = 50;
        CanvasScaler olcek = gameObject.AddComponent<CanvasScaler>();
        olcek.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        olcek.referenceResolution = new Vector2(1080f, 1920f);
        olcek.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        RawImage zemin = Yeni<RawImage>("BolumResmi", transform);
        TumEkran(zemin.rectTransform);
        zemin.texture = BulanikBolum();
        zemin.color = Color.white;

        Image perde = Yeni<Image>("Perde", transform);
        perde.sprite = DoluKare();
        perde.type = Image.Type.Simple;
        perde.raycastTarget = false;
        perde.color = new Color(0.05f, 0.03f, 0.02f, 0.42f);
        TumEkran(perde.rectTransform);

        Color krem = new Color(0.97f, 0.94f, 0.88f);
        Color altin = new Color(0.86f, 0.68f, 0.36f);
        Color mürekkep = new Color(0.16f, 0.11f, 0.08f);

        Yazi("KomboGOL", transform, font, 92, new Color(0.08f, 0.05f, 0.03f, 0.45f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(4f, -158f), new Vector2(980f, 140f));
        Yazi("KomboGOL", transform, font, 92, krem,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(980f, 140f));

        Image cizgi = Yeni<Image>("Cizgi", transform);
        cizgi.sprite = DoluKare();
        cizgi.color = altin;
        cizgi.raycastTarget = false;
        RectTransform cizgiDik = cizgi.rectTransform;
        cizgiDik.anchorMin = new Vector2(0.5f, 1f);
        cizgiDik.anchorMax = new Vector2(0.5f, 1f);
        cizgiDik.pivot = new Vector2(0.5f, 0.5f);
        cizgiDik.anchoredPosition = new Vector2(0f, -248f);
        cizgiDik.sizeDelta = new Vector2(180f, 6f);

        Yazi("bölüm seç", transform, font, 36, altin,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -310f), new Vector2(500f, 70f));

        Button bolum = Dugme("Bolum2", transform, kutu, font, "Bölüm 2", 44,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -420f), new Vector2(560f, 108f),
            mürekkep, krem);
        bolum.onClick.AddListener(() =>
        {
            Ses.Tik();
            BolumSec(OyunBolumu.Salon);
            oyun.BolumuAc();
        });

        Button rastgele = Dugme("RastgeleBolum", transform, kutu, font, "Rastgele bölüm", 40,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -548f), new Vector2(560f, 108f),
            krem,
            new Color(0.33f, 0.24f, 0.14f, 0.94f));
        rastgele.onClick.AddListener(() =>
        {
            Ses.Tik();
            oyun.RastgeleGec();
        });

        Button test = Dugme("TestBolumu", transform, kutu, font, "Test bölümü", 40,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -676f), new Vector2(560f, 108f),
            krem,
            new Color(0.24f, 0.17f, 0.12f, 0.94f));
        test.onClick.AddListener(() =>
        {
            Ses.Tik();
            oyun.TesteGec();
        });

        SesPaneli(font, kutu);
        YenidenDeneButonu();
    }

    public void OkuGoster()
    {
        if (okKanvas != null)
            okKanvas.SetActive(true);
    }

    public static void DusmeEkrani()
    {
        if (GameObject.Find("DusmeEkrani") != null)
            return;

        OlaySistemi();
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Sprite kutu = YuvarlakKutu();
        Color krem = new Color(0.97f, 0.94f, 0.88f);
        Color mürekkep = new Color(0.16f, 0.11f, 0.08f);

        GameObject kok = new GameObject("DusmeEkrani");
        Canvas tuval = kok.AddComponent<Canvas>();
        tuval.renderMode = RenderMode.ScreenSpaceOverlay;
        tuval.sortingOrder = 80;
        CanvasScaler olcek = kok.AddComponent<CanvasScaler>();
        olcek.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        olcek.referenceResolution = new Vector2(1080f, 1920f);
        olcek.matchWidthOrHeight = 0.5f;
        kok.AddComponent<GraphicRaycaster>();

        Image perde = Yeni<Image>("Perde", kok.transform);
        perde.sprite = DoluKare();
        perde.color = new Color(0.04f, 0.03f, 0.02f, 0.78f);
        TumEkran(perde.rectTransform);

        Yazi("Obruğa düştün", kok.transform, font, 64, krem,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 160f), new Vector2(900f, 120f));

        Button yeniden = Dugme("YenidenBasla", kok.transform, kutu, font, "Yeniden başla", 42,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(560f, 120f),
            mürekkep, krem);
        yeniden.onClick.AddListener(SahneyiYenile);
    }

    public static void SahneyiYenile()
    {
        Time.timeScale = 1f;
        direktOyna = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static bool DirektOynaIste()
    {
        if (!direktOyna)
            return false;

        direktOyna = false;
        return true;
    }

    void YenidenDeneButonu()
    {
        GameObject ust = new GameObject("YenidenDene");
        okKanvas = ust;
        Canvas tuval = ust.AddComponent<Canvas>();
        tuval.renderMode = RenderMode.ScreenSpaceOverlay;
        tuval.sortingOrder = 60;
        CanvasScaler olcek = ust.AddComponent<CanvasScaler>();
        olcek.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        olcek.referenceResolution = new Vector2(1080f, 1920f);
        olcek.matchWidthOrHeight = 0.5f;
        ust.AddComponent<GraphicRaycaster>();

        Image daire = Yeni<Image>("OkDugme", ust.transform);
        daire.sprite = Cember();
        daire.color = new Color(0.12f, 0.09f, 0.07f, 0.9f);
        RectTransform dik = daire.rectTransform;
        dik.anchorMin = new Vector2(0f, 1f);
        dik.anchorMax = new Vector2(0f, 1f);
        dik.pivot = new Vector2(0.5f, 0.5f);
        dik.anchoredPosition = new Vector2(96f, -96f);
        dik.sizeDelta = new Vector2(112f, 112f);

        Button dugme = daire.gameObject.AddComponent<Button>();
        dugme.targetGraphic = daire;
        ColorBlock renkler = dugme.colors;
        renkler.normalColor = daire.color;
        renkler.highlightedColor = new Color(0.22f, 0.16f, 0.12f, 0.95f);
        renkler.pressedColor = new Color(0.08f, 0.06f, 0.05f, 0.95f);
        renkler.selectedColor = daire.color;
        renkler.fadeDuration = 0.06f;
        dugme.colors = renkler;
        dugme.onClick.AddListener(YenidenDene);

        Image ok = Yeni<Image>("Ok", daire.transform);
        ok.sprite = OkSprite();
        ok.color = new Color(0.97f, 0.94f, 0.88f);
        ok.raycastTarget = false;
        RectTransform okDik = ok.rectTransform;
        okDik.anchorMin = new Vector2(0.2f, 0.2f);
        okDik.anchorMax = new Vector2(0.8f, 0.8f);
        okDik.offsetMin = Vector2.zero;
        okDik.offsetMax = Vector2.zero;
        ok.gameObject.AddComponent<DonenOk>();

        ust.SetActive(false);
    }

    void YenidenDene()
    {
        Ses.Tik();
        SahneyiYenile();
    }

    void SesPaneli(Font font, Sprite kutu)
    {
        RectTransform panel = Yeni<RectTransform>("SesPaneli", transform);
        panel.anchorMin = new Vector2(1f, 0f);
        panel.anchorMax = new Vector2(1f, 0f);
        panel.pivot = new Vector2(1f, 0f);
        panel.anchoredPosition = new Vector2(-48f, 56f);
        panel.sizeDelta = new Vector2(460f, 120f);

        Yazi("Ses", panel, font, 36, new Color(0.9f, 0.86f, 0.78f),
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(70f, 0f), new Vector2(120f, 80f));

        Button azalt = Dugme("SesAzalt", panel, kutu, font, "−", 54,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(190f, 0f), new Vector2(90f, 90f),
            new Color(0.96f, 0.93f, 0.86f),
            new Color(0.2f, 0.16f, 0.13f, 0.95f));
        azalt.onClick.AddListener(() => SesiDegistir(-0.1f));

        sesYazi = Yazi("100", panel, font, 34, new Color(0.96f, 0.93f, 0.86f),
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(300f, 0f), new Vector2(90f, 80f));

        Button arttir = Dugme("SesArttir", panel, kutu, font, "+", 54,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(400f, 0f), new Vector2(90f, 90f),
            new Color(0.96f, 0.93f, 0.86f),
            new Color(0.2f, 0.16f, 0.13f, 0.95f));
        arttir.onClick.AddListener(() => SesiDegistir(0.1f));
        SesYazisiniGuncelle();
    }

    void SesiDegistir(float fark)
    {
        Ses.Tik();
        ses = Mathf.Clamp01(ses + fark);
        AudioListener.volume = ses;
        PlayerPrefs.SetFloat(SesAnahtar, ses);
        SesYazisiniGuncelle();
    }

    void SesYazisiniGuncelle()
    {
        if (sesYazi != null)
            sesYazi.text = Mathf.RoundToInt(ses * 100f).ToString();
    }

    static void OlaySistemi()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
            return;

        GameObject sistem = new GameObject("EventSystem");
        sistem.AddComponent<EventSystem>();
        sistem.AddComponent<InputSystemUIInputModule>();
    }

    static Texture2D BulanikBolum()
    {
        Camera kamera = Camera.main;
        if (kamera == null)
            return Duz(new Color(0.05f, 0.08f, 0.06f));

        int pikselEn = kamera.pixelWidth > 16 ? kamera.pixelWidth : Screen.width;
        int pikselBoy = kamera.pixelHeight > 16 ? kamera.pixelHeight : Screen.height;
        if (pikselEn < 16 || pikselBoy < 16)
        {
            pikselEn = 1080;
            pikselBoy = 1920;
        }

        int en = 420;
        int boy = Mathf.Max(1, Mathf.RoundToInt(en * (pikselBoy / (float)pikselEn)));
        RenderTexture rt = new RenderTexture(en, boy, 24);
        rt.Create();

        RenderPipeline.StandardRequest istek = new RenderPipeline.StandardRequest();
        if (RenderPipeline.SupportsRenderRequest(kamera, istek))
        {
            istek.destination = rt;
            RenderPipeline.SubmitRenderRequest(kamera, istek);
        }
        else
        {
            RenderTexture eski = kamera.targetTexture;
            kamera.targetTexture = rt;
            kamera.Render();
            kamera.targetTexture = eski;
        }

        RenderTexture onceki = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D doku = new Texture2D(en, boy, TextureFormat.RGB24, false);
        doku.ReadPixels(new Rect(0f, 0f, en, boy), 0, 0);
        doku.Apply();
        RenderTexture.active = onceki;
        rt.Release();
        Destroy(rt);

        return Bulaniklastir(doku);
    }

    static Texture2D Bulaniklastir(Texture2D kaynak)
    {
        int en = kaynak.width;
        int boy = kaynak.height;
        Color[] pikseller = kaynak.GetPixels();
        pikseller = Kutu(pikseller, en, boy, 4, true);
        pikseller = Kutu(pikseller, en, boy, 4, false);
        pikseller = Kutu(pikseller, en, boy, 2, true);
        pikseller = Kutu(pikseller, en, boy, 2, false);
        kaynak.SetPixels(pikseller);
        kaynak.Apply();
        kaynak.filterMode = FilterMode.Bilinear;
        return kaynak;
    }

    static Color[] Kutu(Color[] kaynak, int en, int boy, int yaricap, bool yatay)
    {
        Color[] hedef = new Color[kaynak.Length];
        float bolen = yaricap * 2f + 1f;
        for (int y = 0; y < boy; y++)
        {
            for (int x = 0; x < en; x++)
            {
                Color toplam = Color.black;
                for (int k = -yaricap; k <= yaricap; k++)
                {
                    int xx = yatay ? Mathf.Clamp(x + k, 0, en - 1) : x;
                    int yy = yatay ? y : Mathf.Clamp(y + k, 0, boy - 1);
                    toplam += kaynak[yy * en + xx];
                }

                hedef[y * en + x] = toplam / bolen;
            }
        }

        return hedef;
    }

    static Texture2D Duz(Color renk)
    {
        Texture2D doku = new Texture2D(4, 4, TextureFormat.RGB24, false);
        Color[] pikseller = new Color[16];
        for (int i = 0; i < pikseller.Length; i++)
            pikseller[i] = renk;
        doku.SetPixels(pikseller);
        doku.Apply();
        return doku;
    }

    static Text Yazi(string metin, Transform ebeveyn, Font font, int boyut, Color renk,
        Vector2 koseMin, Vector2 koseMax, Vector2 yer, Vector2 olcu)
    {
        Text yazi = Yeni<Text>(metin, ebeveyn);
        yazi.font = font;
        yazi.text = metin;
        yazi.fontSize = boyut;
        yazi.alignment = TextAnchor.MiddleCenter;
        yazi.color = renk;
        yazi.horizontalOverflow = HorizontalWrapMode.Overflow;
        yazi.verticalOverflow = VerticalWrapMode.Overflow;
        yazi.raycastTarget = false;
        RectTransform dik = yazi.rectTransform;
        dik.anchorMin = koseMin;
        dik.anchorMax = koseMax;
        dik.pivot = new Vector2(0.5f, 0.5f);
        dik.anchoredPosition = yer;
        dik.sizeDelta = olcu;
        return yazi;
    }

    static Button Dugme(string ad, Transform ebeveyn, Sprite sprite, Font font, string metin, int yaziBoyutu,
        Vector2 koseMin, Vector2 koseMax, Vector2 yer, Vector2 olcu, Color yaziRengi, Color zeminRengi)
    {
        Image resim = Yeni<Image>(ad, ebeveyn);
        resim.sprite = sprite;
        resim.type = Image.Type.Sliced;
        resim.color = Color.white;
        RectTransform dik = resim.rectTransform;
        dik.anchorMin = koseMin;
        dik.anchorMax = koseMax;
        dik.pivot = new Vector2(0.5f, 0.5f);
        dik.anchoredPosition = yer;
        dik.sizeDelta = olcu;

        Button dugme = resim.gameObject.AddComponent<Button>();
        dugme.targetGraphic = resim;
        ColorBlock renkler = dugme.colors;
        renkler.normalColor = zeminRengi;
        renkler.highlightedColor = zeminRengi * 1.25f;
        renkler.pressedColor = zeminRengi * 0.75f;
        renkler.selectedColor = zeminRengi;
        renkler.fadeDuration = 0.06f;
        dugme.colors = renkler;

        Text yazi = Yazi(metin, resim.transform, font, yaziBoyutu, yaziRengi,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        yazi.rectTransform.offsetMin = Vector2.zero;
        yazi.rectTransform.offsetMax = Vector2.zero;
        return dugme;
    }

    static void TumEkran(RectTransform dik)
    {
        dik.anchorMin = Vector2.zero;
        dik.anchorMax = Vector2.one;
        dik.offsetMin = Vector2.zero;
        dik.offsetMax = Vector2.zero;
    }

    static T Yeni<T>(string ad, Transform ebeveyn) where T : Component
    {
        GameObject obje = new GameObject(ad, typeof(RectTransform));
        obje.transform.SetParent(ebeveyn, false);
        if (typeof(T) == typeof(RectTransform))
            return obje.GetComponent<T>();
        return obje.AddComponent<T>();
    }

    static Sprite DoluKare()
    {
        Texture2D doku = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        Color[] pikseller = new Color[16];
        for (int i = 0; i < pikseller.Length; i++)
            pikseller[i] = Color.white;
        doku.SetPixels(pikseller);
        doku.Apply();
        return Sprite.Create(doku, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f));
    }

    static Sprite YuvarlakKutu()
    {
        const int n = 64;
        const int yaricap = 22;
        Texture2D doku = new Texture2D(n, n, TextureFormat.RGBA32, false);
        Color[] pikseller = new Color[n * n];
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float dx = 0f;
                float dy = 0f;
                if (x < yaricap)
                    dx = yaricap - x;
                else if (x >= n - yaricap)
                    dx = x - (n - yaricap - 1);
                if (y < yaricap)
                    dy = yaricap - y;
                else if (y >= n - yaricap)
                    dy = y - (n - yaricap - 1);

                bool icerde = dx * dx + dy * dy <= yaricap * yaricap;
                pikseller[y * n + x] = icerde ? Color.white : Color.clear;
            }
        }

        doku.SetPixels(pikseller);
        doku.Apply();
        doku.wrapMode = TextureWrapMode.Clamp;
        return Sprite.Create(doku, new Rect(0f, 0f, n, n), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
            new Vector4(yaricap, yaricap, yaricap, yaricap));
    }

    static Sprite Cember()
    {
        const int n = 64;
        Texture2D doku = new Texture2D(n, n, TextureFormat.RGBA32, false);
        Color[] pikseller = new Color[n * n];
        float c = (n - 1) * 0.5f;
        float r = c - 0.5f;
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float dx = x - c;
                float dy = y - c;
                pikseller[y * n + x] = dx * dx + dy * dy <= r * r ? Color.white : Color.clear;
            }
        }

        doku.SetPixels(pikseller);
        doku.Apply();
        return Sprite.Create(doku, new Rect(0f, 0f, n, n), new Vector2(0.5f, 0.5f));
    }

    static Sprite OkSprite()
    {
        const int n = 128;
        Texture2D doku = new Texture2D(n, n, TextureFormat.RGBA32, false);
        Color[] pikseller = new Color[n * n];
        float c = (n - 1) * 0.5f;
        float bas = 55f * Mathf.Deg2Rad;
        float bit = 305f * Mathf.Deg2Rad;
        Vector2 uc = OkNokta(c, 28f, 62f);
        Vector2 sol = OkNokta(c, 58f, 34f);
        Vector2 sag = OkNokta(c, 2f, 36f);

        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float dx = x - c;
                float dy = y - c;
                float uzak = Mathf.Sqrt(dx * dx + dy * dy);
                float aci = Mathf.Atan2(dy, dx);
                if (aci < 0f)
                    aci += Mathf.PI * 2f;

                bool yay = uzak >= 30f && uzak <= 46f && aci >= bas && aci <= bit;
                bool kafa = Ucgen(new Vector2(x, y), uc, sol, sag);
                pikseller[y * n + x] = yay || kafa ? Color.white : Color.clear;
            }
        }

        doku.SetPixels(pikseller);
        doku.Apply();
        doku.filterMode = FilterMode.Bilinear;
        return Sprite.Create(doku, new Rect(0f, 0f, n, n), new Vector2(0.5f, 0.5f));
    }

    static Vector2 OkNokta(float c, float derece, float uzak)
    {
        float aci = derece * Mathf.Deg2Rad;
        return new Vector2(c + Mathf.Cos(aci) * uzak, c + Mathf.Sin(aci) * uzak);
    }

    static bool Ucgen(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float s1 = (p.x - c.x) * (a.y - c.y) - (a.x - c.x) * (p.y - c.y);
        float s2 = (p.x - a.x) * (b.y - a.y) - (b.x - a.x) * (p.y - a.y);
        float s3 = (p.x - b.x) * (c.y - b.y) - (c.x - b.x) * (p.y - b.y);
        bool eksi = s1 < 0f || s2 < 0f || s3 < 0f;
        bool arti = s1 > 0f || s2 > 0f || s3 > 0f;
        return !(eksi && arti);
    }
}

public class DonenOk : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0f, 0f, -70f * Time.unscaledDeltaTime);
    }
}
