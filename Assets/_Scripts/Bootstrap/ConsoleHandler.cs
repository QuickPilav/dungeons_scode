using DavidFDev.DevConsole;
using Photon.Pun;
using System;
using UnityEngine;

public static class ConsoleHandler
{
    private static bool lastConsoleWasOpen;
    public static void Initialize(MonoBehaviour m)
    {
        DevConsole.RemoveCommand("cam_fov");
        DevConsole.RemoveCommand("cam_ortho");

        DevConsole.EnableConsole();

        DevConsole.AddCommand(Command.Create<bool>(
            name: "createroom",
            aliases: "",
            helpText: "Creates room public or private",
            p1: Parameter.Create(
                name: "isPublic",
                helpText: "Is this room visible to other players"),
            callback: CreateRoom,
            defaultCallback: () => CreateRoom(false)
        ));

        if (Debug.isDebugBuild)
        {
            DevConsole.SetTrackedStat("IsConnecting", () => PhotonManager.IsConnecting, true);
            DevConsole.SetTrackedStat("IsConnected", () => PhotonManager.IsConnected, true);
            DevConsole.SetTrackedStat("InRoom", () => PhotonManager.InRoom, true);
            DevConsole.SetTrackedStat("InLobby", () => PhotonManager.InLobby, true);
            
            DevConsole.SetTrackedStat("Photon.OfflineMode", () => PhotonNetwork.OfflineMode, true);
            DevConsole.SetTrackedStat("Photon.IsConnectedAndReady", () => PhotonNetwork.IsConnectedAndReady, true);
            DevConsole.SetTrackedStat("Photon.IsConnected", () => PhotonNetwork.IsConnected, true);

            DevConsole.AddCommand(Command.Create("fly",
                "noclip",
                "Disables collisions so you can fly",Noclip));

            DevConsole.AddCommand(Command.Create("god",
                "",
                "Toggles invincibility for your character", GodMode));

            DevConsole.AddCommand(Command.Create<string>("summon","","Summons a creature", Parameter.Create("entityName","Entity name to create"),SummonCreature));
        }

        DevConsole.OnConsoleOpened += DevConsole_OnConsoleOpened;
        DevConsole.OnConsoleClosed += DevConsole_OnConsoleClosed;

        ClientUI.OnGamePaused += Instance_OnGamePaused;
        ClientUI.OnGameContinued += Instance_OnGameContinued;
    }

    private static void SummonCreature(string creatureName)
    {
        if (!PlayerController.ClientInstance.Enabled)
        {
            return;
        }

        Vector3 spawnPoint = CameraSystem.Instance.MousePos;

        creatureName = creatureName.ToLower();

        foreach (var item in Enum.GetValues(typeof(Enemies)))
        {
            if(item.ToString().ToLower() == creatureName)
            {
                SpawnManager.Instance.SpawnEnemy<EnemyAI>((Enemies)item, spawnPoint, Quaternion.identity);
            }
        }
    }

    private static void Noclip()
    {
        if(!PlayerController.ClientInstance.Enabled)
        {
            return;
        }

        PlayerController.ClientInstance.Value.ToggleClipping();
    }

    private static void GodMode()
    {
        if (!PlayerController.ClientInstance.Enabled)
        {
            return;
        }

        PlayerController.ClientInstance.Value.ToggleGodMode();
    }

    private static void DevConsole_OnConsoleClosed()
    {
        lastConsoleWasOpen = false;
    }

    private static void DevConsole_OnConsoleOpened()
    {
        lastConsoleWasOpen = true;
        ClientUI.Instance.IsPauseMenuOpen = true;
    }

    private static void Instance_OnGamePaused()
    {
        if (lastConsoleWasOpen)
        {
            DevConsole.OpenConsole();
        }
    }
    private static void Instance_OnGameContinued()
    {
        bool before = lastConsoleWasOpen;
        DevConsole.CloseConsole();
        lastConsoleWasOpen = before;
    }

    [DevConsoleCommand("disconnect", "", "Disconnects from current server")]
    public static void Disconnect() => PhotonManager.Instance.LeaveRoom();
    public static void CreateRoom(bool isPublic) => PhotonManager.Instance.CreateRoom(isPublic);
}
