using System;

/// <summary>
/// A static class with useful variables and
/// functions that you don't have to instantiate
/// to use.
/// </summary>
public static class Utils
{
	/// <summary>
	/// The static randomizer for the entire jart.
	/// </summary>
	public static Random Randomizer = new Random();

	/// <summary>
	/// A generic function that will return a random element
	/// of any array you pass it.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="array"></param>
	/// <returns></returns>
	public static T GetRandomArrayItem<T>(T[] array)
	{
		return array[Randomizer.Next(0, array.Length)];
	}
}
