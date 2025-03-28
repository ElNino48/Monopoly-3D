using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SkillManager : MonoBehaviour
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

    public Player player; // Ссылка на объект игрока
    public GameObject skillPanel; // Ссылка на UI-панель выбора навыков
    [SerializeField] List<Skill> skills = new List<Skill>(); // Список доступных навыков (SkillBoxes)

    void Start()
    {
        skillPanel.SetActive(false); // Скрываем панель навыков при старте
        foreach (Skill skill in skills)
        {
            skill.OnGetSkill += AddSkill;
        }
    }

    // Метод для открытия панели выбора навыков
    public void OpenSkillPanel()
    {
        skillPanel.SetActive(true);
    }

    void AddSkill(Skill skill)
    {
        skill.isSkillActive = true;

        Player player = GameManager.instance.GetCurrentPlayer;
        Debug.Log("player = " + player.nickname + "   skill = "+ skill.SkillType);
        AddSkill(player, skill);
    }

    public void AddSkill(Player player, Skill skill)
    {
        if (player.Skills.Any((playerSkill) => playerSkill.SkillType == skill.SkillType))
        {
            Debug.Log("Skill already applied! ");
            return;
        }
        //Если среди скиллов игрока найдется skilltype такой же, как у добавляемого навыка, то нужно return чтобы не применять навыки дважды
        player.Skills.Add(skill);
        string debugMessage = $"{skill.name} skill applied";

        switch (skill.SkillType)
        {
            case SkillType.Economist:
                Debug.Log(debugMessage + " (under development)");
                break;
            case SkillType.Strateg:
                Debug.Log(debugMessage + " (under development)");
                break;
            case SkillType.Influencer:
                player.RentBonus += skill.SkillModifier;
                //бонус - количество %, добавляемое к 100% ренты (+0.1 = +10%)
                Debug.Log($"{skill.name} skill applied: Rent bonus ({skill.SkillModifier * 100}%)");
                break;
            case SkillType.Speculant:
                Debug.Log(debugMessage + " (under development)");
                break;
            case SkillType.Master:
                Debug.Log(debugMessage + " (under development)");
                break;
            case SkillType.Industrialist:
                Debug.Log(debugMessage + " (under development)");
                break;
            case SkillType.Corporate:
                Debug.Log(debugMessage + " (under development)");
                break;
            case SkillType.Builder:
                Debug.Log(debugMessage + " (under development)");
                break;
        }
    }

    public void RemoveSkill(Player player, Skill skill)
    {
        player.Skills.Remove(skill);

        switch (skill.SkillType)
        {
            case SkillType.Influencer:
                player.RentBonus -= skill.SkillModifier;
                Debug.Log($"{skill.name} skill removed");
                break;
        }
    }
}