using UnityEditor;
using UnityEngine;

// 3D Pinball: masa uzun ve dar, kamera alt uçtan masanın içine bakar.
public static class BolumUretici
{
    static readonly string[] CevreIsimleri =
    {
        "Zemin",
        "Sağ Duvar",
        "Sol Duvar",
        "Arka Duvar",
        "Ön Duvar"
    };

    [MenuItem("Oyun Araçları/Bölüm 1 İnşa Et")]
    public static void Bolum1InsaEt()
    {
        Undo.SetCurrentGroupName("Bölüm 1 İnşa Et");
        int undoGrup = Undo.GetCurrentGroup();

        EskiCevreyiSil();

        GameObject bolum = new GameObject("Bolum 1 Cevre");
        Undo.RegisterCreatedObjectUndo(bolum, "Bölüm klasörü");

        // Üst yüzey Y=5. En 7.2, boy 18. Pinball masası gibi uzun ve dar.
        KupOlustur("Zemin", new Vector3(0f, 4.95f, 9f), new Vector3(7.2f, 0.1f, 18f), bolum.transform, "Assets/Materials/Zemin.mat");
        KupOlustur("Sağ Duvar", new Vector3(3.775f, 5.5f, 9f), new Vector3(0.35f, 1f, 18f), bolum.transform, "Assets/Materials/Duvar.mat");
        KupOlustur("Sol Duvar", new Vector3(-3.775f, 5.5f, 9f), new Vector3(0.35f, 1f, 18f), bolum.transform, "Assets/Materials/Duvar.mat");
        KupOlustur("Arka Duvar", new Vector3(0f, 5.5f, 18.175f), new Vector3(7.9f, 1f, 0.35f), bolum.transform, "Assets/Materials/Duvar.mat");
        KupOlustur("Ön Duvar", new Vector3(0f, 5.5f, -0.175f), new Vector3(7.9f, 1f, 0.35f), bolum.transform, "Assets/Materials/Duvar.mat");

        KupYerlestir("Engel 1", new Vector3(-1.5f, 5.4f, 7.5f), new Vector3(2.4f, 0.8f, 0.35f), new Vector3(0f, 28f, 0f), "Assets/Materials/Engel.mat");
        KupYerlestir("Engel 2", new Vector3(1.8f, 5.4f, 11f), new Vector3(1.1f, 0.8f, 1.1f), Vector3.zero, "Assets/Materials/Engel.mat");
        KupYerlestir("Hedef", new Vector3(0f, 5.55f, 15.5f), new Vector3(1.4f, 1.1f, 0.3f), Vector3.zero, "Assets/Materials/Hedef.mat");

        DuvarTipiVer("Sağ Duvar", DuvarTuru.Normal, 10, true);
        DuvarTipiVer("Sol Duvar", DuvarTuru.Normal, 10, true);
        DuvarTipiVer("Arka Duvar", DuvarTuru.Normal, 10, true);
        DuvarTipiVer("Ön Duvar", DuvarTuru.Normal, 10, true);
        DuvarTipiVer("Engel 1", DuvarTuru.Sayili, 10);
        DuvarTipiVer("Engel 2", DuvarTuru.Kirmizi);
        MaviDuvarOlustur();

        HedefiSilindirYap();

        KapagiYerlestir();
        KamerayiAyarla();

        Undo.CollapseUndoOperations(undoGrup);
        Debug.Log("Masa 3D Pinball oranına getirildi.");
    }

    static void EskiCevreyiSil()
    {
        GameObject eskiBolum = GameObject.Find("Bolum 1 Cevre");
        if (eskiBolum != null)
            Undo.DestroyObjectImmediate(eskiBolum);

        foreach (string isim in CevreIsimleri)
        {
            GameObject eski = GameObject.Find(isim);
            if (eski != null)
                Undo.DestroyObjectImmediate(eski);
        }
    }

    static void KupYerlestir(string isim, Vector3 konum, Vector3 olcek, Vector3 euler, string materyal)
    {
        GameObject obje = GameObject.Find(isim);
        if (obje == null)
        {
            KupOlustur(isim, konum, olcek, null, materyal);
            obje = GameObject.Find(isim);
            if (obje != null)
                obje.transform.rotation = Quaternion.Euler(euler);
            return;
        }

        Undo.RecordObject(obje.transform, isim);
        obje.transform.position = konum;
        obje.transform.localScale = olcek;
        obje.transform.rotation = Quaternion.Euler(euler);
        MateryalVer(obje, materyal);
    }

    static void DuvarTipiVer(string isim, DuvarTuru tur, int can = 10, bool yandaki = false)
    {
        GameObject obje = GameObject.Find(isim);
        if (obje == null)
            return;

        Duvar duvar = obje.GetComponent<Duvar>();
        if (duvar == null)
            duvar = Undo.AddComponent<Duvar>(obje);

        Undo.RecordObject(duvar, "Duvar tipi");
        duvar.tur = tur;
        duvar.baslangicCan = can;
        duvar.yandakiDuvar = yandaki;
    }

    static void MaviDuvarOlustur()
    {
        GameObject eski = GameObject.Find("Mavi Duvar");
        if (eski != null)
            Undo.DestroyObjectImmediate(eski);

        GameObject kutu = GameObject.CreatePrimitive(PrimitiveType.Cube);
        kutu.name = "Mavi Duvar";
        kutu.transform.position = new Vector3(1.55f, 5.4f, 8.6f);
        kutu.transform.localScale = new Vector3(1.7f, 0.8f, 0.35f);
        kutu.transform.rotation = Quaternion.Euler(0f, -20f, 0f);
        Undo.RegisterCreatedObjectUndo(kutu, "Mavi duvar");
        DuvarTipiVer("Mavi Duvar", DuvarTuru.Hayalet, 10);
    }

    static void HedefiSilindirYap()
    {
        GameObject hedef = GameObject.Find("Hedef");
        if (hedef == null)
            return;

        Undo.RecordObject(hedef.transform, "Hedef silindir");
        hedef.transform.position = new Vector3(0f, 5.7f, 15.5f);
        hedef.transform.localScale = new Vector3(0.8f, 0.7f, 0.8f);
        hedef.transform.rotation = Quaternion.identity;

        GameObject ornek = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Mesh silindir = ornek.GetComponent<MeshFilter>().sharedMesh;
        Undo.DestroyObjectImmediate(ornek);
        hedef.GetComponent<MeshFilter>().sharedMesh = silindir;

        BoxCollider kutu = hedef.GetComponent<BoxCollider>();
        if (kutu != null)
            Undo.DestroyObjectImmediate(kutu);

        if (hedef.GetComponent<CapsuleCollider>() == null)
        {
            CapsuleCollider kapsul = Undo.AddComponent<CapsuleCollider>(hedef);
            kapsul.direction = 1;
            kapsul.height = 2f;
            kapsul.radius = 0.5f;
        }

        if (hedef.GetComponent<HedefDavranis>() == null)
            Undo.AddComponent<HedefDavranis>(hedef);
    }

    static void KapagiYerlestir()
    {
        GameObject kapak = GameObject.Find("Kapak");
        if (kapak == null)
        {
            Debug.LogWarning("Kapak sahnede bulunamadı.");
            return;
        }

        Undo.RecordObject(kapak.transform, "Kapak");
        kapak.transform.position = new Vector3(0f, 5.08f, 2.2f);
        kapak.transform.localScale = new Vector3(0.65f, 0.06f, 0.65f);
        kapak.transform.rotation = Quaternion.identity;
        MateryalVer(kapak, "Assets/Materials/Kapak.mat");
    }

    static void KamerayiAyarla()
    {
        GameObject kameraObje = GameObject.Find("Main Camera");
        if (kameraObje == null)
        {
            Debug.LogWarning("Main Camera sahnede bulunamadı.");
            return;
        }

        Undo.RecordObject(kameraObje.transform, "Kamera");
        kameraObje.transform.position = new Vector3(0f, 18.5f, -8.5f);
        kameraObje.transform.rotation = Quaternion.Euler(41f, 0f, 0f);

        Camera kamera = kameraObje.GetComponent<Camera>();
        if (kamera == null)
            return;

        Undo.RecordObject(kamera, "Kamera açısı");
        kamera.fieldOfView = 50f;
    }

    static void KupOlustur(string isim, Vector3 konum, Vector3 olcek, Transform parent, string materyalYolu)
    {
        GameObject kup = GameObject.CreatePrimitive(PrimitiveType.Cube);
        kup.name = isim;
        if (parent != null)
            kup.transform.SetParent(parent);
        kup.transform.position = konum;
        kup.transform.localScale = olcek;

        if (kup.GetComponent<BoxCollider>() == null)
            kup.AddComponent<BoxCollider>();

        MateryalVer(kup, materyalYolu);
        Undo.RegisterCreatedObjectUndo(kup, isim + " oluşturuldu");
    }

    static void MateryalVer(GameObject obje, string yol)
    {
        Material materyal = AssetDatabase.LoadAssetAtPath<Material>(yol);
        Renderer gorunum = obje.GetComponent<Renderer>();
        if (materyal == null || gorunum == null)
            return;

        Undo.RecordObject(gorunum, "Materyal");
        gorunum.sharedMaterial = materyal;
    }
}
