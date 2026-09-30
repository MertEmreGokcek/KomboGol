using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Masayı, kapağı ve kısa görsel efektleri kurar. Fizik objelerine çarpışma kutusu eklemez.
public static class Gorsel
{
    static Material metal;
    static Material baski;
    static Material kece;

    static void CarpismayiSil(GameObject obje)
    {
        Collider carpma = obje.GetComponent<Collider>();
        if (carpma == null)
            return;

        carpma.enabled = false;
        Object.Destroy(carpma);
    }

    public static void SahneyiKur()
    {
        KameraVeIsik();
        KenneyOda();
    }

    static void KenneyOda()
    {
        if (GameObject.Find("KenneyOda") != null)
            return;

        GameObject deneme = Model("floorFull");
        if (deneme == null)
        {
            KeceyiBoy();
            return;
        }

        Bounds karoSinir = Sinir(deneme);
        Object.Destroy(deneme);
        if (karoSinir.size.x < 0.0001f)
        {
            KeceyiBoy();
            return;
        }

        EskiOrtamiGizle();

        // Kenney karosu 10 birim. Oyun masası 7.2 x 18, yani 6 x 15 karo.
        const float karo = 1.2f;
        float olcek = karo / karoSinir.size.x;
        float taban = 5f - karoSinir.size.y * olcek;

        GameObject kok = new GameObject("KenneyOda");

        for (int ix = 0; ix < 12; ix++)
        {
            float x = -6f + ix * karo;
            for (int iz = 0; iz < 19; iz++)
                Koy(kok.transform, "floorFull", new Vector3(x, taban, -2.4f + iz * karo), Quaternion.identity, olcek);
        }

        // Topun sektiği kenarlar. Kalınlık dışarı bakar, oyun yoluna girmez.
        for (int i = 0; i < 15; i++)
            Koy(kok.transform, "wall", new Vector3(3.6f, 5f, i * karo), Quaternion.Euler(0f, 90f, 0f), olcek);
        for (int i = 1; i <= 15; i++)
            Koy(kok.transform, "wall", new Vector3(-3.6f, 5f, i * karo), Quaternion.Euler(0f, -90f, 0f), olcek);
        for (int i = 0; i < 6; i++)
            Koy(kok.transform, "wall", new Vector3(-2.4f + i * karo, 5f, 18f), Quaternion.identity, olcek);
        for (int i = 0; i < 6; i++)
            Koy(kok.transform, "wall", new Vector3(-3.6f + i * karo, 5f, 0f), Quaternion.Euler(0f, 180f, 0f), olcek);

        // Sınıfın dış duvarı, masadan 3.6 m uzakta. Arada eşya durur.
        for (int i = 0; i < 19; i++)
        {
            float z = -1.2f + i * karo;
            string sol = i % 4 == 1 ? "wallWindow" : "wall";
            Koy(kok.transform, sol, new Vector3(-7.2f, 5f, z), Quaternion.Euler(0f, -90f, 0f), olcek);
        }
        for (int i = 0; i < 19; i++)
        {
            float z = -2.4f + i * karo;
            string sag = i % 4 == 2 ? "wallWindow" : "wall";
            Koy(kok.transform, sag, new Vector3(7.2f, 5f, z), Quaternion.Euler(0f, 90f, 0f), olcek);
        }
        for (int i = 0; i < 12; i++)
            Koy(kok.transform, "wall", new Vector3(-6f + i * karo, 5f, 20.4f), Quaternion.identity, olcek);

        // Sol salon: halı, koltuk, sehpa. Hepsi sekme duvarının dışında.
        Koy(kok.transform, "rugRectangle", new Vector3(-5.35f, 5.02f, 1.15f), Quaternion.Euler(0f, 90f, 0f), olcek);
        Koy(kok.transform, "loungeSofa", new Vector3(-5.55f, 5f, 1.45f), Quaternion.Euler(0f, 90f, 0f), olcek);
        Koy(kok.transform, "tableCoffee", new Vector3(-4.55f, 5f, 1.85f), Quaternion.identity, olcek);
        Koy(kok.transform, "lampRoundFloor", new Vector3(-6.55f, 5f, 1.3f), Quaternion.identity, olcek);
        Koy(kok.transform, "bookcaseClosed", new Vector3(-6.55f, 5f, 6f), Quaternion.Euler(0f, 90f, 0f), olcek);
        Koy(kok.transform, "cabinetTelevision", new Vector3(-4.2f, 5f, 8.2f), Quaternion.identity, olcek);
        Koy(kok.transform, "speaker", new Vector3(-5.45f, 5f, 8.15f), Quaternion.identity, olcek);
        Koy(kok.transform, "pottedPlant", new Vector3(-5f, 5f, 12.2f), Quaternion.identity, olcek);
        Koy(kok.transform, "loungeChair", new Vector3(-4.3f, 5f, 13.5f), Quaternion.identity, olcek);

        // Sağ salon: koltuk, sehpa, kitaplık, çalışma masası.
        Koy(kok.transform, "coatRackStanding", new Vector3(5.8f, 5f, 1.15f), Quaternion.identity, olcek);
        Koy(kok.transform, "loungeChair", new Vector3(5.3f, 5f, 3.2f), Quaternion.Euler(0f, -90f, 0f), olcek);
        Koy(kok.transform, "sideTable", new Vector3(4.3f, 5f, 5.1f), Quaternion.Euler(0f, 180f, 0f), olcek);
        Koy(kok.transform, "bookcaseClosed", new Vector3(6.7f, 5f, 8.5f), Quaternion.Euler(0f, -90f, 0f), olcek);
        Koy(kok.transform, "desk", new Vector3(5.2f, 5f, 11f), Quaternion.identity, olcek);
        Koy(kok.transform, "chair", new Vector3(5.85f, 5f, 11.7f), Quaternion.Euler(0f, 180f, 0f), olcek);
        Koy(kok.transform, "lampRoundFloor", new Vector3(6.4f, 5f, 14.6f), Quaternion.identity, olcek);

        // Arka duvarın hemen gerisi ve giriş paspası. Oyun yoluna girmez.
        Koy(kok.transform, "bookcaseClosed", new Vector3(-0.3f, 5f, 18.55f), Quaternion.identity, olcek);
        Koy(kok.transform, "bookcaseClosed", new Vector3(1.3f, 5f, 18.55f), Quaternion.identity, olcek);
        Koy(kok.transform, "rugDoormat", new Vector3(0.3f, 5.02f, -1.2f), Quaternion.identity, olcek);

        Koy(kok.transform, "lampSquareCeiling", new Vector3(-5.2f, 7.5f, 4f), Quaternion.identity, olcek);
        Koy(kok.transform, "lampSquareCeiling", new Vector3(5.2f, 7.5f, 10f), Quaternion.identity, olcek);

        Insan(kok.transform, "03", new Vector3(-6.3f, 5f, 3.6f), 90f);
        Insan(kok.transform, "04", new Vector3(-6.25f, 5f, 9.3f), 90f);
        Insan(kok.transform, "05", new Vector3(-6.15f, 5f, 14.4f), 70f);
        Insan(kok.transform, "06", new Vector3(5.7f, 5f, 6.3f), -90f);
        Insan(kok.transform, "07", new Vector3(6.15f, 5f, 13.6f), -90f);
    }

    static void Insan(Transform ebeveyn, string no, Vector3 ayak, float yon)
    {
        GameObject yer = new GameObject("Misafir" + no);
        yer.transform.SetParent(ebeveyn, false);
        yer.transform.position = ayak;
        yer.transform.rotation = Quaternion.Euler(0f, yon, 0f);
        Karakter(yer.transform, no);
    }

    // Mini Simple karakteri ayağın üstüne koyar. Boyu oda duvarının altında kalır.
    public static bool Karakter(Transform ebeveyn, string no)
    {
#if UNITY_EDITOR
        GameObject kaynak = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Mini Simple Characters Demo/Prefabs/mini simple demo_" + no + ".prefab");
        if (kaynak == null)
            return false;

        GameObject kopya = Object.Instantiate(kaynak, ebeveyn);
        kopya.name = "Karakter";
        kopya.transform.localPosition = Vector3.zero;
        kopya.transform.localRotation = Quaternion.identity;

        Renderer[] gorunumler = kopya.GetComponentsInChildren<Renderer>();
        if (gorunumler.Length == 0)
            return true;

        Bounds sinir = gorunumler[0].bounds;
        for (int i = 1; i < gorunumler.Length; i++)
            sinir.Encapsulate(gorunumler[i].bounds);

        float boy = sinir.size.y;
        if (boy > 0.01f)
            kopya.transform.localScale = Vector3.one * (1.35f / boy);

        sinir = gorunumler[0].bounds;
        for (int i = 1; i < gorunumler.Length; i++)
            sinir.Encapsulate(gorunumler[i].bounds);
        Vector3 kaydir = kopya.transform.position;
        kaydir.y += ebeveyn.position.y - sinir.min.y;
        kopya.transform.position = kaydir;

        foreach (Renderer gorunum in gorunumler)
            DokuyuTasi(gorunum);

        foreach (Collider carpma in kopya.GetComponentsInChildren<Collider>())
        {
            carpma.enabled = false;
            Object.Destroy(carpma);
        }

        RuntimeAnimatorController kontrol = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/Mini Simple Characters Demo/Models/Animations/Mini simple Characters Animation Controller Demo.controller");
        Animator anim = kopya.GetComponentInChildren<Animator>();
        if (anim != null && kontrol != null)
        {
            anim.runtimeAnimatorController = kontrol;
            anim.applyRootMotion = false;
            anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        return true;
#else
        return false;
#endif
    }

    static void DokuyuTasi(Renderer gorunum)
    {
        Material[] malzemeler = gorunum.materials;
        bool degisti = false;
        for (int i = 0; i < malzemeler.Length; i++)
        {
            Material malzeme = malzemeler[i];
            if (malzeme == null || malzeme.shader == null)
                continue;
            if (malzeme.shader.name.Contains("Universal"))
                continue;

            Color renk = malzeme.HasProperty("_Color") ? malzeme.color : Color.white;
            Material yeni = Lit(renk, 0.25f, 0f);
            Texture doku = malzeme.HasProperty("_MainTex") ? malzeme.GetTexture("_MainTex") : null;
            if (doku != null && yeni.HasProperty("_BaseMap"))
                yeni.SetTexture("_BaseMap", doku);
            malzemeler[i] = yeni;
            degisti = true;
        }

        if (degisti)
            gorunum.materials = malzemeler;
    }

    static Bounds Sinir(GameObject obje)
    {
        Renderer[] gorunumler = obje.GetComponentsInChildren<Renderer>();
        Bounds sinir = gorunumler[0].bounds;
        for (int i = 1; i < gorunumler.Length; i++)
            sinir.Encapsulate(gorunumler[i].bounds);
        return sinir;
    }

    static void EskiOrtamiGizle()
    {
        GameObject zemin = GameObject.Find("Zemin");
        if (zemin != null)
        {
            Renderer gorunum = zemin.GetComponent<Renderer>();
            if (gorunum != null)
                gorunum.enabled = false;
        }

        Duvar[] duvarlar = Object.FindObjectsByType<Duvar>();
        foreach (Duvar duvar in duvarlar)
        {
            if (duvar.tur != DuvarTuru.Normal)
                continue;

            Renderer gorunum = duvar.GetComponent<Renderer>();
            if (gorunum != null)
                gorunum.enabled = false;
        }

        string[] eskiler = { "Kabin", "SahaCizgileri" };
        foreach (string ad in eskiler)
        {
            GameObject eski = GameObject.Find(ad);
            if (eski != null)
                Object.Destroy(eski);
        }
    }

    static void Koy(Transform ebeveyn, string ad, Vector3 yer, Quaternion donus, float olcek)
    {
        GameObject obje = Model(ad);
        if (obje == null)
            return;

        obje.transform.SetParent(ebeveyn, false);
        obje.transform.position = yer;
        obje.transform.rotation = donus;
        obje.transform.localScale = Vector3.one * olcek;

        foreach (Collider carpma in obje.GetComponentsInChildren<Collider>())
        {
            carpma.enabled = false;
            Object.Destroy(carpma);
        }
    }

    static GameObject Model(string ad)
    {
#if UNITY_EDITOR
        GameObject kaynak = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/KenneyEsyalar/FBX format/" + ad + ".fbx");
        if (kaynak == null)
            return null;

        GameObject kopya = Object.Instantiate(kaynak);
        kopya.name = ad;
        foreach (Renderer gorunum in kopya.GetComponentsInChildren<Renderer>())
        {
            Material[] malzemeler = gorunum.materials;
            bool degisti = false;
            for (int i = 0; i < malzemeler.Length; i++)
            {
                Material malzeme = malzemeler[i];
                if (malzeme == null || malzeme.shader == null)
                    continue;
                if (malzeme.shader.name.Contains("Universal"))
                    continue;

                Color renk = malzeme.HasProperty("_Color") ? malzeme.color : new Color(0.75f, 0.58f, 0.4f);
                malzemeler[i] = Lit(renk, 0.22f, 0f);
                degisti = true;
            }

            if (degisti)
                gorunum.materials = malzemeler;
        }

        return kopya;
#else
        return null;
#endif
    }

    public static TrailRenderer KapagiGiydir(GameObject kapak)
    {
        Renderer govde = kapak.GetComponent<Renderer>();
        if (govde != null)
            govde.enabled = false;

        GameObject kok = new GameObject("SiseKapagi");
        kok.transform.SetParent(kapak.transform, false);
        Vector3 govdeOlcek = kapak.transform.localScale;
        kok.transform.localScale = new Vector3(
            1f / Mathf.Max(0.001f, govdeOlcek.x),
            1f / Mathf.Max(0.001f, govdeOlcek.y),
            1f / Mathf.Max(0.001f, govdeOlcek.z));

        Material teneke = MetalMalzeme();
        Material boya = BaskiMalzeme();

        // Etek: kapağın aşağı sarkan metal kısmı
        Parca(PrimitiveType.Cylinder, "Etek", kok.transform, new Vector3(0f, -0.012f, 0f), new Vector3(0.62f, 0.026f, 0.62f), Quaternion.identity, teneke);
        Parca(PrimitiveType.Cylinder, "Agiz", kok.transform, new Vector3(0f, -0.04f, 0f), new Vector3(0.66f, 0.008f, 0.66f), Quaternion.identity, teneke);
        // Üstteki boyalı tabla, düz disk değil; etekten dar
        Parca(PrimitiveType.Cylinder, "Tabla", kok.transform, new Vector3(0f, 0.02f, 0f), new Vector3(0.5f, 0.012f, 0.5f), Quaternion.identity, boya);
        Parca(PrimitiveType.Sphere, "Kambur", kok.transform, new Vector3(0f, 0.032f, 0f), new Vector3(0.26f, 0.04f, 0.26f), Quaternion.identity, Lit(new Color(0.55f, 0.14f, 0.11f), 0.18f, 0f));

        const int disSayisi = 21;
        for (int i = 0; i < disSayisi; i++)
        {
            float aci = i * 360f / disSayisi;
            Quaternion yon = Quaternion.Euler(0f, aci, 0f);
            Parca(
                PrimitiveType.Cube,
                "Dis",
                kok.transform,
                yon * new Vector3(0f, -0.012f, 0.3f),
                new Vector3(0.052f, 0.068f, 0.026f),
                yon * Quaternion.Euler(-20f, 0f, 0f),
                teneke);
        }

        GameObject tasiyici = new GameObject("Iz");
        tasiyici.transform.SetParent(kapak.transform, false);
        Vector3 olcek = kapak.transform.localScale;
        tasiyici.transform.localScale = new Vector3(
            1f / Mathf.Max(0.001f, olcek.x),
            1f / Mathf.Max(0.001f, olcek.y),
            1f / Mathf.Max(0.001f, olcek.z));

        TrailRenderer iz = tasiyici.AddComponent<TrailRenderer>();
        iz.time = 0.2f;
        iz.startWidth = 0.16f;
        iz.endWidth = 0.01f;
        iz.minVertexDistance = 0.04f;
        iz.numCornerVertices = 4;
        iz.numCapVertices = 2;
        iz.alignment = LineAlignment.View;
        iz.shadowCastingMode = ShadowCastingMode.Off;
        iz.receiveShadows = false;
        iz.emitting = false;
        iz.material = Lit(new Color(0.62f, 0.58f, 0.5f), 0.05f, 0f);

        Gradient gradyan = new Gradient();
        gradyan.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.78f, 0.74f, 0.66f), 0f),
                new GradientColorKey(new Color(0.45f, 0.4f, 0.34f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.35f, 0f),
                new GradientAlphaKey(0f, 1f)
            });
        iz.colorGradient = gradyan;
        return iz;
    }

    static GameObject Parca(PrimitiveType tip, string ad, Transform ebeveyn, Vector3 yer, Vector3 olcek, Quaternion donus, Material malzeme)
    {
        GameObject obje = GameObject.CreatePrimitive(tip);
        obje.name = ad;
        CarpismayiSil(obje);
        obje.transform.SetParent(ebeveyn, false);
        obje.transform.localPosition = yer;
        obje.transform.localRotation = donus;
        obje.transform.localScale = olcek;
        Renderer gorunum = obje.GetComponent<Renderer>();
        gorunum.sharedMaterial = malzeme;
        gorunum.shadowCastingMode = ShadowCastingMode.On;
        gorunum.receiveShadows = true;
        return obje;
    }

    public static void Cercevele(TextMesh yazi, float kalinlik)
    {
        if (yazi == null || yazi.transform.parent == null)
            return;

        Transform ebeveyn = yazi.transform.parent;
        Vector3 olcek = ebeveyn.lossyScale;
        float kx = kalinlik / Mathf.Max(0.001f, olcek.x);
        float ky = kalinlik / Mathf.Max(0.001f, olcek.y);
        Vector3 yer = yazi.transform.localPosition;
        Quaternion donus = yazi.transform.localRotation;
        Vector3 boyut = yazi.transform.localScale;

        Vector2[] yonler =
        {
            new Vector2(1f, 0f), new Vector2(-1f, 0f), new Vector2(0f, 1f), new Vector2(0f, -1f),
            new Vector2(0.7f, 0.7f), new Vector2(0.7f, -0.7f), new Vector2(-0.7f, 0.7f), new Vector2(-0.7f, -0.7f)
        };

        foreach (Vector2 yon in yonler)
        {
            GameObject kopya = Object.Instantiate(yazi.gameObject, ebeveyn);
            kopya.name = "SayiGolge";
            kopya.transform.localPosition = yer + new Vector3(yon.x * kx, yon.y * ky, 0.02f);
            kopya.transform.localRotation = donus;
            kopya.transform.localScale = boyut;
            TextMesh golge = kopya.GetComponent<TextMesh>();
            golge.color = new Color(0.02f, 0.02f, 0.03f, 1f);
        }
    }

    public static void Kivilcim(Vector3 yer, Color renk)
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject parcacik = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            parcacik.name = "Kivilcim";
            CarpismayiSil(parcacik);
            parcacik.transform.position = yer;
            parcacik.transform.localScale = Vector3.one * Random.Range(0.045f, 0.09f);
            Renderer gorunum = parcacik.GetComponent<Renderer>();
            gorunum.sharedMaterial = Lit(renk, 0.1f, 0f);
            gorunum.shadowCastingMode = ShadowCastingMode.Off;
            Vector3 hiz = new Vector3(Random.Range(-1.4f, 1.4f), Random.Range(0.3f, 1.6f), Random.Range(-1.4f, 1.4f));
            parcacik.AddComponent<KisaParlama>().Kur(hiz, 0.14f, 2f);
        }
    }

    public static void Toz(Vector3 yer, Color renk)
    {
        for (int i = 0; i < 8; i++)
        {
            GameObject parcacik = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            parcacik.name = "Toz";
            CarpismayiSil(parcacik);
            parcacik.transform.position = yer + Random.insideUnitSphere * 0.15f;
            parcacik.transform.localScale = Vector3.one * Random.Range(0.04f, 0.09f);
            Renderer gorunum = parcacik.GetComponent<Renderer>();
            gorunum.sharedMaterial = Lit(Color.Lerp(renk, new Color(0.25f, 0.2f, 0.12f), 0.45f), 0.05f, 0f);
            gorunum.shadowCastingMode = ShadowCastingMode.Off;
            Vector3 hiz = new Vector3(Random.Range(-0.8f, 0.8f), Random.Range(0.4f, 1.5f), Random.Range(-0.8f, 0.8f));
            parcacik.AddComponent<KisaParlama>().Kur(hiz, 0.55f, 7f);
        }
    }

    public static Material Lit(Color renk, float puruzsuz, float metalik)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        Material malzeme = shader != null ? new Material(shader) : new Material(Shader.Find("Standard"));
        malzeme.color = renk;
        if (malzeme.HasProperty("_BaseColor"))
            malzeme.SetColor("_BaseColor", renk);
        if (malzeme.HasProperty("_Smoothness"))
            malzeme.SetFloat("_Smoothness", puruzsuz);
        if (malzeme.HasProperty("_Metallic"))
            malzeme.SetFloat("_Metallic", metalik);
        return malzeme;
    }

    static void KameraVeIsik()
    {
        if (Camera.main != null)
        {
            Camera kamera = Camera.main;
            kamera.clearFlags = CameraClearFlags.SolidColor;
            kamera.backgroundColor = new Color(0.78f, 0.73f, 0.62f);
            UniversalAdditionalCameraData ek = kamera.GetUniversalAdditionalCameraData();
            if (ek != null)
                ek.renderPostProcessing = true;
            HacimEkle(kamera.gameObject);
        }

        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.58f, 0.52f, 0.44f);
        RenderSettings.fog = false;

        Light[] isiklar = Object.FindObjectsByType<Light>();
        foreach (Light isik in isiklar)
        {
            if (isik.type != LightType.Directional)
                continue;

            isik.useColorTemperature = false;
            isik.color = new Color(1f, 0.95f, 0.84f);
            isik.intensity = 1.15f;
            isik.shadows = LightShadows.Soft;
            isik.shadowStrength = 0.65f;
            isik.transform.rotation = Quaternion.Euler(54f, -32f, 0f);
        }
    }

    static void HacimEkle(GameObject kamera)
    {
        if (kamera.GetComponent<Volume>() != null)
            return;

        Volume hacim = kamera.AddComponent<Volume>();
        hacim.isGlobal = true;
        hacim.priority = 20f;
        VolumeProfile profil = ScriptableObject.CreateInstance<VolumeProfile>();
        hacim.sharedProfile = profil;

        Vignette kenar = profil.Add<Vignette>();
        kenar.intensity.Override(0.16f);
        kenar.smoothness.Override(0.35f);

        ColorAdjustments renk = profil.Add<ColorAdjustments>();
        renk.postExposure.Override(0.02f);
        renk.contrast.Override(6f);
        renk.saturation.Override(-6f);
        renk.colorFilter.Override(new Color(1f, 0.96f, 0.88f));
    }

    static void KeceyiBoy()
    {
        GameObject zemin = GameObject.Find("Zemin");
        if (zemin == null)
            return;

        Renderer gorunum = zemin.GetComponent<Renderer>();
        if (gorunum == null)
            return;

        if (kece == null)
        {
            kece = Lit(Color.white, 0.14f, 0f);
            kece.SetTexture("_BaseMap", MasaDokusu());
        }

        gorunum.sharedMaterial = kece;
        gorunum.shadowCastingMode = ShadowCastingMode.Off;
        gorunum.receiveShadows = true;
    }

    static void KabinKur()
    {
        if (GameObject.Find("Kabin") != null)
            return;

        GameObject kok = new GameObject("Kabin");
        Sus(kok.transform, "SiraGovde", new Vector3(0f, 3.15f, 9f), new Vector3(8.5f, 3.5f, 19.3f), new Color(0.45f, 0.3f, 0.16f));
        Sus(kok.transform, "SinifTabani", new Vector3(0f, 1.2f, 9f), new Vector3(40f, 0.2f, 40f), new Color(0.62f, 0.5f, 0.34f));
        Sus(kok.transform, "SolDuvar", new Vector3(-7.4f, 6.2f, 9f), new Vector3(0.25f, 10f, 26f), new Color(0.86f, 0.8f, 0.68f));
        Sus(kok.transform, "SagDuvar", new Vector3(7.4f, 6.2f, 9f), new Vector3(0.25f, 10f, 26f), new Color(0.86f, 0.8f, 0.68f));
        Sus(kok.transform, "Pencere", new Vector3(-7.22f, 7.4f, 4.5f), new Vector3(0.08f, 1.7f, 2.4f), new Color(0.72f, 0.82f, 0.86f));

        GameObject cerceve = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cerceve.name = "TahtaCerceve";
        CarpismayiSil(cerceve);
        cerceve.transform.SetParent(kok.transform, false);
        cerceve.transform.position = new Vector3(0f, 7.2f, 18.7f);
        cerceve.transform.localScale = new Vector3(6.6f, 2.45f, 0.18f);
        cerceve.GetComponent<Renderer>().sharedMaterial = Lit(new Color(0.42f, 0.26f, 0.14f), 0.16f, 0f);

        GameObject tahta = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tahta.name = "KaraTahta";
        CarpismayiSil(tahta);
        tahta.transform.SetParent(cerceve.transform, false);
        tahta.transform.localPosition = new Vector3(0f, 0.04f, -0.55f);
        tahta.transform.localScale = new Vector3(0.9f, 0.78f, 0.35f);
        tahta.GetComponent<Renderer>().sharedMaterial = Lit(new Color(0.1f, 0.24f, 0.16f), 0.08f, 0f);

        GameObject silgi = GameObject.CreatePrimitive(PrimitiveType.Cube);
        silgi.name = "Tebeşirlik";
        CarpismayiSil(silgi);
        silgi.transform.SetParent(cerceve.transform, false);
        silgi.transform.localPosition = new Vector3(0f, -0.46f, -0.7f);
        silgi.transform.localScale = new Vector3(0.92f, 0.06f, 0.55f);
        silgi.GetComponent<Renderer>().sharedMaterial = Lit(new Color(0.55f, 0.36f, 0.18f), 0.12f, 0f);
    }

    static void Sus(Transform ebeveyn, string ad, Vector3 yer, Vector3 olcek, Color renk)
    {
        GameObject kutu = GameObject.CreatePrimitive(PrimitiveType.Cube);
        kutu.name = ad;
        CarpismayiSil(kutu);
        kutu.transform.SetParent(ebeveyn, false);
        kutu.transform.position = yer;
        kutu.transform.localScale = olcek;
        Renderer gorunum = kutu.GetComponent<Renderer>();
        gorunum.sharedMaterial = Lit(renk, 0.12f, 0f);
        gorunum.shadowCastingMode = ShadowCastingMode.On;
        gorunum.receiveShadows = true;
    }

    static Material MetalMalzeme()
    {
        if (metal == null)
            metal = Lit(new Color(0.68f, 0.66f, 0.62f), 0.38f, 0.55f);
        return metal;
    }

    static Material BaskiMalzeme()
    {
        if (baski != null)
            return baski;

        baski = Lit(Color.white, 0.22f, 0.05f);
        baski.SetTexture("_BaseMap", KapakDokusu());
        return baski;
    }

    static Texture2D MasaDokusu()
    {
        const int en = 128;
        const int boy = 512;
        Texture2D doku = new Texture2D(en, boy, TextureFormat.RGBA32, false);
        doku.wrapMode = TextureWrapMode.Clamp;
        doku.filterMode = FilterMode.Bilinear;
        Color acik = new Color(0.74f, 0.56f, 0.36f);
        Color orta = new Color(0.62f, 0.44f, 0.26f);
        Color koyu = new Color(0.4f, 0.26f, 0.14f);
        Color[] pikseller = new Color[en * boy];
        for (int y = 0; y < boy; y++)
        {
            int tahta = (y / 64) % 2;
            Color taban = tahta == 0 ? acik : orta;
            bool oluk = y % 64 < 2;
            for (int x = 0; x < en; x++)
            {
                float damar = Mathf.PerlinNoise(x * 0.08f, y * 0.02f);
                Color renk = Color.Lerp(taban, koyu, damar * 0.28f);
                if (oluk)
                    renk = koyu;
                pikseller[y * en + x] = renk;
            }
        }

        doku.SetPixels(pikseller);
        doku.Apply();
        return doku;
    }

    static Texture2D KapakDokusu()
    {
        const int n = 256;
        Texture2D doku = new Texture2D(n, n, TextureFormat.RGBA32, false);
        doku.wrapMode = TextureWrapMode.Clamp;
        doku.filterMode = FilterMode.Bilinear;
        Color[] pikseller = new Color[n * n];
        Color teneke = new Color(0.62f, 0.6f, 0.56f);
        Color krem = new Color(0.9f, 0.86f, 0.78f);
        Color bordo = new Color(0.48f, 0.12f, 0.1f);

        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float dx = (x + 0.5f) / n - 0.5f;
                float dy = (y + 0.5f) / n - 0.5f;
                float r = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                Color renk = teneke;
                if (r > 0.86f)
                    renk = teneke;
                else if (r > 0.72f)
                    renk = krem;
                else if (r > 0.22f)
                    renk = bordo;
                else
                    renk = krem;

                pikseller[y * n + x] = renk;
            }
        }

        doku.SetPixels(pikseller);
        doku.Apply();
        return doku;
    }
}

public class KisaParlama : MonoBehaviour
{
    Vector3 hiz;
    float omur;
    float yercekimi;
    float gecen;
    Vector3 basOlcek;

    public void Kur(Vector3 hiz, float omur, float yercekimi)
    {
        this.hiz = hiz;
        this.omur = omur;
        this.yercekimi = yercekimi;
        basOlcek = transform.localScale;
    }

    void Update()
    {
        gecen += Time.deltaTime;
        hiz += Vector3.down * yercekimi * Time.deltaTime;
        transform.position += hiz * Time.deltaTime;
        float kalan = 1f - Mathf.Clamp01(gecen / omur);
        transform.localScale = basOlcek * Mathf.Max(0.01f, kalan);
        if (gecen >= omur)
            Destroy(gameObject);
    }
}
