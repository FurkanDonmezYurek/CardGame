using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CardsSO", menuName = "Scriptable Objects/CardsSO")]
public class CardsSO : ScriptableObject
{
    [SerializeField]
    public string[] Who;
    public string[] How;
    public string[] Where;
}
