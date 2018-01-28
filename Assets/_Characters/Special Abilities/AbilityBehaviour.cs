using System.Collections;
using UnityEngine;

namespace RPG.Characters
{
    public abstract class AbilityBehaviour : MonoBehaviour
    {
        protected AbilityConfig config = null;   // Protected can be accessed by children

        AudioSource audioSource = null;
        AudioClip audioclip = null;

        const float PARTICLE_CLEAN_UP_DELAY = 5f;

        // Abstract class is overridden in the child classes.
        public abstract void Use(AbilityUseParams useParams);

        public void SetConfig(AbilityConfig configToSet)
        {
            config = configToSet;
        }

        protected void PlayAbilityAudio()
        {
            audioclip = config.GetRandomAbilitySound();
            if (audioclip != null)
            {
                audioSource = GetComponent<AudioSource>();
                audioSource.PlayOneShot(audioclip);
            }
        }

        protected void PlayParticleEffect()
        {
            var particlePrefab = Instantiate(config.GetParticlePrefab(), this.gameObject.transform);
            particlePrefab.GetComponent<ParticleSystem>().Play();
            StartCoroutine(DestroyParticleWhenFinished(particlePrefab));
        }

        IEnumerator DestroyParticleWhenFinished(GameObject particlePrefab)
        {
            while (particlePrefab.GetComponent<ParticleSystem>().isPlaying)
            {
                yield return new WaitForSeconds(PARTICLE_CLEAN_UP_DELAY);
            }
            Destroy(particlePrefab);
            yield return new WaitForEndOfFrame();
        }


    }
}
