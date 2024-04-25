using HarmonyLib;
using System.Collections;
using System.Linq;
using TeacherAPI;
using UnityEngine;

namespace NullTeacher.Patches
{
    [HarmonyPatch(typeof(CoreGameManager), nameof(CoreGameManager.EndSequence))]
    internal class EndSequencePatch
    {
        internal static void Postfix(CoreGameManager __instance, ref IEnumerator __result)
        {
            var originalEnumerator = __result;

            IEnumerator GetEnumerator()
            {
                // Copy pasted from CoreGameManager lmao
                if (TeacherManager.Instance.GetTeachersOfType<NullTeacher>().Count() > 0)
                {
                    var nolives = __instance.lives < 1 && __instance.extraLives < 1;
                    float time = 0f;
                    float glitchRate = 0.5f;
                    Shader.SetGlobalInt("_ColorGlitching", 1);
                    Shader.SetGlobalInt("_SpriteColorGlitching", 1);
                    if (Singleton<PlayerFileManager>.Instance.reduceFlashing)
                    {
                        Shader.SetGlobalInt("_ColorGlitchVal", Random.Range(0, 4096));
                        Shader.SetGlobalInt("_SpriteColorGlitchVal", Random.Range(0, 4096));
                    }
                    yield return null;
                    while (time <= (nolives ? 1f : 5f))
                    {
                        time += Time.unscaledDeltaTime * 0.5f;
                        Shader.SetGlobalFloat("_VertexGlitchSeed", Random.Range(0f, 1000f));
                        Shader.SetGlobalFloat("_TileVertexGlitchSeed", Random.Range(0f, 1000f));
                        Singleton<InputManager>.Instance.Rumble(time / 5f, 0.05f);
                        if (!Singleton<PlayerFileManager>.Instance.reduceFlashing)
                        {
                            glitchRate -= Time.unscaledDeltaTime;
                            Shader.SetGlobalFloat("_VertexGlitchIntensity", Mathf.Pow(time, 2.2f));
                            Shader.SetGlobalFloat("_TileVertexGlitchIntensity", Mathf.Pow(time, 2.2f));
                            Shader.SetGlobalFloat("_ColorGlitchPercent", time * 0.05f);
                            Shader.SetGlobalFloat("_SpriteColorGlitchPercent", time * 0.05f);
                            if (glitchRate <= 0f)
                            {
                                Shader.SetGlobalInt("_ColorGlitchVal", Random.Range(0, 4096));
                                Shader.SetGlobalInt("_SpriteColorGlitchVal", Random.Range(0, 4096));
                                Singleton<InputManager>.Instance.SetColor(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
                                glitchRate = 0.55f - time * 0.1f;
                            }
                        }
                        else
                        {
                            Shader.SetGlobalFloat("_ColorGlitchPercent", time * 0.25f);
                            Shader.SetGlobalFloat("_SpriteColorGlitchPercent", time * 0.25f);
                            Shader.SetGlobalFloat("_VertexGlitchIntensity", time * 2f);
                            Shader.SetGlobalFloat("_TileVertexGlitchIntensity", time * 2f);
                        }
                        yield return null;
                    }
                    yield return null;
                    if (nolives)
                    {
                        Application.Quit();
                    }
                    else
                    {
                        if (__instance.lives > 0)
                        {
                            __instance.lives--;
                        }
                        else
                        {
                            __instance.extraLives--;
                        }
                        Singleton<BaseGameManager>.Instance.RestartLevel();
                    }
                    yield break;
                }
                else
                {
                    yield return originalEnumerator;
                }
            }

            __result = GetEnumerator();
        }
    }
}
