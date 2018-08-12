using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Boo.Lang;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class WinLoseConditions : MonoBehaviour
{

	public enum EGameStatus
	{
		Tutorial,
		Running,
		Paused
	}

	[SerializeField] private float _dayLength = 180f;
	private float _currentTime = 0;
	public IntReactiveProperty _dayNumber = new IntReactiveProperty();
	private int _daysOfFlood = 0;
	[SerializeField] private int _floodStartsAtDay = 3;
	[SerializeField] private AnimationCurve _floodLevels;
	public EGameStatus GameStatus;
	public FloatReactiveProperty FloodLevel = new FloatReactiveProperty();
	public FloatReactiveProperty Wood = new FloatReactiveProperty(30);
	[SerializeField] private float _woodGoal = 1000;


	[SerializeField] private Image _arkBar;
	[SerializeField] private Image _timerBar;
	[SerializeField] private Text _floodText;


	private void Start()
	{
		_dayNumber.Subscribe(OnDayChange);
	}

	public void OnDayChange(int day)
	{
		_daysOfFlood = Mathf.Max(0, _daysOfFlood - _floodStartsAtDay + 1);

		var timeUntilFlood = _daysOfFlood > 0 ? "Repent!" : $"Days untils flood: <size=18><color=#FCDC70>{_floodStartsAtDay - _dayNumber.Value}</color></size>";
		_floodText.text = timeUntilFlood;
	}

	// Update is called once per frame
	void Update () {

		if (GameStatus == EGameStatus.Running)
			_currentTime += Time.deltaTime;

		var inverseDaytime = (_dayLength - _currentTime) / _dayLength;
		_timerBar.fillAmount = inverseDaytime;

		if (_currentTime >= _dayLength)
		{
			_currentTime = 0;
			_dayNumber.Value++;
		}
	}

}
