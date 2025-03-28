using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Skill : MonoBehaviour
{
    [SerializeField] SkillManager.SkillType skillType;
    [SerializeField] string skillName;
    [SerializeField] string description;
    [SerializeField] TMP_Text skillNameText;
    [SerializeField] TMP_Text skillDescriptionText;
    [SerializeField] Button getSkillButton;
    [SerializeField] public bool isSkillActive;
    [SerializeField] float skillModifier;

    public SkillManager.SkillType SkillType => skillType;
    public float SkillModifier => skillModifier;

    public event System.Action<Skill> OnGetSkill;

    void OnValidate()
    {
        skillNameText.text = skillName;
        skillDescriptionText.text = description;
    }

    void Start()
    {
        getSkillButton.onClick.AddListener(OnGetSkillButtonClick);
    }

    void OnGetSkillButtonClick()
    {
        Debug.Log("click GetSkill");
        OnGetSkill?.Invoke(this); //if OnGetSkill != null
    }
}
