﻿using NpgsqlTypes;
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace Npgsql {
	partial class Executer {

		public static string Addslashes(string filter, params object[] parms) {
			if (filter == null || parms == null) return string.Empty;
			if (parms.Length == 0) return filter;
			object[] nparms = new object[parms.Length];
			for (int a = 0; a < parms.Length; a++) {
				if (parms[a] == null) nparms[a] = "NULL";
				else {
					if (parms[a] is bool || parms[a] is bool?)
						nparms[a] = (bool) parms[a] ? "'t'" : "'f'";
					else if (parms[a] is string || parms[a] is Enum)
						nparms[a] = string.Concat("'", parms[a].ToString().Replace("'", "''"), "'");
					else if (decimal.TryParse(string.Concat(parms[a]), out decimal trydec))
						nparms[a] = parms[a];
					else if (parms[a] is DateTime) {
						DateTime dt = (DateTime) parms[a];
						nparms[a] = string.Concat("'", dt.ToString("yyyy-MM-dd HH:mm:ss.ffffff"), "'");
					} else if (parms[a] is DateTime?) {
						DateTime? dt = parms[a] as DateTime?;
						nparms[a] = string.Concat("'", dt.Value.ToString("yyyy-MM-dd HH:mm:ss.ffffff"), "'");
					} else if (parms[a] is IEnumerable) {
						string sb = "";
						var ie = parms[a] as IEnumerable;
						foreach (var z in ie) sb += z == null ? string.Concat(",NULL") : string.Concat(",'", z.ToString().Replace("'", "''"), "'");
						nparms[a] = string.IsNullOrEmpty(sb) ? sb : sb.Substring(1);
					} else {
						nparms[a] = string.Concat("'", parms[a].ToString().Replace("'", "''"), "'");
						//if (parms[a] is string) nparms[a] = string.Concat('N', nparms[a]);
					}
				}
			}
			try { string ret = string.Format(filter, nparms); return ret; } catch { return filter; }
		}

		private static DateTime dt1970 = new DateTime(1970, 1, 1);
		private static ThreadLocal<Random> rnd = new ThreadLocal<Random>(() => new Random());
		private static readonly int __staticMachine = ((0x00ffffff & Environment.MachineName.GetHashCode()) +
#if NETSTANDARD1_5 || NETSTANDARD1_6
			1
#else
			AppDomain.CurrentDomain.Id
#endif
			) & 0x00ffffff;
		private static readonly int __staticPid = Process.GetCurrentProcess().Id;
		private static int __staticIncrement = rnd.Value.Next();
		/// <summary>
		/// 生成类似Mongodb的ObjectId有序、不重复Guid
		/// </summary>
		/// <returns></returns>
		public static Guid NewMongodbId() {
			var now = DateTime.Now;
			var uninxtime = (int) now.Subtract(dt1970).TotalSeconds;
			int increment = Interlocked.Increment(ref __staticIncrement) & 0x00ffffff;
			var rand = rnd.Value.Next(0, int.MaxValue);
			var guid = $"{uninxtime.ToString("x8").PadLeft(8, '0')}{__staticMachine.ToString("x8").PadLeft(8, '0').Substring(2, 6)}{__staticPid.ToString("x8").PadLeft(8, '0').Substring(6, 2)}{increment.ToString("x8").PadLeft(8, '0')}{rand.ToString("x8").PadLeft(8, '0')}";
			return Guid.Parse(guid);
		}

		public static NpgsqlRange<T> ParseNpgsqlRange<T>(string s) {
			if (string.IsNullOrEmpty(s) || s == "empty") return NpgsqlRange<T>.Empty;
			string s1 = s.Trim('(', ')', '[', ']');
			string[] ss = s1.Split(new char[] { ',' }, 2);
			if (ss.Length != 2) return NpgsqlRange<T>.Empty;
			T t1 = default(T);
			T t2 = default(T);
			if (!string.IsNullOrEmpty(ss[0])) t1 = (T) Convert.ChangeType(ss[0], typeof(T));
			if (!string.IsNullOrEmpty(ss[1])) t2 = (T) Convert.ChangeType(ss[1], typeof(T));
			return new NpgsqlRange<T>(t1, s[0] == '[', s[0] == '(', t2, s[s.Length - 1] == ']', s[s.Length - 1] == ')');
		}
		/// <summary>
		/// 将 1010101010 这样的二进制字符串转换成 BitArray
		/// </summary>
		/// <param name="_1010">1010101010</param>
		/// <returns></returns>
		public static BitArray Parse1010(string _1010) {
			BitArray ret = new BitArray(_1010.Length);
			for (int a = 0; a < _1010.Length; a++) ret[a] = _1010[a] == '1';
			return ret;
		}
	}
}

public static partial class Npgsql_ExtensionMethods {
	public static string To1010(this BitArray ba) {
		char[] ret = new char[ba.Length];
		for (int a = 0; a < ba.Length; a++) ret[a] = ba[a] ? '1' : '0';
		return new string(ret);
	}
}