using UnityEngine;

// Sekme anında kamerayı çok hafif titreştirir.
public class EkranSallama : MonoBehaviour
{
    Vector3 sabitKonum;
    float kalan;
    float depremGuc;
    const float Sure = 0.1f;
    const float Guc = 0.055f;

    void Awake()
    {
        sabitKonum = transform.localPosition;
    }

    public void KonumuKaydet()
    {
        sabitKonum = transform.localPosition;
        kalan = 0f;
    }

    public static void HafifSalla()
    {
        if (Camera.main == null)
            return;

        EkranSallama salla = Camera.main.GetComponent<EkranSallama>();
        if (salla == null)
            salla = Camera.main.gameObject.AddComponent<EkranSallama>();

        salla.kalan = Mathf.Max(salla.kalan, Sure);
    }

    public static void Deprem(float sure)
    {
        if (Camera.main == null)
            return;

        EkranSallama salla = Camera.main.GetComponent<EkranSallama>();
        if (salla == null)
            salla = Camera.main.gameObject.AddComponent<EkranSallama>();

        salla.kalan = Mathf.Max(salla.kalan, sure);
        salla.depremGuc = 0.28f;
    }

    void LateUpdate()
    {
        if (kalan <= 0f)
        {
            transform.localPosition = sabitKonum;
            return;
        }

        kalan -= Time.unscaledDeltaTime;
        float guc = depremGuc > 0f ? depremGuc : Guc;
        if (depremGuc > 0f)
            guc += Mathf.PerlinNoise(Time.unscaledTime * 2.4f, 3f) * 0.22f;
        float oran = depremGuc > 0f ? 1f : Mathf.Clamp01(kalan / Sure);
        Vector2 kayma = Random.insideUnitCircle * guc * oran;
        transform.localPosition = sabitKonum + new Vector3(kayma.x, kayma.y, 0f);
        if (kalan <= 0f)
            depremGuc = 0f;
    }
}
