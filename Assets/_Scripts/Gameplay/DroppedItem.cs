using Photon.Pun;
using System.Collections;
using UnityEngine;

public class DroppedItem : ClickableButton
{
    private ItemSystem.Items item;
    private int currentBullets;
    private int bulletsLeft;

    private string itemName_English;
    private string itemName_Turkish;

    private bool initialized;

    private bool noAmmo;

    public override bool CantInteractWhen => base.CantInteractWhen || !initialized || noAmmo;

    public override int InteractionPriority => 6;

    protected override bool OnClicked (PlayerController ply)
    {
        if (!ply.Inventory.AddItem(item,out _, currentBullets, bulletsLeft))
            return false;

        initialized = false;
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
            return true;
        }

        photonView.RPC(nameof(DestroyViaOwnerRpc), photonView.Owner);
        return true;
    }

    public override string GetPrompt (Language language)
    {
        string itemName = language == Language.English ? itemName_English : itemName_Turkish;
        string additiveBulletsText = currentBullets == -1 ? string.Empty : $"{currentBullets} / {bulletsLeft} ";
        return $"{base.GetPrompt(language)} {itemName} {additiveBulletsText}";
    }

    [PunRPC]
    public void DestroyViaOwnerRpc ()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    public override void OnPhotonInstantiate (PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;

        this.item = (ItemSystem.Items)data[0];
        this.currentBullets = (int)data[1];
        this.bulletsLeft = (int)data[2];

        noAmmo = currentBullets == 0 && bulletsLeft == 0;
        
        if (ResourceManager.InventoryScriptableBases.TryGetValue(item.ToString(), out var value))
        {
            itemName_English = value.itemName_English;
            itemName_Turkish = value.itemName_Turkish;
        }
        else
        {
            itemName_English = item.ToString();
            itemName_Turkish = item.ToString();
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        transform.GetChild((int)item).gameObject.SetActive(true);

        var rb = GetComponent<Rigidbody>();
        if (!PhotonNetwork.IsMasterClient)
        {
            rb.isKinematic = true;
        }
        else
        {
            rb.velocity = (transform.forward + transform.up).normalized * Random.Range(3f, 5f);
            rb.angularVelocity = Mathf.Sign(Random.Range(-1f,1f)) * Random.Range(15f, 20f) * transform.right + transform.forward * Random.Range(-5f,5f);

            if(bulletsLeft == 0 && currentBullets == 0)
            {
                Debug.Log("YERE ATILAN SÝLAHIN MERMÝSÝ BÝTMÝÞ, SÝLÝNÝYOR");
                StartCoroutine(enumerator());

                IEnumerator enumerator ()
                {
                    yield return new WaitForSeconds(5f);
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }

        initialized = true;
    }
}
