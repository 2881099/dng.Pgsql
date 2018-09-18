对 Npgsql 进行的二次封装，包含连接池、缓存

也是dotnetgen_postgresql生成器所需postgresql数据库基础封装

# 安装

> Install-Package dng.Pgsql

# 使用

```csharp
public static Npgsql.Executer PgsqlInstance = 
    new Npgsql.Executer(IDistributedCache, masterConnectionString, slaveConnectionStrings, ILogger);

PgsqlInstance.ExecuteReader
PgsqlInstance.ExecuteReaderAsync

PgsqlInstance.ExecuteArray
PgsqlInstance.ExecuteArrayAsync

PgsqlInstance.ExecuteNonQuery
PgsqlInstance.ExecuteNonQueryAsync

PgsqlInstance.ExecuteScalar
PgsqlInstance.ExecuteScalarAsync
```

# 事务

```csharp
PgsqlInstance.Transaction(() => {

});
```

# 缓存壳

```csharp
PgsqlInstance.CacheShell(key, timeoutSeconds, () => {
    return dataSource;
});

PgsqlInstance.RemoveCache(key);
```

# 读写分离

若配置了从数据库连接串，从数据库可以设置多个，访问策略为随机。从库实现了故障切换，自动恢复机制。

以下方法执行 sql 语句，为 select 开头，则默认查从数据库，反之则查主数据库。

```csharp
PgsqlInstance.ExecuteReader
PgsqlInstance.ExecuteReaderAsync

PgsqlInstance.ExecuteArray
PgsqlInstance.ExecuteArrayAsync
```

以下方法在主数据库执行：

```csharp
PgsqlInstance.ExecuteNonQuery
PgsqlInstance.ExecuteNonQueryAsync

PgsqlInstance.ExecuteScalar
PgsqlInstance.ExecuteScalarAsync
```
