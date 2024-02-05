using Firebase.Database;
using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestLobby : MonoBehaviour
{
    private Lobby hostLobby;
    public static Lobby currentLobby;
    private string playerName;
    string joinCode;
    public Text text;
    public GameObject playButton;

    DatabaseReference dbReference;
    int count = 0;

    int myTestCode;
    public bool serverOnline = false;

    GameObject voiceChatUI;
    public bool playPressed = false;
    public bool joining = false;
    public static bool playing = false;
    public bool creating = false;
    public bool canJoin = false;
    private async void Start()
    {
        PublicVariables.testlobby = this;
        playing = false;
        PublicVariables.disconnected = false;
        playerName = "UnNamedPlayer" + UnityEngine.Random.Range(10, 99);
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            playButton.SetActive(true);
        }
        else
        {
            playButton.SetActive(true);
        }
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        /*
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        FirebaseDatabase.DefaultInstance
       .GetReference("server").Child("online")
       .ValueChanged += HandleValueChanged;
        */
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        FirebaseDatabase.DefaultInstance
       .GetReference("server").Child("version")
       .ValueChanged += CheckCurrentVersion;

    }
    void HandleValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);

            return;
        }

        count++;

        StartCoroutine(CheckIfOnline());
    }
    void CheckCurrentVersion(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);

            return;
        }
        DataSnapshot snapshot = args.Snapshot;
        if (snapshot.Exists)
        {
            if (snapshot.Value.ToString() == PublicVariables.Version)
            {
                canJoin = true;
            }
            else
            {
                canJoin = false;
            }
        }

    }

    IEnumerator CheckIfOnline()
    {
        yield return new WaitForSeconds(4f);
        if (count > 2)
        {
            if (playPressed)
                JoinLobby();
        }
        else
        {
            if (playPressed)
                CreateLobby();
        }
        StopAllCoroutines();
    }

    public static IEnumerator GetTestCode(Action<int> onCallback)
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        var woodData = reference.Child("server").Child("test").GetValueAsync();
        yield return new WaitUntil(predicate: () => woodData.IsCompleted);

        if (woodData != null)
        {
            DataSnapshot snapshot = woodData.Result;
            onCallback.Invoke(Convert.ToInt32(snapshot.Value));
        }

    }


    [Command]
    private async void CreateLobby()
    {

        if (playing || !canJoin)
        {
            return;
        }
        if (text.text != null) playerName = text.text;
        PublicVariables.username = playerName;

        try
        {
            myTestCode = UnityEngine.Random.Range(0, 99999);
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
            await reference.Child("server").Child("test").SetValueAsync(myTestCode);
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("join code: " + joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData);

            StartCoroutine(GetTestCode((int code) =>
            {
                if (code == myTestCode)
                {
                    Debug.Log("Creating lobby");
                    dbReference.Child("server").Child("id").SetValueAsync(joinCode);
                    NetworkManager.Singleton.StartHost();
                    NetworkManager.Singleton.SceneManager.LoadScene("OpenWorld", LoadSceneMode.Single);
                    DatabaseManager.CreateUser();
                    QuantumConsole.Instance.Deactivate();
                    enabled = false;
                }
                else
                {
                    JoinLobby();
                }
            }));



        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            Debug.Log("ylempi");

        }

        /*
             try
             {

                 string lobbyName = "Server1";
                 int maxPlayers = 10;

                 CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
                 {
                     IsPrivate = false,
                     Player = GetPlayer(),
                     Data = new Dictionary<string, DataObject>
                         {
                             {"ServerCode",new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                         }
                 };

                 Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
                 hostLobby = lobby;
                 currentLobby = lobby;
                 StartCoroutine(Timer());
                 Debug.Log("Lobby Created! " + lobby.Name + " " + lobby.MaxPlayers);
                 PrintPlayers(hostLobby);
                 NetworkManager.Singleton.SceneManager.LoadScene("OpenWorld", LoadSceneMode.Single);
                 VivoxVoiceManager.Instance.Login(playerName);
                 DatabaseManager.CreateUser();
                 QuantumConsole.Instance.Deactivate();


             }
             catch (LobbyServiceException e)
             {
                 Debug.LogError(e);
                 Debug.Log("alempi");
             }
        */

    }
    IEnumerator Timer()
    {
        yield return new WaitForSeconds(6f);

        if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("ping sent");
            SendHeartbeat();
            StartCoroutine(Timer());

        }
    }
    async void SendHeartbeat()
    {
        await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
    }

    [Command]
    private async void ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();



            Debug.Log("Lobbies found:" + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {

                Debug.Log(lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Data["ServerCode"].Value);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }
    [Command]
    private void JoinLobby()
    {
        if (playing || !canJoin)
        {
            return;
        }
        if (text.text != null)
            playerName = text.text;
        PublicVariables.username = playerName;



        /*
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions { Player = GetPlayer() };
            Debug.Log("Lobbies found:" + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
            Lobby joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id, joinLobbyByIdOptions);
            currentLobby = joinedLobby;
            joinCode = joinedLobby.Data["ServerCode"].Value;

            //PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
            if (e.Reason == LobbyExceptionReason.RateLimited)
            {
                playButton.SetActive(true);
                Debug.Log("Too many requests");
                return;
            }
            
        }
        */
        StartCoroutine(GetId((string code) =>
        {
            NetworkManager.Singleton.Shutdown();
            joinCode = code;
            joinServer(code);
        }));


    }
    async void joinServer(string code)
    {
        try
        {

            Debug.Log("Joining with " + code);
            JoinAllocation joinAllocation;
            StartCoroutine(Delay());
            joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);
            //SceneManager.LoadSceneAsync("OpenWorld");



            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
            joinAllocation.RelayServer.IpV4,
            (ushort)joinAllocation.RelayServer.Port,
            joinAllocation.AllocationIdBytes,
            joinAllocation.Key,
            joinAllocation.ConnectionData,
            joinAllocation.HostConnectionData

           );
            try
            {

                NetworkManager.Singleton.StartClient();
                enabled = false;

            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
                CreateLobby();
            }




            DatabaseManager.CreateUser();
            QuantumConsole.Instance.Deactivate();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            StopAllCoroutines();

            if (e.Reason == RelayExceptionReason.JoinCodeNotFound)
            {
                CreateLobby();
            }


        }
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(5);
        if (!PublicVariables.inGame)
        {
            StartCoroutine(CheckIfOnline());
        }
    }
    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in lobby: " + lobby.Name + " " + lobby.Data["ServerCode"].Value);
        joinCode = lobby.Data["ServerCode"].Value;
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                    }
        };
    }

    public void Play()
    {
        playButton.SetActive(false);

        playPressed = true;
        JoinLobby();

    }

    public static IEnumerator GetId(Action<string> onCallback)
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        var idData = reference.Child("server").Child("id").GetValueAsync();
        yield return new WaitUntil(predicate: () => idData.IsCompleted);

        if (idData != null)
        {
            DataSnapshot snapshot = idData.Result;
            onCallback.Invoke(snapshot.Value.ToString());
        }

    }

    IEnumerator TryToCreateServer()
    {
        yield return new WaitForSeconds(2);
        if (!serverOnline)
        {

            CreateLobby();
        }
        else
        {
            joining = false;
        }
    }

    private void Update()
    {
        /*
        if(playPressed && !playing)
        {
            playButton.SetActive(false);
            if(serverOnline)
            {
                if (!joining)
                {
                    joining = true;
                    JoinLobby();
                }
            }
            else
            {
                if(!creating)
                {
                    creating = true;
                    StartCoroutine(TryToCreateServer());

                }
            }
        }
        if (playing) enabled = false;
        */
    }
    private void OnDisable()
    {
        StopAllCoroutines();

        FirebaseDatabase.DefaultInstance
        .GetReference("server")
        .ValueChanged -= HandleValueChanged;
    }


}
