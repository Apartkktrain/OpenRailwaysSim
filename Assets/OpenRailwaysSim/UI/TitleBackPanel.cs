﻿using UnityEngine;

public class TitleBackPanel : GamePanel {

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape))
			show (false);
	}

	public void OKButton () {
		//TODO マルチプレイ対応予定

		show (false);
		GameCanvas.pausePanel.show (false);
		Main.closeMap ();
		GameCanvas.titlePanel.show (true);
	}
}
