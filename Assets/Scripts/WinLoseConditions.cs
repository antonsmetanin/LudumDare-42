using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinLoseConditions : MonoBehaviour
{


	public AudioClip Music;
	public AudioSource Source;
	
	public enum EGameStatus
	{
		Tutorial,
		Running,
		Paused,
		Over
	}

	[SerializeField] private float _dayLength = 180f;
	private float _currentTime = 0;
	public IntReactiveProperty _dayNumber = new IntReactiveProperty(1);
	private int _daysOfFlood = 0;
	[SerializeField] private int _floodStartsAtDay = 3;
	public EGameStatus GameStatus;
	public FloatReactiveProperty FloodLevel = new FloatReactiveProperty();
	public FloatReactiveProperty Ark = new FloatReactiveProperty(10);
	public PlayerController _player;
	public Image FadeToBlack;
	public TextAnimation ta;

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

	private void Awake()
	{
		BotsSurvivors.Survived = 0;
	}

	private void Start()
	{
		_woodBar.gameObject.SetActive(false);

		FadeToBlack.CrossFadeAlpha(0, 1, true);

		
		_dayNumber.Subscribe(OnDayChange);
		Ark.Subscribe(f =>
		{
			_arkBar.fillAmount = f / _arkGoal;
			_ark.UpdateStage(f / _arkGoal);

			if (f >= _arkGoal)
			{
				StartCoroutine(Win());
			}
			
		});
		
		Ark.Value = 0;
		
		_ark.OnRecycle += OnRecycle;
	}

	private IEnumerator Win()
	{
		yield return new WaitForSeconds(2);
		GameStatus = EGameStatus.Over;
		FadeToBlack.gameObject.SetActive(true);
		yield return new WaitForSeconds(2);
		SceneManager.LoadSceneAsync("Win", LoadSceneMode.Single);
	}
	
	private IEnumerator Lose()
	{
		yield return new WaitForSeconds(1);
		
		GameStatus = EGameStatus.Over;
		FadeToBlack.gameObject.SetActive(true);
		yield return new WaitForSeconds(2);
		SceneManager.LoadSceneAsync("Lose", LoadSceneMode.Single);
	}

	private void OnRecycle(float obj)
	{
		Ark.Value += obj;
	}

	public void OnDayChange(int day)
	{
		_daysOfFlood = Mathf.Max(0, _dayNumber.Value - _floodStartsAtDay + 1);

		var timeUntilFlood = _daysOfFlood > 0 ? "<size=18><color=#FCDC70>Repent!</color></size>" : $"Days untils flood: <size=18><color=#FCDC70>{_floodStartsAtDay - _dayNumber.Value}</color></size>";
		_floodText.text = timeUntilFlood;

		if (_daysOfFlood > 0)
		{
			StartCoroutine(Co_flood(3));
			ta.Play(3);
		}
		
		if(_daysOfFlood > _floodLevels.Count - 1)
			StartCoroutine(Lose());
	}

	// Update is called once per frame
	void Update () {

		//FadeToBlack.gameObject.SetActive(true);
		
		if (GameStatus == EGameStatus.Running)
			_currentTime += Time.deltaTime;
		else if (GameStatus == EGameStatus.Over)
		{
			FadeToBlack.CrossFadeAlpha(1, 4, true);
			Source.volume -= .05f;
		}

		var inverseDaytime = (_dayLength - _currentTime) / _dayLength;
		_timerBar.fillAmount = inverseDaytime;

		if (_currentTime >= _dayLength)
		{
			_currentTime = 0;
			_dayNumber.Value++;
		}
	}

	private bool drown;
	
	public IEnumerator Co_flood(float time)
	{
		float t = 0;

		var currentHeight = _flood.position.y;
		var floodHeight = _floodLevels[Mathf.Clamp(_daysOfFlood, 0, _floodLevels.Count - 1)];

		while (t < time)
		{
			t += Time.deltaTime;
			var y = Mathf.Lerp(currentHeight, floodHeight, t / time);
			_flood.position = new Vector3(_flood.position.x, y, _flood.position.z);
			yield return null;
			
			if (!drown && _flood.position.y > _player.transform.position.y + _player.WaterThreshold)
			{
				drown = true;
				_player.Drowned();
				StartCoroutine(Lose());
			}
		}
	}
}
