﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[ExecuteInEditMode]
public class MicrogameCollection : MonoBehaviour
{
    [SerializeField]
    private List<Microgame> finishedMicrogames, stageReadyMicrogames, unfinishedMicrogames, bossMicrogames;

    [System.Serializable]
    public class Microgame
    {
        [SerializeField]
        private string _microgameId;
        public string microgameId { get { return _microgameId; } }

        [SerializeField]
        private MicrogameTraits[] _difficultyTraits;
        public MicrogameTraits[] difficultyTraits { get { return _difficultyTraits; } }

        public Microgame(string microgameId, MicrogameTraits[] difficultyTraits)
        {
            _microgameId = microgameId;
            _difficultyTraits = difficultyTraits;
        }
    }

	public enum Restriction
	{
		All,
		StageReady,
		Finished
	}

	public void updateMicrogames()
	{
		finishedMicrogames = new List<Microgame>();
		stageReadyMicrogames = new List<Microgame>();
		unfinishedMicrogames = new List<Microgame>();
		bossMicrogames = new List<Microgame>();

		string[] microgameDirectories = Directory.GetDirectories(Application.dataPath + "/Resources/Microgames/_Finished/");
		for (int i = 0; i < microgameDirectories.Length; i++)
		{
			string[] dirs = microgameDirectories[i].Split('/');
			string microgameId = dirs[dirs.Length - 1];
            MicrogameTraits[] difficultyTraits = getDifficultyTraits(microgameId, false);
            finishedMicrogames.Add(new Microgame(microgameId, difficultyTraits));
		}

		microgameDirectories = Directory.GetDirectories(Application.dataPath + "/Resources/Microgames/");
		for (int i = 0; i < microgameDirectories.Length; i++)
		{
			string[] dirs = microgameDirectories[i].Split('/');
			string microgameId = dirs[dirs.Length - 1];
			if (!microgameId.StartsWith("_"))
			{
                MicrogameTraits[] difficultyTraits = getDifficultyTraits(microgameId, true);
				if (difficultyTraits[0].isStageReady)
					stageReadyMicrogames.Add(new Microgame(microgameId, difficultyTraits));
				else
					unfinishedMicrogames.Add(new Microgame(microgameId, difficultyTraits));
			}
		}

		microgameDirectories = Directory.GetDirectories(Application.dataPath + "/Resources/Microgames/_Bosses/");
		for (int i = 0; i < microgameDirectories.Length; i++)
		{
			string[] dirs = microgameDirectories[i].Split('/');
			string microgameId = dirs[dirs.Length - 1];
            MicrogameTraits[] difficultyTraits = getDifficultyTraits(microgameId, true);
            bossMicrogames.Add(new Microgame(microgameId, difficultyTraits));
		}
	}

    MicrogameTraits[] getDifficultyTraits(string microgameId, bool skipFInishedFolder)
    {
        MicrogameTraits[] traits = new MicrogameTraits[3];
        for (int i = 0; i < 3; i++)
        {
            traits[i] = MicrogameTraits.findMicrogameTraits(microgameId, i + 1, skipFInishedFolder);
        }
        return traits;
    }

    //public List<Microgame> getMicrogames(Restriction restriction)
    //{
    //    List<CollectionBase> returnList = convertToStageMicrogameList(finishedMicrogames);
    //    if (restriction != Restriction.Finished)
    //    {
    //        returnList.AddRange(convertToStageMicrogameList(stageReadyMicrogames));
    //        if (restriction == Restriction.All)
    //            returnList.AddRange(convertToStageMicrogameList(unfinishedMicrogames));
    //    }
    //    return returnList;
    //}

    /// <summary>
    /// Returns all microgames in the game (with given restriction) in Stage.Microgame type (used for determining what will play in the stage)
    /// </summary>
    /// <param name="restriction"></param>
    /// <returns></returns>
    public List<Stage.Microgame> getStageMicrogames(Restriction restriction)
	{
        List<Stage.Microgame> returnList = convertToStageMicrogameList(finishedMicrogames);
		if (restriction != Restriction.Finished)
		{
			returnList.AddRange(convertToStageMicrogameList(stageReadyMicrogames));
			if (restriction == Restriction.All)
				returnList.AddRange(convertToStageMicrogameList(unfinishedMicrogames));
		}
		return returnList;
	}

	/// <summary>
	/// Returns a copied list of all boss microgmaes, regardless of completion, in Stage.Microagme type
	/// </summary>
	/// <returns></returns>
	public List<Stage.Microgame> getBossMicrogames()
	{
        List<Stage.Microgame> returnList = convertToStageMicrogameList(bossMicrogames);
		return returnList;
	}

    public Microgame findMicrogame(string microgameId)
    {
        //TODO optimize this whole process
        foreach (Microgame microgame in finishedMicrogames)
        {
            if (microgame.microgameId.Equals(microgameId))
                return microgame;
        }
        foreach (Microgame microgame in stageReadyMicrogames)
        {
            if (microgame.microgameId.Equals(microgameId))
                return microgame;
        }
        foreach (Microgame microgame in unfinishedMicrogames)
        {
            if (microgame.microgameId.Equals(microgameId))
                return microgame;
        }
        foreach (Microgame microgame in bossMicrogames)
        {
            if (microgame.microgameId.Equals(microgameId))
                return microgame;
        }
        Debug.Log("oops " + microgameId);
        return null;
    }

    private List<Stage.Microgame> convertToStageMicrogameList(List<Microgame> list)
    {
        List<Stage.Microgame>  returnList = new List<Stage.Microgame>();
        for (int i = 0; i < list.Count; i++)
        {
            returnList.Add(new Stage.Microgame(list[i].microgameId));
        }
        return returnList;
    }
}
