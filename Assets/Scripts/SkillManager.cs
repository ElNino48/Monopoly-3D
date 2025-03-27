using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    public Player player; // ������ �� ������ ������
    public GameObject skillPanel; // ������ �� UI-������ ������ �������
    public List<Skills> availableSkills = new List<Skills>(); // ������ ��������� ������� (�������)
    [SerializeField] GameObject economistBox;//1
    [SerializeField] GameObject strategBox;//2
    [SerializeField] GameObject influencerBox;//3
    [SerializeField] GameObject speculantBox;//4
    [SerializeField] GameObject masterBox;//5
    [SerializeField] GameObject industrialistBox;//6
    [SerializeField] GameObject corporateBox;//skill7
    [SerializeField] GameObject builderBox;//skill8

    void Start()
    {
        skillPanel.SetActive(false); // �������� ������ ������� ��� ������
    }

    // ����� ��� �������� ������ ������ �������
    public void OpenSkillPanel()
    {
        skillPanel.SetActive(true);
        // ����� ����� ��������� UI-�������� ���������� ��������
    }

    // ����� ��� ������ ������ �������
    //public void ChooseSkill(int skillIndex)
    //{
    //    if (skillIndex >= 0 && skillIndex < availableSkills.Count)
    //    {
    //        Skills chosenSkillPrefab = availableSkills[skillIndex];
    //        Skills chosenSkill = Instantiate(chosenSkillPrefab); // ������� ��������� ���������� ������
    //        player.AddSkill(chosenSkill); // ��������� ����� � ������
    //        skillPanel.SetActive(false); // ��������� ������ �������
    //    }
    //}

    public void AddEconomistSkill()
    {
        foreach (Skills skill in availableSkills)
        {
            if (Skills.SkillType.Economist == skill.skillType)
            {
                skill.isEconomistSkillActive = true;
            }
        }
    }
    public void AddStrategSkill()
    {
        foreach (Skills skill in availableSkills)
        {
            if (Skills.SkillType.Strateg == skill.skillType)
            {
                skill.isStrategSkillActive = true;
            }
        }
    }
    public void AddInfluencerSkill()
    {
        foreach (Skills skill in availableSkills)
        {
            if (Skills.SkillType.Influencer == skill.skillType)
            {
                skill.isEconomistSkillActive = true;
            }
        }
    }
    public void AddSpeculantSkill()
    {
        foreach (Skills skill in availableSkills)
        {
            if (Skills.SkillType.Speculant == skill.skillType)
            {
                skill.isSpeculantSkillActive = true;
            }
        }
    }
    public void AddMasterSkill()
    {
        foreach (Skills skill in availableSkills)
        {
            if (Skills.SkillType.Master == skill.skillType)
            {
                skill.isMasterSkillActive = true;
            }
        }
    }
    public void AddIndustrialistSkill()
    {
        foreach (Skills skill in availableSkills)
        {
            if (Skills.SkillType.Industrialist == skill.skillType)
            {
                skill.isIndustrialistSkillActive = true;
            }
        }
    }
    public void AddCorporateSkill()
    {
        foreach (Skills skill in availableSkills)
        {
            if (Skills.SkillType.Corporate == skill.skillType)
            {
                skill.isCorporateSkillActive = true;
            }
        }
    }
    public void AddBuilderSkill()
    {
        foreach (Skills skill in availableSkills)
        {
            if (Skills.SkillType.Builder == skill.skillType)
            {
                skill.isBuilderSkillActive = true;
            }
        }
    }
}