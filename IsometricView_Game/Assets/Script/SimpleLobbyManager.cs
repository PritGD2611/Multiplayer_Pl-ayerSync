using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SimpleLobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public TMP_InputField roomNameInput;
    public Transform roomListParent;
    public GameObject roomListItemPrefab;

    private NetworkRunner runner;
    private List<SessionInfo> currentSessions = new();
    private const int GAME_SCENE_INDEX = 1;
    async void Start()
    {
        runner = gameObject.AddComponent<NetworkRunner>();
        runner.AddCallbacks(this);

        Debug.Log("[Lobby] Joining session lobby...");
        var result = await runner.JoinSessionLobby(SessionLobby.Shared);
        Debug.Log($"[Lobby] Join lobby result: {result.Ok}, Error: {result.ErrorMessage}");
    }

    // ================= CREATE ROOM =================
    public async void CreateRoom()
    {
        string roomName = roomNameInput.text;
        Debug.Log($"[Lobby] Creating room: {roomName}");

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = roomName,
            Scene = SceneRef.FromIndex(GAME_SCENE_INDEX),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        Debug.Log($"[Lobby] Create room result: {result.Ok}, Error: {result.ErrorMessage}");
    }

    // ================= JOIN ROOM =================
    public async void JoinRoom(string roomName)
    {
        Debug.Log($"[Lobby] Joining room: {roomName}");

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = roomName
        });

        Debug.Log($"[Lobby] Join room result: {result.Ok}, Error: {result.ErrorMessage}");
    }

    // ================= REFRESH =================
    public async void RefreshRooms()
    {
        Debug.Log("[Lobby] Refreshing room list...");

        foreach (Transform child in roomListParent)
            Destroy(child.gameObject);

        await runner.JoinSessionLobby(SessionLobby.Shared);
    }

    // ================= SESSION LIST UPDATE =================
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log($"[Lobby] Session list updated: {sessionList.Count} rooms found");
        currentSessions = sessionList;

        foreach (Transform child in roomListParent)
            Destroy(child.gameObject);

        foreach (SessionInfo session in sessionList)
        {
            GameObject item = Instantiate(roomListItemPrefab, roomListParent);
            item.GetComponentInChildren<TMP_Text>().text =
                session.Name + " (" + session.PlayerCount + "/" + session.MaxPlayers + ")";

            string sessionName = session.Name;
            item.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                JoinRoom(sessionName);
            });
        }
    }

    // ================= UNUSED CALLBACKS =================
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}