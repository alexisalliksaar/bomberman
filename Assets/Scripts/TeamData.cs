using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/TeamData")]
public class TeamData : ScriptableObject
{
    public string Name;

    public Color TeamColor;
    public int SortingLayerOrder;
}
