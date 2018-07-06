对 Npgsql 进行的二次封装，包含连接池、缓存

也是dotnetgen_postgresql生成器所需postgresql数据库基础封装

# 安装

> Install-Package dng.Pgsql

# 使用

```csharp
public static Npgsql.Executer PgsqlInstance = 
    new Npgsql.Executer(IDistributedCache, connectionString, ILogger);

//PgsqlInstance.ExecuteReader
//PgsqlInstance.ExecuteReaderAsync

//ExecuteArray
//ExecuteArrayAsync

//ExecuteNonQuery
//ExecuteNonQueryAsync

//ExecuteScalar
//ExecuteScalarAsync
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