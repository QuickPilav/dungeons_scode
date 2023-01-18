using System.Collections.Generic;
using UnityEngine;

public class MinimapSystem : StaticInstance<MinimapSystem>
{
    public static Sprite M_Enemy { get; private set; }
    public static Sprite M_Player { get; private set; }

    public static Transform CameraFollow { get; private set; }
    public static float Minimap_Y { get => lastMinimapY; }
    private static float lastMinimapY;


    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 25f;

    [SerializeField] private Camera minimapCam;
    [SerializeField] private MinimapMarker markerPrefab;
    [SerializeField] private RectTransform minimapUI;
    [Space]
    [SerializeField] private Sprite m_enemy;
    [SerializeField] private Sprite m_player;

    private Dictionary<string, List<MinimapMarker>> markers = new Dictionary<string, List<MinimapMarker>>();
    private bool initialized;
    private Transform minimapTransform;
    private MinimapSettings settings;

    public void Initialize()
    {
        CameraFollow = transform;
        minimapTransform = minimapCam.transform;
        lastMinimapY = Instance.minimapTransform.position.y;

        SaveSocket.OnSettingsChanged.SubscribeToEvent(UpdateMinimapSettings);
        initialized = true;

        M_Enemy = m_enemy;
        M_Player = m_player;
    }

    private void UpdateMinimapSettings(SettingsSave save)
    {
        this.settings = save.minimapSettings;

        //minimapUI.localScale = settings.size * Vector3.one;
        minimapCam.orthographicSize = Mathf.Lerp(maxZoom, minZoom, Mathf.InverseLerp(0, 1f, settings.zoom));
    }

    private void OnDestroy()
    {
        SaveSocket.OnSettingsChanged.UnsubscribeToEvent(UpdateMinimapSettings);
    }

    private void Update()
    {
        if (!initialized)
            return;

        if (CameraFollow == null)
            return;

        Vector3 targetPosition = CameraFollow.position;
        targetPosition.y = Minimap_Y;
        minimapTransform.position = targetPosition;
    }

    public void RegisterMarker(string markerKey, MinimapMarker mm)
    {
        if (markers.TryGetValue(markerKey, out List<MinimapMarker> values))
        {
            values.Add(mm);
        }
        else
        {
            markers.Add(markerKey, new List<MinimapMarker>());
            markers[markerKey].Add(mm);
        }
    }

    public void SetNewFollowTarget(Transform t)
    {
        CameraFollow = t;
    }

    public void RemoveMarker(string markerKey)
    {
        markers.Remove(markerKey);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rotateWithTarget">Eðer false ise hep kuzeye bakar</param>
    /// <param name="mapIcon">MinimapSystem.Instance.Marker_ yazarark ulaþabilirsiniz</param>
    /// <param name="alwaysOnMap">Haritanýn kýyýsýna dayansýn mý ?</param>
    /// <param name="priority">Ne kadar fazlaysa o kadar yüksekte görünür</param>
    /// <param name="camFollow">Kamera bu objeyi mi takip etsin (oyuncuya verilmesi mantýklý)</param>
    /// <param name="fixedSize">Haritayla birlikte büyüme kapatýlsýn mý?</param>
    public void SpawnMarker(string markerKey, Transform markerOwner, float size, bool rotateWithTarget, Sprite mapIcon, Color color, bool alwaysOnMap, float priority, bool fixedSize, bool camFollow)
    {
        if (!initialized)
            Initialize();

        var spawned = Instantiate(markerPrefab, Vector3.zero, Quaternion.Euler(90, 0, 0));
        spawned.Initialize(minimapCam, markerKey, markerOwner, size, rotateWithTarget, mapIcon, color, alwaysOnMap, priority, fixedSize);
        if (camFollow)
        {
            CameraFollow = markerOwner;
        }
    }
}
