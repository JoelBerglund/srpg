﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using srpg;
using System.Linq;
using System;

public class GBattle : MonoBehaviour {

	public GTileSetShit GTileSet;
	public GameObject GBeing;
	public GameObject BeingInfoPanel;
	public GameObject BeingInfoText;
	public GameEventDisplayS GEDS;

	public GameObject BeingCommandPanel;
	public UnityEngine.UI.Button CommandButton;
	
	public GBeingSelection BeingSelector;

	private Battle battle;
	private UnityBattleIO BIO;

	private void Start () {
		battle = GameObject.Find("GameControl")
				.GetComponent<Assets.GameControl>()
				.GETBATTLE();

		BIO = new UnityBattleIO(GTileSet, BeingCommandPanel, CommandButton, GEDS);
		battle.SetIO(BIO);

		GTileSet.Build(battle.TileSet);
		GTileSet.Clicked += TileClickHandler;

		foreach(var b in battle.Beings) {
			BeingSetUp(Instantiate(GBeing), b);
		}

		battle.BeingAdded += (s, e) => BeingSetUp(gbeingToAdd ?? Instantiate(GBeing), e.Being);
		BeingSelector.Populate();
	}
	
	private void BeingSetUp(GameObject gb, Being b) {
		var old = gb.transform.position;
		gb.transform.position = new Vector3(b.Place.X, b.Place.Height*GTileS.HeightMultiplier, b.Place.Y);

		b.MoveStarted += (s, e) => gb.GetComponent<GBeingMover>().Move(e.Path);

		var binft = Instantiate(BeingInfoText);
		var binft2 = binft.GetComponent<BeingToTextHandler>();
		binft.transform.position += new Vector3(0, 100000, 0);
		binft2.SetBeing(b);
		b.TurnStarted += (s, e) => binft.transform.position -= new Vector3(0, 100000, 0);
		b.TurnFinished += (s, e) => binft.transform.position += new Vector3(0, 100000, 0);
		binft.transform.SetParent(BeingInfoPanel.transform, false);
	}
	
	public GameObject DrawBeing(Being b) {
		return Instantiate(GBeing);
	}
	private GameObject gbeingToAdd;
	public void AddBeing(Being b, int x, int y, GameObject gb = null) {
		gbeingToAdd = gb;
		b.AddToBattle(battle, x, y);
	}

	public Action<object, TileClickedEventArgs> TileCLickHappening;
	private void TileClickHandler(object s, TileClickedEventArgs e) {
		if (TileCLickHappening != null) {
			TileCLickHappening(s, e);
		}
		else
			BIO.Do(e.Tile);
	}

	public void StartBattle() {
		battle.Paused = false;
	}
}