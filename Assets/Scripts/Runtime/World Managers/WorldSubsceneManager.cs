using UnityEngine;
using System.Collections.Generic;

namespace LZ
{
    public class WorldSubsceneManager : MonoBehaviour
    {
        private List<PlayerManager> playersIn_Area01_Subarea00 = new List<PlayerManager>();
        private List<PlayerManager> playersIn_Area01_Subarea01 = new List<PlayerManager>();
        private List<PlayerManager> playersIn_Area01_Subarea02 = new List<PlayerManager>();
        private List<PlayerManager> playersIn_Area01_Subarea03 = new List<PlayerManager>();
        private List<PlayerManager> playersIn_Area01_Subarea04 = new List<PlayerManager>();
        private List<PlayerManager> playersIn_Area01_Subarea05 = new List<PlayerManager>();

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
    }
}
