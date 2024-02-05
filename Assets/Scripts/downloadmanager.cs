using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class downloadmanager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string url;
    public string name;
    public static string savePath;
    public bool canPing = true;

    // Start is called before the first frame update
    void Start()
    {

        //downloadvideo();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void downloadvideo(string name)
    {
        StartCoroutine(DownloadVideo(name));
    }
    void downloadvideowithurl(string url)
    {
        StartCoroutine(DownloadVideo2(url));
    }

    IEnumerator DownloadVideo(string name)
    {

        UnityEngine.Debug.Log("started");
        savePath = string.Format("{0}/{1}.mp4", UnityEngine.Application.persistentDataPath, name);
        if (System.IO.File.Exists(savePath))
        {
            UnityEngine.Debug.Log("File exists");
            videoPlayer.url = savePath;
            PublicVariables.serverRpcs.videoDownloadedServerRpc(PublicVariables.username, PublicVariables.myOwnerClientId);
            //videoPlayer.Play();
            //Enter serverrpc here to inform client has the file and comment play command
            yield break;
        }


        url = GetVideoLink(name);
        UnityWebRequest www = UnityWebRequest.Get(url);
        if (canPing)
        {
            canPing = false;
            StartCoroutine(ping());

        }

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            UnityEngine.Debug.Log(www.error);
        }
        else
        {

            System.IO.File.WriteAllBytes(savePath, www.downloadHandler.data);
            //Enter serverrpc here to inform client has the file and comment play command
            UnityEngine.Debug.Log("downloaded to " + savePath);
            videoPlayer.url = savePath;
            PublicVariables.serverRpcs.videoDownloadedServerRpc(PublicVariables.username, PublicVariables.myOwnerClientId);

            //videoPlayer.Play();
        }
    }

    IEnumerator DownloadVideo2(string url)
    {
        int random = Random.Range(0, 999999999);
        savePath = string.Format("{0}/{1}.mp4", UnityEngine.Application.persistentDataPath, random.ToString());

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            UnityEngine.Debug.Log(www.error);
        }
        else
        {

            System.IO.File.WriteAllBytes(savePath, www.downloadHandler.data);
            UnityEngine.Debug.Log("downloaded to " + savePath);
            videoPlayer.url = savePath;
            PublicVariables.serverRpcs.videoDownloadedServerRpc(PublicVariables.username, PublicVariables.myOwnerClientId);

            //videoPlayer.Play();
            //Enter serverrpc here to inform client has the file and comment play command
        }
    }
    IEnumerator ping()
    {
        yield return new WaitForSeconds(2);
        UnityEngine.Debug.Log("downloading");
        canPing = true;
    }

    string GetVideoLink(string name)
    {
        if (name.Contains("spongebob"))
        {
            if (name.Contains("0000"))
            {
                return "https://ww.megacartoons.net/video/SpongeBob-SquarePants-Squeaky-Boots.mp4";
            }
            if (name.Contains("0001"))
            {
                return "https://ww.megacartoons.net/video/SpongeBob-SquarePants-Culture-Shock.mp4";
            }
            if (name.Contains("0002"))
            {
                return "https://ww.megacartoons.net/video/SpongeBob-SquarePants-MuscleBob-BuffPants.mp4";
            }
            if (name.Contains("0003"))
            {
                return "https://ww.megacartoons.net/video/SpongeBob-SquarePants-Imitation-Krabs.mp4";
            }
            if (name.Contains("0004"))
            {
                return "https://ww.megacartoons.net/video/SpongeBob-SquarePants-Something-Smells.mp4";
            }
            if (name.Contains("0005"))
            {
                return "https://ww.megacartoons.net/video/SpongeBob-SquarePants-Your-Shoes-Untied.mp4";
            }
            if (name.Contains("0006"))
            {
                return "https://ww.megacartoons.net/video/SpongeBob-SquarePants-Arrgh.mp4";
            }

            if (name.Contains("0007"))
            {
                return "https://ww.megacartoons.net/video/SpongeBob-SquarePants-Life-of-Crime.mp4";
            }
            return "";


        }
        else if (name.Contains("southpark"))
        {
            UnityEngine.Debug.Log("southpark");
            if (name.Contains("1001"))
            {
                return "https://asmassets.mtvnservices.com/asm/mtv_international/alberto/2018/sp_1413_HD_intro_512x288_450.mp4";
            }
            if (name.Contains("1002"))
            {
                return "https://asmassets.mtvnservices.com/asm/mtv_international/alberto/2018/sp_1413_HD_intro_512x288_450.mp4";
            }
            if (name.Contains("1003"))
            {
                return "";
            }
            if (name.Contains("1004"))
            {
                return "";
            }
            if (name.Contains("1005"))
            {
                return "";
            }
            if (name.Contains("1006"))
            {
                return "";
            }
            if (name.Contains("1007"))
            {
                return "";
            }
            if (name.Contains("1008"))
            {
                return "";
            }
            if (name.Contains("1009"))
            {
                return "";
            }
            if (name.Contains("1010"))
            {
                return "";
            }
            //season 11

            if (name.Contains("1101"))
            {
                return "https://public.am.files.1drv.com/y4mkhEtT6H51wZOHIBHSbESNpS0kp6DDno2rTEm5XsOey9FSx9ryvErZjLAstXqih23-rQJ2T90ZfiaTg0MxChteIlpaIvE_5qoA9XOgvtqLcVDKAbvfg3WjvAuURykUqllcqlvP_5sH9I0brg-SegRvc1U8hQ1957UlwenmVdAoQeqm8v-Pc8I6IRtWbhE9FRMoePsmNZoCqp7TQBIqwWHJpjSffJaIssPAQXTkmBtwtc?AVOverride=1";
            }
            if (name.Contains("1102"))
            {
                return "";
            }
            if (name.Contains("1103"))
            {
                return "";
            }
            if (name.Contains("1104"))
            {
                return "";
            }
            if (name.Contains("1105"))
            {
                return "";
            }
            if (name.Contains("1106"))
            {
                return "";
            }
            if (name.Contains("1107"))
            {
                return "";
            }
            if (name.Contains("1108"))
            {
                return "";
            }
            if (name.Contains("1109"))
            {
                return "";
            }
            if (name.Contains("1110"))
            {
                return "";
            }

            //Season 15
            if (name.Contains("1501"))
            {
                return "https://onedrive.live.com/embed?cid=935D260DD19C7C94&resid=935D260DD19C7C94%21131375&authkey=ANNUp10NuDSFPWM";
            }

            if (name.Contains("1502"))
            {
                return "https://onedrive.live.com/embed?cid=935D260DD19C7C94&resid=935D260DD19C7C94%21131376&authkey=AJQGlhUt2sUWGfc";
            }

            if (name.Contains("1503"))
            {
                return "https://onedrive.live.com/embed?cid=935D260DD19C7C94&resid=935D260DD19C7C94%21131377&authkey=ALOlwmO1fVTSiDY";
            }
            if (name.Contains("1504"))
            {
                return "https://onedrive.live.com/embed?cid=935D260DD19C7C94&resid=935D260DD19C7C94%21131378&authkey=ANc3wnvsPDH3ELM";
            }
            if (name.Contains("1505"))
            {
                return "https://public.am.files.1drv.com/y4mLFWV35wSPRqTd4vH27RwKUbfnM6o3x7uREY4wzZ7G7pubqYxUPlabxpwufN_37nAsYtcfQX3gNclrJhJCx8OWDvVr4Dd3NacShDAxtmAn4QvDY8QXLhCAAOZNZhsQK2kZMZJvM5A0VLQZaVhrXc3g3XBZAr9OE1pq5ULEAzUbD4JFU3eR359HgUf2X5O4Yy0J13MxLiQjkqSQOCREi1k6JRi5Pj9-Xws221JWp2v6oM?AVOverride=1";
            }
            if (name.Contains("1506"))
            {
                return "https://public.am.files.1drv.com/y4m79klV2KFs0-Q6pFaCIb2TF1BWX_bdB1oM6FG4MdZlQpWw_dEjiUVUMUohle3IHkUlebe0Jo-xIeDERm9oBr4aFcaFzxRdHaWglTv3-nfDLFF2xb8gN0WvohU4wnt5wTM7_m_gfeInlkt0L7tWA1ye1NeMwlAn5J6nlMfY1zV8S6I84bQadpxgT3Ecld4RifdxLrqLf1Zg6H0pmQEEEDx_nSOXp7dqxESiorWwD_Mqlw?AVOverride=1";
            }
            if (name.Contains("1507"))
            {
                return "https://public.am.files.1drv.com/y4mNs_k0im_Qe8AKr7E6JLmINsHSd-KWc_r79yrM86RUqVV51leunhxuEAyvEy5CGW2w9PkHEACd4UQg0ckLv1fBRYm0x1p5iLuD1lSRJY0UR0lr5N-CrsMFW1iTG4UtjtMIAR2hBkRQWzGUDzPWmJZBmLKRREycIOqT7zk64axg7YWXwM-EbcUy-qAPjIb3ANbcMOKCF4fQQHlw5UWzdaE-LWwimphHvoyKMy0xxjyrx0?AVOverride=1";
            }
            if (name.Contains("1508"))
            {
                return "https://public.am.files.1drv.com/y4mLTW4aqSWOqDW4-oIPKZEgVG4dTb8MMNhoezdlW0qM1yfJd46MCbha8wNjVfssrbPHdzjCKn1IJBKk8S7-YNEgLR0hqdmKN1O7ZDEpOk5BmnkMI-ClTNVwJXV2eNFFHNu2iNoAsFj74rhffcNs_udQFKcRxmYNETfBg67u6ULUSI55BwbiTLxG99_HLNW7nvPfKbS-URI8slOeq1wcsLozKIGmJhg1G0Ar9cJxN7jS1k?AVOverride=1";
            }
            if (name.Contains("1509"))
            {
                return "https://public.am.files.1drv.com/y4m_WtNw6Bml10dIHxa1Ysr3Dm9yh488gqutR33KCuG3CE4V17pv_Ut245qTSnSEqTxp_tcMIJa0Lj7N5T1u8-DjlJ9mP68ZECKXnmafE3fXLpHGLNNxSnn8bEvIuxIMkkxGmL4QIf8qVKvAqm-_pCPFb0DG96Wh7mfKO4Whn6Mylv5r8KZ4l5QQmHDKyAR5UmmhBOnrfWHUhHL8TS2Rk4mgXj8K5FvEFEATh88ZGETFmU?AVOverride=1 ";
            }
            if (name.Contains("1510"))
            {
                return "https://public.am.files.1drv.com/y4m7xtsYQ2CNlseDrx49BIqTkOZ-TTSKrlEyzKX1TTjRvntf8wpbsaqIFz7Z3iK2TpPws-Tk6GVYapiaeM4wLZ93Lk3pRoSAmGI2vr4oos8vbYN0vc58bUzKafWvQXtnY8VRr8glRtAgzZ9pJP8ccGHb_iQCa_IBlRQbbZ1gTGJvrcwnlvfpN41lWXWilv3fKMo5zasHo4sP7B9BuZT11KcRwMBw1m95NqRD4-sZeL5lnk?AVOverride=1";
            }
            if (name.Contains("1511"))
            {
                return "https://public.am.files.1drv.com/y4mlbQIYXkzYueZhxwDlGod_KjjOuC14nXlTk4xZE3zwJDmUmCFAaQ51966K3xBA404ZBnYhskj3CDm7e6G2dSjai-3cc0CC_CcUqkty_4HwW4l3H6CparYF5SHPVDIdLxl620gqkWR_GvqxVoffJV8J5kJC0x6Xz0yklhzF5jgQd159VST3yF69IX_tvoP6gA5S-h5YpcFHHcqzkBdKkelV0PgROzAjyjuinORUkOXxKM?AVOverride=1";
            }
            if (name.Contains("1512"))
            {
                return "https://public.am.files.1drv.com/y4mdX0kfYJGgNQBbSMS-rnn3Wx-1rSYsu9Z6-WwLvtGJTYgeyHjx93NTh5Kr36_63xC8hhHAQS4nGQfk2VqeD-Xk0f8AepakQEKCUdYLO6Cyrz5I7PD7hFMqIjusSrtBZqNAQWGBfYkNqH-ArJQ3TMWv-I01ocwcllDolk_gxfzXtZj4W3iZ3Wj0DRNs5koMLWyhNtJJlKEcyVr0CxynNnseJ0Iv0D6O3vtbqGN3lxMKFM?AVOverride=1";
            }
            if (name.Contains("1513"))
            {
                return "https://public.am.files.1drv.com/y4mB_UD0W46x8U-3TWzhMcsGugSpeqQroQE9-wHFA4chysRDUIyNcrPHmEAtkFOEZ-VUGQt_hc9wQvu4YPttGDrHFCLAkMraAhcY3APG7w_C3rKLDadB0CH6dSySLXNgNaxNXvoIO-QztEli-lgfqu3qWgUR-jgJ8BzzGLrTTUmMEQhjeRl0T6aRs8asJGRnAfo31KQkA7wafYCQpi_9blS_bxfjvMwG4wNZVx9EEuc9v0?AVOverride=1";
            }
            if (name.Contains("1514"))
            {
                return "https://public.am.files.1drv.com/y4mkuwlq8_fiDC2aBrAbkuGDCCTye3Zbxy35DBbdg5bADBmxFhUx7ThvKmmkL0HjDOrZQAuJm1bz5qmfBM06jWWd3KoYe-KcbKlj5sKIIB0pktau8X_D6aaR3um0oh5G3kP3QC4FV63ofKl0ylp5ESHGspRISh0cZtEcQ_h1XWgq-NUHox_8IR9cNoxF-kJ5EapcdchjFS2gwlBTVBMeVvVSFDj9sv8YeQIRJpEP0eAlrk?AVOverride=1";
            }

        }
        return "";
    }
}
