using System;
using System.IO;
using System.Collections.Generic;
using Eleon;
using Eleon.Modding;
using UnityEngine;
using System.Drawing;
using System.Windows.Input;

namespace LCDHandler
{
    public class MyEmpyrionMod : IMod, ModInterface
    {
        internal static string ModShortName = "LCDHandler";
        public static string ModVersion = ModShortName + " v0.0.0 made by Xango2000 (Tested: v1.1.7 build 3082)";
        public static string ModPath = "..\\Content\\Mods\\" + ModShortName + "\\";
        internal static IModApi modApi;
        internal static bool debug = true;
        internal static Dictionary<int, Storage.StorableData> SeqNrStorage = new Dictionary<int, Storage.StorableData> { };
        public int thisSeqNr = 2000;
        internal static SetupYaml.Root SetupYamlData = new SetupYaml.Root { };
        internal static string BootupTimestamp = "Timestamp";



        string Name = "DefaultName";
        //internal int HasBeenRun = 0;
        internal int IEntityLooper = 0;
        
        Dictionary<int, IEntity> KnownEntities = new Dictionary<int, IEntity> { };
        IEntity StoredIEntity;
        int Ticker = 0;

        internal bool Online = false;
        bool Running = false;

        public void Game_Event(CmdId eventId, ushort seqNr, object data)
        {
            CommonFunctions.Log("Game_Event Received", Name);
            if (eventId == CmdId.Event_GameEvent)
            {
                GameEventData GameEvent = (GameEventData)data;
                try { CommonFunctions.Log("GameEvent.EventType = " + GameEvent.EventType, Name); } catch { }
                try { CommonFunctions.Log("GameEvent.Amount = " + GameEvent.Amount, Name); } catch { }
                try { CommonFunctions.Log("GameEvent.Flag = " + GameEvent.Flag, Name); } catch { }
                try { CommonFunctions.Log("GameEvent.Name = " + GameEvent.Name, Name); } catch { }
                try { CommonFunctions.Log("GameEvent.PlayerID = " + GameEvent.PlayerId, Name); } catch { }
                try { CommonFunctions.Log("GameEvent.Type = " + GameEvent.Type, Name); } catch { }
                try { CommonFunctions.Log("GameEvent.ItemStacks = " + GameEvent.ItemStacks, Name); } catch { }
                try { CommonFunctions.Log("", Name); } catch { }
            }
        }

        public void Game_Exit()
        {
        }

        public void Game_Start(ModGameAPI dediAPI)
        {
            Storage.DediAPI = dediAPI;
        }

        public void Game_Update()
        {

        }

        public void Init(IModApi modAPI)
        {
            modApi = modAPI;
            BootupTimestamp = CommonFunctions.TimeStampFilename();
            Name = modApi.Application.Mode.ToString();
            if (modApi.Application.Mode == ApplicationMode.PlayfieldServer)
            {
                try { modAPI.Application.OnPlayfieldLoaded += Application_OnPlayfieldLoaded; } catch { }
                try { modAPI.Application.Update += Application_Update; } catch { }
            }
            //try { modAPI.Application.OnPlayfieldUnloading -= Application_OnPlayfieldUnloading; } catch { }
        }

        private void Application_Update()
        {
            Ticker = Ticker + 1;
            if ( modApi.Application.Mode == ApplicationMode.PlayfieldServer)
            {
                if (Ticker > 100)
                {
                    Ticker = 0;
                    try
                    {
                        Playfield_OnEntityLoaded(StoredIEntity);
                        foreach(int entity in KnownEntities.Keys)
                        {
                            Playfield_OnEntityLoaded(KnownEntities[entity]);
                        }
                    }
                    catch (Exception ex)
                    {
                        CommonFunctions.Exception(ex, "ApplicationUpdateError.txt");
                        /*
                        CommonFunctions.Log("Message: " + ex.Message, "ERROR");
                        CommonFunctions.Log("Data: " + ex.Data, "ERROR");
                        CommonFunctions.Log("HelpLink: " + ex.HelpLink, "ERROR");
                        CommonFunctions.Log("InnerException: " + ex.InnerException, "ERROR");
                        CommonFunctions.Log("Source: " + ex.Source, "ERROR");
                        CommonFunctions.Log("StackTrace: " + ex.StackTrace, "ERROR");
                        CommonFunctions.Log("TargetSite: " + ex.TargetSite, "ERROR");
                        CommonFunctions.Log("", "ERROR");
                        */
                    }
                }
            }
        }


        private void Application_OnPlayfieldLoaded(IPlayfield playfield)
        {
            if (Running == false)
            {
                Running = true;
                playfield.OnEntityLoaded += Playfield_OnEntityLoaded;
            }
            if (playfield.Name != null)
            {
                CommonFunctions.Log("Running Application_PlayfieldLoaded for " + playfield.Name, Name);
                IEntityLooper = 0;
            }
        }

        private void Playfield_OnEntityLoaded(IEntity entity)
        {

            if ( entity.Id == 1354036)
            {
                StoredIEntity = entity;
            }
            try
            {
                if ( entity.Type == EntityType.CV || entity.Type == EntityType.HV || entity.Type == EntityType.SV || entity.Type == EntityType.BA)
                {
                    if (!KnownEntities.ContainsKey(entity.Id))
                    {
                        KnownEntities[entity.Id] = entity;
                    }
                    CommonFunctions.Log("EntityLoading = " + entity.Name, Name);
                    IDevicePosList HarvestContainers = entity.Structure.GetDevices(DeviceTypeName.HarvestCntr);
                    for(int i = 0; i <= HarvestContainers.Count; i++)
                    {
                        VectorInt3 Coords = HarvestContainers.GetAt(i);
                        CommonFunctions.Log("Harvest Container at " + Coords.x + "," + Coords.y + "," + Coords.z, Name);
                    }


                    List<VectorInt3> CargoPositions = entity.Structure.GetDevicePositions("Cargo:Iron");
                    int IronCount = 0;
                    foreach (VectorInt3 pos in CargoPositions)
                    {
                        try
                        {
                            IContainer CargoBox = entity.Structure.GetDevice<IContainer>(pos.x, pos.y, pos.z);
                            List<ItemStack> CargoContents = CargoBox.GetContent();
                            foreach (ItemStack Stack in CargoContents)
                            {
                                if ( Stack.id == 2249)
                                {
                                    IronCount = IronCount + Stack.count;
                                }
                            }
                            CommonFunctions.Log("IronCount= " + IronCount);
                        }
                        catch { }
                    }

                    List<VectorInt3> LCDPositions = entity.Structure.GetDevicePositions("LCD:Iron");
                    foreach (VectorInt3 pos in LCDPositions)
                    {
                        try
                        {
                            ILcd LCD = entity.Structure.GetDevice<ILcd>(pos.x, pos.y, pos.z);
                            LCD.SetText("IronOre = " + IronCount);
                            CommonFunctions.Log("LCD Change: " + entity.Id, Name);
                        }
                        catch { }
                    }
                }
            } catch { }
        }

        public void Shutdown()
        {
        }
    }
}
