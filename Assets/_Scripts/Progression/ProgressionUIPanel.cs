using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ProgressionUIPanel
{
    [SerializeField] private Slider progressionSlider;
    [SerializeField] private TextMeshProUGUI minXPText;
    [SerializeField] private TextMeshProUGUI maxXPText;
    [SerializeField] private TextMeshProUGUI currentXPText;

    public void Initialize()
    {
        ProgressionSystem.OnExperienceUpdated.SubscribeToEvent(OnExperienceUpdated);
    }

    public void Dispose()
    {
        ProgressionSystem.OnExperienceUpdated.UnsubscribeToEvent(OnExperienceUpdated);
    }

    private void OnExperienceUpdated(ProgressionSystem.VisualLevel vl)
    {
        currentXPText.text = $"{vl.currentLevelString}.LV {vl.loadedXp}xp";
        minXPText.text = vl.currentLevel.xpRequired.ToString();
        maxXPText.text = vl.nextLevel.xpRequired.ToString();

        progressionSlider.maxValue = vl.nextLevel.xpRequired - vl.currentLevel.xpRequired;
        progressionSlider.value = vl.remainingXp;
    }
}
