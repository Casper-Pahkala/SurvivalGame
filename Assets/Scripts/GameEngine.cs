using QFSW.QC;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEngine : MonoBehaviour
{
    public GameObject axeAirHitSound;
    public GameObject axeWoodHitSound;
    public GameObject resourceNotification;
    public float offset;
    public float distance;
    public Transform notifications;
    bool soundPlayed = false;
    public float liftedFloat;
    int notificationCount = 0;
    private VivoxVoiceManager _vivoxVoiceManager;
    public ServerRpcs ServerRpcs;
    bool muted = false;
    bool muted2 = false;
    public static Transform m_Notifications;

    bool connected = false;



    void Start()
    {
        StartCoroutine(DatabaseManager.GetWood((int woodCountDB) =>
        {
            PublicVariables.woodCount = woodCountDB;
            Debug.Log(PublicVariables.woodCount);
        }));


        PublicVariables.gameEngine = GetComponent<GameEngine>();

        QuantumConsole.Instance.Deactivate();



    }

    // Update is called once per frame
    void Update()
    {


        if (NetworkManager.Singleton.IsConnectedClient)
        {
            connected = true;
        }

        if (!NetworkManager.Singleton.IsConnectedClient && connected)
        {
            connected = false;
            Debug.Log("Connection to host lost");
            //disconnect();
        }

    }
    [Command]
    private void muteMe()
    {
        muted = !muted;
        GameObject[] rosters = GameObject.FindGameObjectsWithTag("Roster");
        foreach (GameObject roster in rosters)
        {
            if (roster.GetComponent<RosterItem>().Participant.IsSelf)
            {
                roster.GetComponent<RosterItem>().IsMuted = muted;
            }
        }
    }

    [Command]
    private void muteAll()
    {
        muted2 = !muted2;
        GameObject[] rosters = GameObject.FindGameObjectsWithTag("Roster");
        foreach (GameObject roster in rosters)
        {
            if (!roster.GetComponent<RosterItem>().Participant.IsSelf)
            {
                roster.GetComponent<RosterItem>().IsMuted = muted2;
            }
        }
    }

    public async void disconnect()
    {
        try
        {
            PublicVariables.disconnected = true;

            NetworkManager.Singleton.Shutdown();
            string playerId = AuthenticationService.Instance.PlayerId;
            VivoxVoiceManager _vivoxVoiceManager = VivoxVoiceManager.Instance;
            _vivoxVoiceManager.DisconnectAllChannels();
            _vivoxVoiceManager.Logout();
            await AuthenticationService.Instance.DeleteAccountAsync();
            AuthenticationService.Instance.ClearSessionToken();
            PublicVariables.inGame = false;
            SceneManager.LoadScene("Lobby");

            Debug.Log("Disconnected");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

}
