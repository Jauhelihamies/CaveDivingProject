using UnityEngine;
using TMPro;

public class WaterWaveTextEffect : MonoBehaviour
{
    private TMP_Text textComponent;

    [Header("Aallon asetukset")]
    public float waveSpeed = 3.0f;       // Kuinka nopeasti aalto etenee tekstiss‰
    public float waveHeight = 15.0f;     // Kuinka korkealle yksitt‰inen kirjain nousee
    public float waveWavelength = 0.2f;  // Kirjainten v‰linen viive (mit‰ pienempi, sit‰ loivempi aalto)

    void Awake()
    {
        // Haetaan TextMesh Pro -komponentti samasta objektista
        textComponent = GetComponent<TMP_Text>();
    }

    void Update()
    {
        // Varmistetaan, ett‰ teksti on ladattu ja ajan tasalla
        textComponent.ForceMeshUpdate();

        TMP_TextInfo textInfo = textComponent.textInfo;
        int characterCount = textInfo.characterCount;

        // K‰yd‰‰n jokainen tekstin kirjain l‰pi
        for (int i = 0; i < characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // Hyp‰t‰‰n yli n‰kym‰ttˆm‰t merkit (kuten v‰lilyˆnnit)
            if (!charInfo.isVisible)
                continue;

            // Haetaan tiedot siit‰, mihin mesh-materiaaliin kirjain kuuluu
            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            // Lasketaan aaltoliike: jokainen kirjain (i) saa oman pienen aikasiirtym‰n (waveWavelength)
            float offset = Mathf.Sin(Time.time * waveSpeed + (i * waveWavelength)) * waveHeight;

            // Kirjaimella on 4 kulmapistett‰ (vertices). Siirret‰‰n niit‰ kaikkia Y-akselilla saman verran.
            vertices[vertexIndex + 0].y += offset; // Vasen ala
            vertices[vertexIndex + 1].y += offset; // Vasen yl‰
            vertices[vertexIndex + 2].y += offset; // Oikea yl‰
            vertices[vertexIndex + 3].y += offset; // Oikea ala
        }

        // P‰ivitet‰‰n muutetut mesh-tiedot takaisin TextMesh Prolle, jotta muutokset n‰kyv‰t ruudulla
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}