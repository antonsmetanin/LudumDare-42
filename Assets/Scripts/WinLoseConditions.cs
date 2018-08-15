using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
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
	public IntReactiveProperty _dayNumber = new IntReactiveProperty(1);
	private int _daysOfFlood = 0;
	[SerializeField] private int _floodStartsAtDay = 3;
	public EGameStatus GameStatus;
	public FloatReactiveProperty FloodLevel = new FloatReactiveProperty();
	public FloatReactiveProperty Wood = new FloatReactiveProperty(30);
	public FloatReactiveProperty Ark = new FloatReactiveProperty(10);

	[SerializeField] private float _arkGoal = 1000;


	[SerializeField] private List<float> _floodLevels;
	[SerializeField] private Transform _flood;

	[ContextMenu("Add flood level")]
	void addfloodLevel()
	{
		if(_floodLevels == null)
			_floodLevels = new List<float>();

		_floodLevels.Add(_flood.position.y);
	}


	[SerializeField] private Image _arkBar;
	[SerializeField] private Image _woodBar;

	[SerializeField] private Image _timerBar;
	[SerializeField] private Text _floodText;
	[SerializeField] private Ark _ark;



	private void Start()
	{
		_dayNumber.Subscribe(OnDayChange);
		Ark.Subscribe(f =>
		{
			_arkBar.fillAmount = f / _arkGoal;
			_ark.UpdateStage(f / _arkGoal);
		});
		Wood.Subscribe(f => { _woodBar.fillAmount = (Ark.Value + f) / _arkGoal; });

		Ark.Value++;
		Wood.Value++;
	}

	public void OnDayChange(int day)
	{
		_daysOfFlood = Mathf.Max(0, _dayNumber.Value - _floodStartsAtDay + 1);

		var timeUntilFlood = _daysOfFlood > 0 ? "<size=18><color=#FCDC70>Repent!</color></size>" : $"Days untils flood: <size=18><color=#FCDC70>{_floodStartsAtDay - _dayNumber.Value}</color></size>";
		_floodText.text = timeUntilFlood;

		if (_daysOfFlood > 0)
		{
			StartCoroutine(Co_flood(1));
		}
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

	public IEnumerator Co_flood(float time)
	{
		float t = 0;

		var currentHeight = _flood.transform.position.y;
		var floodHeight = _floodLevels[Mathf.Clamp(_daysOfFlood, 0, _floodLevels.Count - 1)];

		while (t < time)
		{
			t += Time.deltaTime;
			var y = Mathf.Lerp(currentHeight, floodHeight, t / time);
			_flood.transform.position = new Vector3(_flood.position.x, y, _flood.position.z);
			yield return null;
		}
	}
}
