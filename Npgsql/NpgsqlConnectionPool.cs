using Npgsql;
using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Npgsql {

	public class NpgsqlConnectionPool : ObjectPool<NpgsqlConnection> {

		internal Action availableHandler;
		internal Action unavailableHandler;

		public NpgsqlConnectionPool(string name, string connectionString, Action availableHandler, Action unavailableHandler) : base(null) {
			var policy = new NpgsqlConnectionPoolPolicy {
				_pool = this,
				Name = name
			};
			this.Policy = policy;
			policy.ConnectionString = connectionString;

			this.availableHandler = availableHandler;
			this.unavailableHandler = unavailableHandler;
		}

		public void Return(Object<NpgsqlConnection> obj, Exception exception, bool isRecreate = false) {
			if (exception != null && exception is NpgsqlException) {

				if (exception is System.IO.IOException) {

					base.SetUnavailable();

				} else if (obj.Value.Ping() == false) {

					base.SetUnavailable();
				}
			}
			base.Return(obj, isRecreate);
		}
	}

	public class NpgsqlConnectionPoolPolicy : IPolicy<NpgsqlConnection> {

		internal NpgsqlConnectionPool _pool;
		public string Name { get; set; } = "PostgreSQL NpgsqlConnection 对象池";
		public int PoolSize { get; set; } = 100;
		public TimeSpan SyncGetTimeout { get; set; } = TimeSpan.FromSeconds(10);
		public int AsyncGetCapacity { get; set; } = 10000;
		public bool IsThrowGetTimeoutException { get; set; } = true;
		public int CheckAvailableInterval { get; set; } = 5;

		private string _connectionString;
		public string ConnectionString {
			get => _connectionString;
			set {
				_connectionString = value ?? "";
				Match m = Regex.Match(_connectionString, @"Maximum\s*pool\s*size\s*=\s*(\d+)", RegexOptions.IgnoreCase);
				if (m.Success == false || int.TryParse(m.Groups[1].Value, out var poolsize) == false || poolsize <= 0) poolsize = 100;
				PoolSize = poolsize;

				//var initConns = new Object<NpgsqlConnection>[poolsize];
				//for (var a = 0; a < poolsize; a++) try { initConns[a] = _pool.Get(); } catch { }
				//foreach (var conn in initConns) _pool.Return(conn);
			}
		}


		public bool OnCheckAvailable(NpgsqlConnection obj) {
			if (obj.State == ConnectionState.Closed) obj.Open();
			var cmd = obj.CreateCommand();
			cmd.CommandText = "select 1";
			cmd.ExecuteNonQuery();
			return true;
		}

		public NpgsqlConnection OnCreate() {
			var conn = new NpgsqlConnection(_connectionString);
			return conn;
		}

		public void OnGet(Object<NpgsqlConnection> obj) {

			if (_pool.IsAvailable) {

				if (DateTime.Now.Subtract(obj.LastReturnTime).TotalSeconds > 60 && obj.Value.Ping() == false) {

					try {
						obj.Value.Open();
					} catch {
						if (_pool.SetUnavailable() == true)
							throw new Exception($"【{this.Name}】状态不可用，等待后台检查程序恢复方可使用。");
					}
				}
			}
		}

		async public Task OnGetAsync(Object<NpgsqlConnection> obj) {

			if (_pool.IsAvailable) {

				if (DateTime.Now.Subtract(obj.LastReturnTime).TotalSeconds > 60 && obj.Value.Ping() == false) {

					try {
						await obj.Value.OpenAsync();
					} catch {
						if (_pool.SetUnavailable() == true)
							throw new Exception($"【{this.Name}】状态不可用，等待后台检查程序恢复方可使用。");
					}
				}
			}
		}

		public void OnGetTimeout() {

		}

		public void OnReturn(Object<NpgsqlConnection> obj) {

		}

		public void OnAvailable() {
			_pool.availableHandler?.Invoke();
		}

		public void OnUnavailable() {
			_pool.unavailableHandler?.Invoke();
		}
	}

	public static class NpgsqlConnectionExtensions {

		public static bool Ping(this NpgsqlConnection that) {
			try {
				var cmd = that.CreateCommand();
				cmd.CommandText = "select 1";
				cmd.ExecuteNonQuery();
				return true;
			} catch {
				try { that.Close(); } catch { }
				return false;
			}
		}
	}
}
