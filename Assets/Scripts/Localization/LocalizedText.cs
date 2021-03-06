﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedText : MonoBehaviour
{
	[SerializeField]
	private Prefix keyPrefix;
	[SerializeField]
	private string _key;
	public string key
	{
		get {return _key;}
		set { _key = value; setText(); }
	}

	private Text textComponent;
	private TextMesh textMesh;

	private enum Prefix
	{
		None,
		CurrentMicrogame
	}

	void Start ()
	{
		textComponent = GetComponent<Text>();
		textMesh = GetComponent<TextMesh>();
		setText();
	}

	/// <summary>
	/// Sets the key to load from and reloads the text with the new key
	/// </summary>
	/// <param name="key"></param>
	public void setKey(string key)
	{
		this._key = key;
		setText();
	}

	void setText()
	{
		string value;
		if (keyPrefix == Prefix.CurrentMicrogame)
			value = TextHelper.getLocalizedMicrogameText(key, getText());
		else
			value = TextHelper.getLocalizedText(getPrefixedKey(), getText());
		setText(value);
	}

	private void setText(string text)
	{
		if (textComponent != null)
			textComponent.text = text;
		else if (textMesh != null)
			textMesh.text = text;
	}

	private string getText()
	{
		if (textComponent != null)
			return textComponent.text;
		if (textMesh != null)
			return textMesh.text;
		return "";
	}

	string getPrefixedKey()
	{
		switch(keyPrefix)
		{
			//Handled seperately
			//case (Prefix.CurrentMicrogame):
			//	return "microgame." + gameObject.scene.name.Substring(0, gameObject.scene.name.Length - 1) + ".";
			default:
				return key;
		}
	}
}
