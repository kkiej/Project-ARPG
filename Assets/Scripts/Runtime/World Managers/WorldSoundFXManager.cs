using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class WorldSoundFXManager : MonoBehaviour
    {
        public static WorldSoundFXManager instance;

        [Header("Boss Track")]
        [SerializeField] AudioSource bossIntroPlayer;
        [SerializeField] AudioSource bossLoopPlayer;

        [Header("Damage Sounds")]
        public AudioClip[] physicalDamageSFX;

        [Header("Action Sounds")]
        public AudioClip rollSFX;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void PlayBossTrack(AudioClip introTrack, AudioClip loopTrack)
        {
            bossIntroPlayer.volume = 1;
            bossIntroPlayer.clip = introTrack;
            bossIntroPlayer.loop = false;
            bossIntroPlayer.Play();

            bossLoopPlayer.volume = 1;
            bossLoopPlayer.clip = loopTrack;
            bossLoopPlayer.loop = true;
            bossLoopPlayer.PlayDelayed(bossIntroPlayer.clip.length);
        }

        public void StopBossMusic()
        {
            StartCoroutine(FadeOutBossMusicThenStop());
        }
        
        private IEnumerator FadeOutBossMusicThenStop()
        {
            while (bossLoopPlayer.volume > 0)
            {
                bossLoopPlayer.volume -= Time.deltaTime;
                bossIntroPlayer.volume -= Time.deltaTime;
                yield return null;
            }

            bossIntroPlayer.Stop();
            bossLoopPlayer.Stop();
        }
        
        public AudioClip ChooseRandomSFXFromArray(AudioClip[] array)
        {
            int index = Random.Range(0, array.Length);

            return array[index];
        }

        /*public AudioClip ChooseRandomFootStepSoundBasedOnGround(GameObject steppedOnObject, CharacterManager character)
        {
            if (steppedOnObject.CompareTag("Dirt"))
            {
                return ChooseRandomSFXFromArray(character.characterSoundFXManager.footStepsDirt);
            }
            else if (steppedOnObject.CompareTag("Stone"))
            {
                return ChooseRandomSFXFromArray(character.characterSoundFXManager.footStepsStone);
            }

            return null;
        }*/
    }
}