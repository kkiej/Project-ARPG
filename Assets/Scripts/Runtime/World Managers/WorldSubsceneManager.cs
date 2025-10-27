using UnityEngine;
using System.Collections.Generic;

namespace LZ
{
    public class WorldSubsceneManager : MonoBehaviour
    {
        public static WorldSubsceneManager instance;

        [SerializeField] private List<PlayerManager> playersIn_Area01_Subarea00 = new List<PlayerManager>();
        [SerializeField] private List<PlayerManager> playersIn_Area01_Subarea01 = new List<PlayerManager>();
        [SerializeField] private List<PlayerManager> playersIn_Area01_Subarea02 = new List<PlayerManager>();
        [SerializeField] private List<PlayerManager> playersIn_Area01_Subarea03 = new List<PlayerManager>();
        [SerializeField] private List<PlayerManager> playersIn_Area01_Subarea04 = new List<PlayerManager>();
        [SerializeField] private List<PlayerManager> playersIn_Area01_Subarea05 = new List<PlayerManager>();

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

        public List<string> GenerateDoNotUnloadListBasedOnPlayerLocations()
        {
            List<string> doNotUnloadLocations = new List<string>();

            //  THE WORLD SCENE IS NEVER UNLOADED
            doNotUnloadLocations.Add(WorldSceneManager.instance.world);

            int playersInScene;

            //  SUB AREA 00
            //  SET PLAYERS IN SCENE COUNT TO 0
            playersInScene = 0;

            //  CHECK FOR ANY PLAYERS IN THIS SPECIFIC SCENE
            for (int i = 0; i < playersIn_Area01_Subarea00.Count; i++)
            {
                if (playersIn_Area01_Subarea00[i] != null)
                    playersInScene++;
            }

            //  IF THE PLAYERS IN THIS SCENE ARE GREATER THAN 0, KEEP SCENES LOADED THAT ARE REQUIRED FOR THIS SCENE
            if (playersInScene > 0)
            {
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_00);
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_01);
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_04);
            }

            //  SUB AREA 01
            //  SET PLAYERS IN SCENE COUNT TO 0
            playersInScene = 0;

            //  CHECK FOR ANY PLAYERS IN THIS SPECIFIC SCENE
            for (int i = 0; i < playersIn_Area01_Subarea01.Count; i++)
            {
                if (playersIn_Area01_Subarea01[i] != null)
                    playersInScene++;
            }

            //  IF THE PLAYERS IN THIS SCENE ARE GREATER THAN 0, KEEP SCENES LOADED THAT ARE REQUIRED FOR THIS SCENE
            if (playersInScene > 0)
            {
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_01);
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_00);
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_02);
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_04);
            }

            //  SUB AREA 02
            //  SET PLAYERS IN SCENE COUNT TO 0
            playersInScene = 0;

            //  CHECK FOR ANY PLAYERS IN THIS SPECIFIC SCENE
            for (int i = 0; i < playersIn_Area01_Subarea02.Count; i++)
            {
                if (playersIn_Area01_Subarea02[i] != null)
                    playersInScene++;
            }

            //  IF THE PLAYERS IN THIS SCENE ARE GREATER THAN 0, KEEP SCENES LOADED THAT ARE REQUIRED FOR THIS SCENE
            if (playersInScene > 0)
            {
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_02);
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_03);
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_01);
            }

            //  SUB AREA 03
            //  SET PLAYERS IN SCENE COUNT TO 0
            playersInScene = 0;

            //  CHECK FOR ANY PLAYERS IN THIS SPECIFIC SCENE
            for (int i = 0; i < playersIn_Area01_Subarea03.Count; i++)
            {
                if (playersIn_Area01_Subarea03[i] != null)
                    playersInScene++;
            }

            //  IF THE PLAYERS IN THIS SCENE ARE GREATER THAN 0, KEEP SCENES LOADED THAT ARE REQUIRED FOR THIS SCENE
            if (playersInScene > 0)
            {
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_03);
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_02);
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_04);
            }

            //  SUB AREA 04
            //  SET PLAYERS IN SCENE COUNT TO 0
            playersInScene = 0;

            //  CHECK FOR ANY PLAYERS IN THIS SPECIFIC SCENE
            for (int i = 0; i < playersIn_Area01_Subarea04.Count; i++)
            {
                if (playersIn_Area01_Subarea04[i] != null)
                    playersInScene++;
            }

            //  IF THE PLAYERS IN THIS SCENE ARE GREATER THAN 0, KEEP SCENES LOADED THAT ARE REQUIRED FOR THIS SCENE
            if (playersInScene > 0)
            {
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_04);
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_00);
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_01);
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_03);
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_05);
            }

            //  SUB AREA 05
            //  SET PLAYERS IN SCENE COUNT TO 0
            playersInScene = 0;

            //  CHECK FOR ANY PLAYERS IN THIS SPECIFIC SCENE
            for (int i = 0; i < playersIn_Area01_Subarea05.Count; i++)
            {
                if (playersIn_Area01_Subarea05[i] != null)
                    playersInScene++;
            }

            //  IF THE PLAYERS IN THIS SCENE ARE GREATER THAN 0, KEEP SCENES LOADED THAT ARE REQUIRED FOR THIS SCENE
            if (playersInScene > 0)
            {
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_05);
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_00);
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_01);
                doNotUnloadLocations.Add(WorldSceneManager.instance.area_01_Subarea_04);
            }

            return doNotUnloadLocations;
        }

        //  THIS IS CALLED WHENEVER A PLAYER ENTERS A NEW ADDITIVE SCENE
        public void LoadAreasBasedOnAreaCurrentIn(WorldSceneLocation areaCurrentlyIn, PlayerManager player)
        {
            //  1. IS THE PLAYER CURRENTLY ALREADY IN THE AREA? IF SO, ABORT SO WE DO NOT RELOAD
            if (IsPlayerAlreadyInArea(areaCurrentlyIn, player))
                return;

            //  2. REMOVE THE PLAYER FROM ANY PREVIOUS LOCATIONS
            RemovePlayerFromPreviousLocation(player);

            //  3. ADD THE PLAYER TO THE NEW LOCATION
            AddPlayerToNewLocation(areaCurrentlyIn, player);

            //  4. LOAD THE NEW SCENES AROUND THE PLAYER
            LoadAdditiveScenesAroundCurrentArea(areaCurrentlyIn);

            //  5. UNLOAD ANY UNREQUIRED SCENES
            WorldSceneManager.instance.CheckForUnRequiredScenes();
        }

        private bool IsPlayerAlreadyInArea(WorldSceneLocation area, PlayerManager player)
        {
            bool isPlayerInArea = false;

            switch (area)
            {
                case WorldSceneLocation.Area01_Subarea00:
                    if (playersIn_Area01_Subarea00.Contains(player))
                        isPlayerInArea = true;
                    break;
                case WorldSceneLocation.Area01_Subarea01:
                    if (playersIn_Area01_Subarea01.Contains(player))
                        isPlayerInArea = true;
                    break;
                case WorldSceneLocation.Area01_Subarea02:
                    if (playersIn_Area01_Subarea02.Contains(player))
                        isPlayerInArea = true;
                    break;
                case WorldSceneLocation.Area01_Subarea03:
                    if (playersIn_Area01_Subarea03.Contains(player))
                        isPlayerInArea = true;
                    break;
                case WorldSceneLocation.Area01_Subarea04:
                    if (playersIn_Area01_Subarea04.Contains(player))
                        isPlayerInArea = true;
                    break;
                case WorldSceneLocation.Area01_Subarea05:
                    if (playersIn_Area01_Subarea05.Contains(player))
                        isPlayerInArea = true;
                    break;
                default:
                    break;
            }

            return isPlayerInArea;
        }

        private void RemovePlayerFromPreviousLocation(PlayerManager player)
        {
            if (player == null)
                return;

            if (playersIn_Area01_Subarea00.Contains(player))
                playersIn_Area01_Subarea00.Remove(player);

            if (playersIn_Area01_Subarea01.Contains(player))
                playersIn_Area01_Subarea01.Remove(player);

            if (playersIn_Area01_Subarea02.Contains(player))
                playersIn_Area01_Subarea02.Remove(player);

            if (playersIn_Area01_Subarea03.Contains(player))
                playersIn_Area01_Subarea03.Remove(player);

            if (playersIn_Area01_Subarea04.Contains(player))
                playersIn_Area01_Subarea04.Remove(player);

            if (playersIn_Area01_Subarea05.Contains(player))
                playersIn_Area01_Subarea05.Remove(player);
        }

        private void AddPlayerToNewLocation(WorldSceneLocation area, PlayerManager player)
        {
            switch (area)
            {
                case WorldSceneLocation.Area01_Subarea00:
                    if (!playersIn_Area01_Subarea00.Contains(player))
                        playersIn_Area01_Subarea00.Add(player);
                    break;
                case WorldSceneLocation.Area01_Subarea01:
                    if (!playersIn_Area01_Subarea01.Contains(player))
                        playersIn_Area01_Subarea01.Add(player);
                    break;
                case WorldSceneLocation.Area01_Subarea02:
                    if (!playersIn_Area01_Subarea02.Contains(player))
                        playersIn_Area01_Subarea02.Add(player);
                    break;
                case WorldSceneLocation.Area01_Subarea03:
                    if (!playersIn_Area01_Subarea03.Contains(player))
                        playersIn_Area01_Subarea03.Add(player);
                    break;
                case WorldSceneLocation.Area01_Subarea04:
                    if (!playersIn_Area01_Subarea04.Contains(player))
                        playersIn_Area01_Subarea04.Add(player);
                    break;
                case WorldSceneLocation.Area01_Subarea05:
                    if (!playersIn_Area01_Subarea05.Contains(player))
                        playersIn_Area01_Subarea05.Add(player);
                    break;
                default:
                    break;
            }
        }

        private void LoadAdditiveScenesAroundCurrentArea(WorldSceneLocation area)
        {
            List<string> scenesToLoad = new List<string>();

            switch (area)
            {
                case WorldSceneLocation.Area01_Subarea00:
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_00);
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_01);
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_04);
                    break;
                case WorldSceneLocation.Area01_Subarea01:
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_01);
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_00);
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_02);
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_04);
                    break;
                case WorldSceneLocation.Area01_Subarea02:
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_02);
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_03);
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_01);
                    break;
                case WorldSceneLocation.Area01_Subarea03:
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_03);
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_02);
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_04);
                    break;
                case WorldSceneLocation.Area01_Subarea04:
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_04);
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_00);
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_01);
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_03);
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_05);
                    break;
                case WorldSceneLocation.Area01_Subarea05:
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_05);
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_00);
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_01);
                    scenesToLoad.Add(WorldSceneManager.instance.area_01_Subarea_04);
                    break;
                default:
                    break;
            }

            if (scenesToLoad.Count <= 0)
                return;

            WorldSceneManager.instance.LoadAdditiveScenes(scenesToLoad);
        }
    }
}
