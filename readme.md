
``` graphql
query GetProcessesWithoutExtensions {
  processes(page: { page: 0, size: 5, orderBy: NAME, orderDirection: DESC }) {
    id
    name
  }
}
```

``` sql
select Id, Name from Processes
ORDER BY Name Desc
OFFSET 0 ROWS FETCH NEXT 5 ROWS ONLY
```


``` graphql
query GetProcessesWithExtensionCount {
  processes(page: { page: 1, size: 5, orderBy: NAME, orderDirection: DESC }) {
    id
    name
    extensions(where: { filter: "1f" }) {
      count
    }
  }
}
```

``` sql
select Id, Name from Processes
      ORDER BY Name Desc
      OFFSET 5 ROWS FETCH NEXT 5 ROWS ONLY
```

``` sql
SELECT
      ex1.ProcessId, 
      (SELECT COUNT(*)
         FROM Extensions as ex2
         WHERE ex2.ProcessId = ex1.ProcessId
         AND ex2.Name like '%1f%'
      ) AS Count,
      null as Json
      FROM Extensions as ex1
      WHERE ex1.ProcessId IN ('9fc76a22-6170-4169-d87e-08dc4b700e94', 'd3fa902a-8dd4-4006-d8a0-08dc4b700e94', '4938eb0e-ec31-49a4-d891-08dc4b700e94', '89df7eb8-d6fc-47e4-d8ab-08dc4b700e94', '5fd386cb-586c-42bd-d86c-08dc4b700e94')
      AND ex1.Name like '%1f%'
      GROUP BY ex1.ProcessId, ex1.Name
```