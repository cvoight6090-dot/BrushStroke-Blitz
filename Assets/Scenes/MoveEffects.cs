using System.Collections.Generic;
using UnityEngine;

//spawns visual effect prefab on enemy target (normally UI image prefab with PixelEffect)

public static class MoveEffects
{
    public static float Play(MoveData move, List<Entity> targets)
    {
        if (move.effectPrefab == null) return 0f;

        float duration = move.effectDuration;
        
        bool isUI = move.effectPrefab.GetComponent<RectTransform>() != null;

        foreach (Entity target in targets)
        {
            GameObject fx;

            if (isUI)
            {
                Canvas canvas = target.GetComponentInParent<Canvas>();
                if (canvas == null) continue;

                fx = UnityEngine.Object.Instantiate(move.effectPrefab, canvas.transform);
                fx.transform.position = target.transform.position;
                fx.transform.SetAsLastSibling(); //draws on top of everything including sprites
            }
            //only UI sprites are used right now not the particle prefabs yet
            else
            {
                fx = UnityEngine.Object.Instantiate(move.effectPrefab, target.transform.position,
                Quaternion.identity);
            }

            //PixelEffect knows its length 
            PixelEffect pixelEffect = fx.GetComponent<PixelEffect>();
            if (pixelEffect != null) duration = pixelEffect.Duration;
            
            //just in case the pixelEffect doesn't destroy the object first
            UnityEngine.Object.Destroy(fx, duration + 2f);
        }

        return duration;
    }
}
