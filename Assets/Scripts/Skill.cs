using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Skill : MonoBehaviour
{
    [SerializeField] private SkillManager.SkillType skillType;
    [SerializeField] private string skillName;
    [SerializeField] private string description;
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private TMP_Text skillDescriptionText;
    [SerializeField] private Button getSkillButton;
    [SerializeField] public bool isSkillActive;
    [SerializeField] private float skillModifier;

    public SkillManager.SkillType SkillType => skillType;
    public float SkillModifier => skillModifier;

    public event System.Action<Skill> OnGetSkill;

    private void OnValidate()
    {
        skillNameText.text = skillName;
        skillDescriptionText.text = description;
    }

    private void Start()
    {
        getSkillButton.onClick.AddListener(OnGetSkillButtonClick);
    }

    private void OnGetSkillButtonClick()
    {
        OnGetSkill?.Invoke(this); //if OnGetSkill != null
    }
}
