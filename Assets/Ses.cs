using UnityEngine;

// Sesler kodla üretilir. Hazır bir şarkının kaydı değildir.
public static class Ses
{
    const int Ornek = 44100;
    static AudioSource efek;
    static AudioSource ilahiKaynak;
    static AudioSource metalKaynak;
    static AudioClip firlatma;
    static AudioClip sekme;
    static AudioClip kazanma;
    static AudioClip tik;
    static AudioClip kirilma;
    static AudioClip bagirma;
    static AudioClip ilahi;
    static AudioClip metal;

    public static void Firlat()
    {
        Cal(FirlatmaKlip(), 0.7f);
    }

    public static void Sekme()
    {
        Cal(SekmeKlip(), 0.55f);
    }

    public static void Kazan()
    {
        Cal(KazanmaKlip(), 0.75f);
    }

    public static void Tik()
    {
        Cal(TikKlip(), 0.4f);
    }

    public static void Kiril()
    {
        Cal(KirilmaKlip(), 0.8f);
    }

    public static void Bagir()
    {
        Cal(BagirmaKlip(), 0.95f);
    }

    // Kaybedince ikisi birden başlar: yavaş org ilahisi ve üstüne metal.
    public static void Kaybedince()
    {
        KaynaklariKur();
        ilahiKaynak.clip = IlahiKlip();
        ilahiKaynak.volume = 0.62f;
        ilahiKaynak.loop = true;
        ilahiKaynak.Play();
        metalKaynak.clip = MetalKlip();
        metalKaynak.volume = 0.5f;
        metalKaynak.loop = true;
        metalKaynak.Play();
    }

    static void Cal(AudioClip klip, float ses)
    {
        KaynaklariKur();
        efek.PlayOneShot(klip, ses);
    }

    static void KaynaklariKur()
    {
        if (efek != null)
            return;

        GameObject kok = new GameObject("Ses");
        efek = KaynakEkle(kok);
        ilahiKaynak = KaynakEkle(kok);
        metalKaynak = KaynakEkle(kok);
    }

    static AudioSource KaynakEkle(GameObject kok)
    {
        AudioSource kaynak = kok.AddComponent<AudioSource>();
        kaynak.playOnAwake = false;
        kaynak.spatialBlend = 0f;
        kaynak.ignoreListenerPause = true;
        return kaynak;
    }

    static AudioClip FirlatmaKlip()
    {
        if (firlatma != null)
            return firlatma;

        int n = (int)(0.18f * Ornek);
        float[] veri = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Ornek;
            float zarf = 1f - t / 0.18f;
            float gurultu = Gurultu(i) * zarf * zarf;
            float vurus = Mathf.Sin(t * 180f * Mathf.PI * 2f) * zarf;
            veri[i] = Mathf.Clamp(gurultu * 0.35f + vurus * 0.4f, -1f, 1f);
        }

        firlatma = KlipYap("Firlatma", veri);
        return firlatma;
    }

    static AudioClip SekmeKlip()
    {
        if (sekme != null)
            return sekme;

        int n = (int)(0.09f * Ornek);
        float[] veri = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Ornek;
            float zarf = Mathf.Exp(-t * 48f);
            veri[i] = Mathf.Clamp((Gurultu(i + 90) * 0.55f + Mathf.Sin(t * 520f * Mathf.PI * 2f) * 0.35f) * zarf, -1f, 1f);
        }

        sekme = KlipYap("Sekme", veri);
        return sekme;
    }

    static AudioClip KazanmaKlip()
    {
        if (kazanma != null)
            return kazanma;

        float[] notalar = { 523.25f, 659.25f, 783.99f };
        int n = (int)(0.7f * Ornek);
        float[] veri = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Ornek;
            int sira = Mathf.Min(2, (int)(t / 0.18f));
            float yerel = t - sira * 0.18f;
            float zarf = Mathf.Exp(-yerel * 6f) * (1f - Mathf.Clamp01((t - 0.55f) / 0.15f));
            float nota = Mathf.Sin(t * notalar[sira] * Mathf.PI * 2f);
            veri[i] = Mathf.Clamp(nota * zarf * 0.55f, -1f, 1f);
        }

        kazanma = KlipYap("Kazanma", veri);
        return kazanma;
    }

    static AudioClip TikKlip()
    {
        if (tik != null)
            return tik;

        int n = (int)(0.045f * Ornek);
        float[] veri = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Ornek;
            veri[i] = Mathf.Sin(t * 980f * Mathf.PI * 2f) * Mathf.Exp(-t * 80f) * 0.45f;
        }

        tik = KlipYap("Tik", veri);
        return tik;
    }

    static AudioClip KirilmaKlip()
    {
        if (kirilma != null)
            return kirilma;

        int n = (int)(0.35f * Ornek);
        float[] veri = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Ornek;
            float zarf = Mathf.Exp(-t * 10f);
            float tok = Mathf.Sin(t * 90f * Mathf.PI * 2f) * Mathf.Exp(-t * 18f);
            veri[i] = Mathf.Clamp((Gurultu(i + 400) * 0.7f * zarf + tok * 0.5f), -1f, 1f);
        }

        kirilma = KlipYap("Kirilma", veri);
        return kirilma;
    }

    static AudioClip BagirmaKlip()
    {
        if (bagirma != null)
            return bagirma;

        int n = (int)(0.42f * Ornek);
        float[] veri = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Ornek;
            float perde = Mathf.Lerp(420f, 160f, t / 0.42f);
            float zarf = Mathf.Sin(Mathf.Clamp01(t / 0.04f) * Mathf.PI * 0.5f) * Mathf.Exp(-t * 3.2f);
            float ses = Mathf.Sin(t * perde * Mathf.PI * 2f);
            ses += Mathf.Sin(t * perde * 2f * Mathf.PI * 2f) * 0.35f;
            veri[i] = Mathf.Clamp((ses * 0.7f + Gurultu(i + 80) * 0.18f) * zarf, -1f, 1f);
        }

        bagirma = KlipYap("Bagirma", veri);
        return bagirma;
    }

    static AudioClip IlahiKlip()
    {
        if (ilahi != null)
            return ilahi;

        // Özgün, yavaş bir amin kadansı. Bilinen bir ilahinin melodisi değil.
        float[][] akorlar =
        {
            new[] { 146.83f, 220f, 293.66f },
            new[] { 174.61f, 220f, 261.63f },
            new[] { 196f, 246.94f, 293.66f },
            new[] { 146.83f, 220f, 293.66f }
        };

        float sure = 8f;
        int n = (int)(sure * Ornek);
        float[] veri = new float[n];
        float akorSuresi = sure / akorlar.Length;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Ornek;
            int sira = Mathf.Min(akorlar.Length - 1, (int)(t / akorSuresi));
            float yerel = t - sira * akorSuresi;
            float zarf = Mathf.Clamp01(yerel / 0.25f) * (1f - Mathf.Clamp01((yerel - (akorSuresi - 0.35f)) / 0.35f));
            float vibrato = 1f + 0.004f * Mathf.Sin(t * 5.2f * Mathf.PI * 2f);
            float org = 0f;
            float[] akor = akorlar[sira];
            for (int k = 0; k < akor.Length; k++)
            {
                float frekans = akor[k] * vibrato;
                org += Mathf.Sin(t * frekans * Mathf.PI * 2f) * 0.22f;
                org += Mathf.Sin(t * frekans * 2f * Mathf.PI * 2f) * 0.08f;
            }

            float koro = Mathf.Sin(t * akor[1] * 1.003f * Mathf.PI * 2f) * 0.12f;
            float son = 1f - Mathf.Clamp01((t - (sure - 0.6f)) / 0.6f);
            veri[i] = Mathf.Clamp((org + koro) * zarf * son, -1f, 1f);
        }

        ilahi = KlipYap("Ilahi", veri);
        return ilahi;
    }

    static AudioClip MetalKlip()
    {
        if (metal != null)
            return metal;

        float sure = 8f;
        int n = (int)(sure * Ornek);
        float[] veri = new float[n];
        float vurus = 60f / 168f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Ornek;
            float sekizlik = vurus * 0.5f;
            float yer = t % sekizlik;
            float gitarZarf = yer < sekizlik * 0.42f ? 1f - yer / (sekizlik * 0.42f) : 0f;
            float testere = Mathf.Repeat(t * 82.41f, 1f) * 2f - 1f;
            float gitar = (float)System.Math.Tanh(testere * 5.5f) * gitarZarf * gitarZarf;

            float olcu = t % vurus;
            float kick = 0f;
            if (olcu < 0.11f)
            {
                float frekans = Mathf.Lerp(150f, 42f, olcu / 0.11f);
                kick = Mathf.Sin(t * frekans * Mathf.PI * 2f) * (1f - olcu / 0.11f);
            }

            float trampet = 0f;
            float olcuNo = Mathf.Floor(t / vurus);
            bool arkaVurus = ((int)olcuNo % 2) == 1;
            float arka = (t % (vurus * 2f)) - vurus;
            if (arkaVurus && arka >= 0f && arka < 0.12f)
                trampet = Gurultu(i) * Mathf.Exp(-arka * 28f);

            float son = 1f - Mathf.Clamp01((t - (sure - 0.4f)) / 0.4f);
            veri[i] = Mathf.Clamp((gitar * 0.42f + kick * 0.7f + trampet * 0.38f) * son, -1f, 1f);
        }

        metal = KlipYap("Metal", veri);
        return metal;
    }

    static float Gurultu(int i)
    {
        int n = i * 1103515245 + 12345;
        return (((n >> 16) & 32767) / 32767f) * 2f - 1f;
    }

    static AudioClip KlipYap(string ad, float[] veri)
    {
        AudioClip klip = AudioClip.Create(ad, veri.Length, 1, Ornek, false);
        klip.SetData(veri, 0);
        return klip;
    }
}
