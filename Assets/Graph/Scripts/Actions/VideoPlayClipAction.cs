using UnityEngine;
using UnityEngine.Video;

namespace GameplayIngredients.Actions
{
    public class VideoPlayClipAction : ActionBase
    {
        [NonNullCheck]
        public VideoPlayer player;
        public VideoClip clip;
        public RenderTexture renderTexture;

        public override void Execute(GameObject instigator = null)
        {
            if(player != null)
            {
                player.Stop();

                if (clip != null)
                    player.clip = clip;

                    if (renderTexture != null)
                            player.targetTexture = renderTexture;

                    player.Play();
            }
        }
    }
}