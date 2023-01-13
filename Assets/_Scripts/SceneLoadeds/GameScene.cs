using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

public class GameScene : SceneLoadedHandler
{
    public static WaitForSeconds OneSecondWaiter = new WaitForSeconds(1);
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Material wallMat;

    [SerializeField] private AudioClip[] fireClips;

    /// <summary>pos, magnitude</summary>
    public Action<Vector3, float> WorldExplosionEvent;

    public AudioClip GetRandomFireClip() => fireClips[UnityEngine.Random.Range(0, fireClips.Length)];

    public Transform[] SpawnPoints { get => spawnPoints; }

    protected override bool subscribeToGameSceneLoaded => true;

    public Material WallMaterial { get => wallMat; }

    protected override void OnGameSceneLoaded()
    {
        base.OnGameSceneLoaded();
        ImReady();

        //reset wall material, so we don't see behind walls
        wallMat.SetVector(CircleScript.posId, Vector3.zero);
        wallMat.SetFloat(CircleScript.sizeId, 0);
    }

    public void ImReady()
    {
        HeroSelectorUI.Instance.OnHeroSelected += SpawnPlayer;
    }

    private void SpawnPlayer()
    {
        PhotonManager.Instance.SpawnPlayer();
    }

    private void OnDrawGizmos()
    {
        if (spawnPoints != null)
        {
            foreach (var item in spawnPoints)
            {
                if (item == null)
                    continue;

                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(item.position, 0.2f);
            }
        }

    }

    public void StartCountdown(int countdownFrom)
    {
        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

            var customProperties = PhotonNetwork.CurrentRoom.CustomProperties;

            for (int i = 0; i <= countdownFrom; i++)
            {
                InGameUI.Instance.ShowCountdown(countdownFrom - i);
                //Debug.Log($"{countdownFrom - i} saniye sonra oyun baþlayacak!");

#if !UNITY_EDITOR
                yield return OneSecondWaiter;
#endif
                if (PhotonNetwork.IsMasterClient && playerCount != PhotonNetwork.CurrentRoom.PlayerCount)
                {
                    //Debug.Log("Yeni biri geldi ya da çýktý!");

                    customProperties[PhotonManager.GAME_STARTING_KEY] = false;
                    PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);

                    InGameUI.Instance.ShowCountdown(-1);

                    StartCoroutine(PhotonManager.Instance.WaitForAllPlayersGetReady());
                    yield break;
                }
            }

            InGameUI.Instance.ShowCountdown(-1);

            if (PhotonNetwork.IsMasterClient)
            {
                customProperties[PhotonManager.GAME_STARTING_KEY] = false;
                customProperties[PhotonManager.GAME_STARTED_KEY] = true;
                PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
            }

            StartGame();
        }
    }

    public void StartGame()
    {
        PlayerController.ClientInstance.Value.GameStarted = true;
    }
}
