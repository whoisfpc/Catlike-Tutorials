using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FPSCounter))]
public class FPSDisplay : MonoBehaviour
{

	public Text highestFPSLabel, averageFPSLabel, lowestFPSLabel;
	FPSCounter fpsCounter;
	[SerializeField]
	private FPSColor[] coloring;
	private static string[] stringsFrom00To99;

	[System.Serializable]
	private struct FPSColor
	{
		public Color color;
		public int minimumFPS;
	}

	void Awake()
	{
		fpsCounter = GetComponent<FPSCounter>();
		stringsFrom00To99 = new string[100];
		for (int i = 0; i <= 99; i++)
		{
			stringsFrom00To99[i] = "" + i;
		}
	}

	void Update()
	{
		Display(highestFPSLabel, fpsCounter.HighestFPS);
		Display(averageFPSLabel, fpsCounter.AverageFPS);
		Display(lowestFPSLabel, fpsCounter.LowestFPS);
	}

	void Display(Text label, int fps)
	{
		label.text = stringsFrom00To99[Mathf.Clamp(fps, 0, 99)];
		for (int i = 0; i < coloring.Length; i++)
		{
			if (fps >= coloring[i].minimumFPS)
			{
				label.color = coloring[i].color;
				break;
			}
		}
	}
}