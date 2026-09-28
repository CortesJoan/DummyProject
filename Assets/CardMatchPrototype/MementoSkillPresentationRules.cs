using UnityEngine;

/// <summary>Presentation timing and literal effect captions; does not own duel rules.</summary>
public static class MementoSkillPresentationRules
{
    public const float MinimumSeconds = 3.6f;
    public static float Duration(float voiceSeconds, string subtitle)
    {
        float reading = string.IsNullOrEmpty(subtitle) ? 0f : subtitle.Length / 24f + 0.65f;
        return Mathf.Clamp(Mathf.Max(MinimumSeconds, Mathf.Max(voiceSeconds + 0.3f, reading)), MinimumSeconds, 8f);
    }

    public static string Effect(int technique, bool hostile, bool copied)
    {
        if (hostile && !copied)
        {
            if (technique == 2) return "La rival observa una pareja antes de elegir.";
            if (technique == 4) return "Observa dos cartas. Si acierta, repite turno.";
            return technique == 3 ? "La rival observa cuatro cartas antes de elegir."
                : "La rival observa dos cartas antes de elegir.";
        }
        switch (technique)
        {
            case 0: return hostile ? "Puede repetir su siguiente intento si falla."
                : "Recuperas tu último intento y el combo anterior.";
            case 1: return hostile ? "Reduce en uno el daño de tu próxima pareja."
                : "Tu combo queda protegido contra un fallo.";
            case 2: return hostile ? "Localiza una pareja y recuerda sus cartas."
                : "Mira el tablero: una pareja se revelará.";
            case 3: return hostile ? "Encuentra una pareja y te hace daño."
                : "Encuentra una pareja por ti.";
            case 4: return hostile ? "Si acierta, conserva el turno."
                : "Repites el intento sin perder el turno ni tu combo.";
            default: return "Observa el tablero al terminar la animación.";
        }
    }
}
