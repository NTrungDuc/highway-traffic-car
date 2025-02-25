//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Get, add, consume currency. Get unlocked vehicles, and unlocked parts string list.
/// </summary>
public class HR_API {

    /// <summary>
    /// When player currency changes...
    /// </summary>
    public delegate void onPlayerGoldsChanged();
    public static event onPlayerGoldsChanged OnPlayerGoldsChanged;
    public delegate void onPlayerDiamondsChanged();
    public static event onPlayerDiamondsChanged OnPlayerDiamondsChanged;

    /// <summary>
    /// Gets the current currency as an int.
    /// </summary>
    /// <returns></returns>
    public static int GetCurrencyGold() {

        return PlayerPrefs.GetInt("CurrencyGold", 0);

    }

    /// <summary>
    /// Gets the current currency as an int.
    /// </summary>
    /// <returns></returns>
    public static int GetCurrencyDiamond()
    {

        return PlayerPrefs.GetInt("CurrencyDiamond", 0);

    }

    /// <summary>
    /// Consumes currency.
    /// </summary>
    /// <param name="consume"></param>
    public static void ConsumeCurrencyGold(int consume) {

        int current = GetCurrencyGold();

        PlayerPrefs.SetInt("CurrencyGold", current - consume);

        if (OnPlayerGoldsChanged != null)
            OnPlayerGoldsChanged();

    }

    /// <summary>
    /// Consumes currency.
    /// </summary>
    /// <param name="consume"></param>
    public static void ConsumeCurrencyDiamond(int consume)
    {

        int current = GetCurrencyDiamond();

        PlayerPrefs.SetInt("CurrencyDiamond", current - consume);

        if (OnPlayerDiamondsChanged != null)
            OnPlayerDiamondsChanged();

    }

    /// <summary>
    /// Adds currency.
    /// </summary>
    /// <param name="add"></param>
    public static void AddCurrencyGold(int add) {

        int current = GetCurrencyGold();

        PlayerPrefs.SetInt("CurrencyGold", current + add);

        if (OnPlayerGoldsChanged != null)
            OnPlayerGoldsChanged();

    }

    /// <summary>
    /// Adds currency.
    /// </summary>
    /// <param name="add"></param>
    public static void AddCurrencyDiamond(int add)
    {

        int current = GetCurrencyDiamond();

        PlayerPrefs.SetInt("CurrencyDiamond", current + add);

        if (OnPlayerDiamondsChanged != null)
            OnPlayerDiamondsChanged();

    }

    /// <summary>
    /// Gets unlocked vehicles with their indexes.
    /// </summary>
    /// <returns></returns>
    public static int[] UnlockedVehicles() {

        List<int> unlockeds = new List<int>();

        for (int i = 0; i < HR_PlayerCars.Instance.cars.Length; i++) {

            if (PlayerPrefs.HasKey(HR_PlayerCars.Instance.cars[i].playerCar.name + "Owned"))
                unlockeds.Add(i);

        }

        return unlockeds.ToArray();

    }

    /// <summary>
    /// Unlocks target vehicle.
    /// </summary>
    /// <param name="index"></param>
    public static void UnlockVehice(int index) {

        PlayerPrefs.SetInt(HR_PlayerCars.Instance.cars[index].playerCar.name + "Owned", 1);

    }

    /// <summary>
    /// Locks target vehicle.
    /// </summary>
    /// <param name="index"></param>
    public static void LockVehice(int index) {

        PlayerPrefs.DeleteKey(HR_PlayerCars.Instance.cars[index].playerCar.name + "Owned");

    }

    /// <summary>
    /// Is this vehicle purchased?
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static bool OwnedVehicle(int index) {

        if (PlayerPrefs.HasKey(HR_PlayerCars.Instance.cars[index].playerCar.name + "Owned"))
            return true;
        else
            return false;

    }

    /// <summary>
    /// Saves high scores.
    /// </summary>
    /// <param name="scores"></param>
    public static void SaveHighScores(int[] scores) {

        PlayerPrefs.SetInt("bestScoreOneWay", scores[0]);
        PlayerPrefs.SetInt("bestScoreTwoWay", scores[1]);
        PlayerPrefs.SetInt("bestScoreTimeAttack", scores[2]);
        PlayerPrefs.SetInt("bestScoreBomb", scores[3]);

    }

    /// <summary>
    /// Loads high scores.
    /// </summary>
    /// <param name="scores"></param>
    public static int[] GetHighScores() {

        int[] scores = new int[4];

        scores[0] = PlayerPrefs.GetInt("bestScoreOneWay");
        scores[1] = PlayerPrefs.GetInt("bestScoreTwoWay");
        scores[2] = PlayerPrefs.GetInt("bestScoreTimeAttack");
        scores[3] = PlayerPrefs.GetInt("bestScoreBomb");

        return scores;

    }

}
