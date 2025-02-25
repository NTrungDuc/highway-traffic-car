using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revive : MonoBehaviour
{
    private HR_PlayerHandler player;
    private void OnEnable()
    {
        HR_GamePlayHandler.OnPlayerSpawned += HR_PlayerHandler_OnPlayerSpawned;
    }
    private void HR_PlayerHandler_OnPlayerSpawned(HR_PlayerHandler _player)
    {

        player = _player;

    }
    public void RevivePlayer()
    {
        player.Revive();
    }
    private void OnDisable()
    {
        HR_GamePlayHandler.OnPlayerSpawned -= HR_PlayerHandler_OnPlayerSpawned;
    }
}
