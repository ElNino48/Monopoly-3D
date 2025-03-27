using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class Skills : MonoBehaviour
{
    public enum SkillType
    {
        Economist,
        Strateg,
        Influencer,
        Speculant,
        Master,
        Industrialist,
        Corporate,
        Builder
    }

    public SkillType skillType;
    private void Start()
    {
        economistSkillDescriptionText.text = "Экономист \n Уменьшение издержек на строительство на 10% для каждого здания";
        strategSkillDescriptionText.text = "Стратeг \n Разыграть дополнительную карту события";
        influencerSkillDescriptionText.text = "Инфлюенсер \n Увеличивает доход от аренды других игроков на 10%";
        speculantSkillDescriptionText.text = "Спекулянт \n Получение процента со сделок других игроков";
        masterSkillDescriptionText.text = "Мастер \n Может строить уникальное улучшение зданий, которые повышают ценность на 20%";
        industrialistSkillDescriptionText.text = "Промышленник \n Получает 5% дополнительного дохода со всех улиц промышленных районов";
        corporateSkillDescriptionText.text = "Корпорат \n Получает 5% дополнительного дохода со всех улиц непромышленных районов";
        builderSkillDescriptionText.text = "Строитель \n Может строить два здания за ход";
    }
    [Header("Экономист")]
    [SerializeField] TMP_Text economistSkillDescriptionText;
    [SerializeField] public bool isEconomistSkillActive;

    [Header("Стратeг")]
    [SerializeField] TMP_Text strategSkillDescriptionText;
    [SerializeField] public bool isStrategSkillActive;

    [Header("Инфлюенсер")]
    [SerializeField] TMP_Text influencerSkillDescriptionText;
    [SerializeField] public bool isInfluencerSkillActive;
    [SerializeField] public float rentIncreasePercentage = 0.1f;

    [Header("Спекулянт")]
    [SerializeField] TMP_Text speculantSkillDescriptionText;
    [SerializeField] public bool isSpeculantSkillActive;
    [SerializeField] internal int speculantPercentage;

    [Header("Мастер")]
    [SerializeField] TMP_Text masterSkillDescriptionText;
    [SerializeField] public bool isMasterSkillActive;
    [SerializeField] internal int masterPercentage;

    [Header("Промышленник")]
    [SerializeField] TMP_Text industrialistSkillDescriptionText;
    [SerializeField] public bool isIndustrialistSkillActive;
    [SerializeField] internal int industrialistPercentage;

    [Header("Корпорат")]
    [SerializeField] TMP_Text corporateSkillDescriptionText;
    [SerializeField] public bool isCorporateSkillActive;
    [SerializeField] internal int corporatePercentage;

    [Header("Строитель")]
    [SerializeField] TMP_Text builderSkillDescriptionText;
    [SerializeField] public bool isBuilderSkillActive;

    public void ApplySkill(Player player)
    {
        switch (skillType)
        {
            case SkillType.Economist: 
                Debug.Log("Economist skills applied(under development)");
                break;
            case SkillType.Strateg:
                Debug.Log("Strateg skills applied(under development)");
                break;
            case SkillType.Influencer:
                player.ApplyRentBonus(rentIncreasePercentage);
                Debug.Log("INFLUENCER skills applied : +" + rentIncreasePercentage * 100 + "%");
                break;
            case SkillType.Speculant:
                Debug.Log("Speculant skills applied(under development)");
                break;
            case SkillType.Master:
                Debug.Log("Master skills applied(under development)");
                break;
            case SkillType.Industrialist:
                Debug.Log("Industrialist skills applied(under development)");
                break;
            case SkillType.Corporate:
                Debug.Log("Corporate skills applied(under development)");
                break;
            case SkillType.Builder:
                Debug.Log("Builder skills applied(under development)");
                break;

        }
    }
    public void RemoveSkill(Player player)
    {
        switch (skillType)
        {
            case SkillType.Influencer:
                player.RemoveRentBonus(rentIncreasePercentage);
                Debug.Log("INFLUENCER skills removed: " + rentIncreasePercentage * 100 + "%");
                break;
                // Остальные skill types дальше:
        }
    }
}
