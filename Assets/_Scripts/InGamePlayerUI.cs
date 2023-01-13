using Steamworks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGamePlayerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private RawImage avatarImage;
    [SerializeField] private TextMeshProUGUI heroText;

    [SerializeField] private Slider healthSlider;

    public void Initialize (PlayerController ply, PlayerClass plyClass, int currentHealth, int maxHealth)
    {
        ply.OnHealthChanged += OnHealthChanged;

        heroText.text = plyClass.ToString();

        OnUpdateCustomProperties(ply);
        OnHealthChanged(currentHealth, maxHealth);
    }

    public void OnUpdateCustomProperties (PlayerController ply)
    {

        var customProps = ply.photonView.Controller.CustomProperties;

        //try
        {
            if (customProps != null && customProps.TryGetValue(PlayerController.STEAM_ID_KEY, out object steamId))
            {
                var csteamId = (CSteamID)steamId;

                string nickname = SteamFriends.GetFriendPersonaName(csteamId);
                nameText.text = nickname;
                
                SteamManager.GetMediumSteamAvatar(csteamId, out Texture2D tex);
                avatarImage.texture = tex;
            }
        }
        /*
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
        */
    }

    private void OnHealthChanged (int currentHealth, int maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }
}
