﻿using System.Collections;
using Tomlyn.Model;

namespace Echidna2.Serialization;

public static class TomlExtensions
{
	public static T GetCasted<T>(this TomlTable table, string key) => (T)table[key];
	public static T GetCasted<T>(this TomlTable table, string key, T defaultValue) => table.TryGetValue(key, out object? value) ? (T)value : defaultValue;
	public static List<T> GetList<T>(this TomlTable table, string key) => table.GetCasted<IEnumerable>(key, new TomlArray()).Cast<T>().ToList();
	public static Dictionary<string, object> GetDict(this TomlTable table, string key) => table.GetCasted(key, new TomlTable()).ToDictionary();
	public static string GetString(this TomlTable table, string key) => table.GetCasted(key, "");
}