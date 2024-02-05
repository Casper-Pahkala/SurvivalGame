using QFSW.QC;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    private Toggle m_MenuToggle;
    private float m_TimeScaleRef = 1f;
    private float m_VolumeRef = 1f;
    private bool m_Paused;
    RosterItem myRoster;
    bool muted = false;
    bool muted2 = false;

    public GameObject menu;



    void Awake()
    {
        m_MenuToggle = GetComponent<Toggle>();

    }


    private void MenuOn()
    {
        m_TimeScaleRef = Time.timeScale;
        Time.timeScale = 0f;

        m_VolumeRef = AudioListener.volume;
        AudioListener.volume = 0f;

        m_Paused = true;
    }


    public void MenuOff()
    {
        Time.timeScale = m_TimeScaleRef;
        AudioListener.volume = m_VolumeRef;
        m_Paused = false;
    }


    public void OnMenuStatusChange()
    {
        if (m_MenuToggle.isOn && !m_Paused)
        {
            MenuOn();
        }
        else if (!m_MenuToggle.isOn && m_Paused)
        {
            MenuOff();
        }
    }
    public void Continue()
    {
        m_MenuToggle.isOn = !m_MenuToggle.isOn;
        QuantumConsole.gamePaused = m_MenuToggle.isOn;
        Cursor.visible = m_MenuToggle.isOn;
        menu.SetActive(m_MenuToggle.isOn);
        if (m_MenuToggle.isOn)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            QuantumConsole.Instance.Deactivate();
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    [Command]
    public async void disconnect()
    {
        try
        {
            PublicVariables.playerNetworkHandler.sendPlayerData();
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


#if !MOBILE_INPUT
    void Update()
    {
        if (Loot.lootOpen)
        {
            return;
        }
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.F1))
        {
            m_MenuToggle.isOn = !m_MenuToggle.isOn;
            QuantumConsole.gamePaused = m_MenuToggle.isOn;
            Cursor.visible = m_MenuToggle.isOn;
            menu.SetActive(m_MenuToggle.isOn);
            if (m_MenuToggle.isOn)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                QuantumConsole.Instance.Deactivate();
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        if (Input.GetKeyDown(KeyCode.N))
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
        if (Input.GetKeyDown(KeyCode.M))
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
    }
#endif

}
