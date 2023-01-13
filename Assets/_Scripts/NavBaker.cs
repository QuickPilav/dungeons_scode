using RoboRyanTron.QuickButtons;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class NavBaker : MonoBehaviour
{
    [SerializeField] private GameObject[] kapatilacaklar;

    public QuickButton Kapat = new QuickButton(nameof(Bake));
    public QuickButton Ac = new QuickButton(nameof(Bake2));

    [ContextMenu("kapat")]
    public void Bake ()
    {
        foreach (var item in kapatilacaklar)
        {
            item.SetActive(false);
        }
    }

    [ContextMenu("aç")]
    public void Bake2()
    {
        foreach (var item in kapatilacaklar)
        {
            item.SetActive(true);
        }
    }
}
