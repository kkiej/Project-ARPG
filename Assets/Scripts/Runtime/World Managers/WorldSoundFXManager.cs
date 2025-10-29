using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

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
        public AudioClip pickUpItemSFX;
        public AudioClip rollSFX;
        public AudioClip stanceBreakSFX;
        public AudioClip criticalStrikeSFX;
        public AudioClip[] releaseArrowSFX;
        public AudioClip[] notchArrowSFX;
        public AudioClip healingFlaskSFX;

        [Header("UI Sounds")]
        public AudioClip unableToContinueUISFX;
        public AudioClip hoverUISFX;
        public AudioClip confirmUISFX;

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

        public AudioClip ChooseRandomSFXFromArray(AudioClip[] array)
        {
            int index = Random.Range(0, array.Length);

            return array[index];
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

        public void AlertNearbyCharactersToSound(Vector3 positionOfSound, float rangeOfSound)
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            Collider[] characterColliders = Physics.OverlapSphere(positionOfSound, rangeOfSound);

            List<AICharacterManager> charactersToAlert = new List<AICharacterManager>();

            for (int i = 0; i < characterColliders.Length; i++)
            {
                AICharacterManager aiCharacter = characterColliders[i].GetComponent<AICharacterManager>();

                if (aiCharacter == null)
                    continue;

                if (charactersToAlert.Contains(aiCharacter))
                    continue;

                charactersToAlert.Add(aiCharacter);
            }

            for (int i = 0; i < charactersToAlert.Count; i++)
            {
                charactersToAlert[i].aiCharacterCombatManager.AlertCharacterToSound(positionOfSound);
            }
        }
    }
}