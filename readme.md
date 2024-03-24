
This repository showcases a method for implementing complex SQL query generation within the HotChocolate framework. A prime entry point for understanding this implementation is through the `` ExtensionService.GetExtensions `` method. This method illustrates how SQL queries are dynamically generated based on the selected fields in a GraphQL request. Exploring the usage of this method will reveal the process for extracting necessary information from GraphQL requests.

To get started quickly, you can clone the repository and run the project immediately. It is configured to use a localdb instance, with data being automatically bootstrapped for ease of use.

While there are numerous enhancements that could be made to tailor this setup for a production environment, the primary objective of this example is to elucidate the core principles behind SQL query generation in the context of GraphQL requests.

This example provides the foundation for executing a variety of queries, demonstrating the flexibility and power of dynamically generating SQL queries based on GraphQL inputs.

Get only processes:
``` graphql
query GetProcessesWithoutExtensions {
  processes(page: { page: 0, size: 5, orderBy: NAME, orderDirection: DESC }) {
    id
    name
  }
}
```
As result, we would have the following SQL query:
``` sql
SELECT Id, Name from Processes
ORDER BY Name Desc
OFFSET 0 ROWS FETCH NEXT 5 ROWS ONLY
```

Get processes and extentions count with filter:
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

As result, we would have two SQL queries:
``` sql
SELECT Id, Name from Processes
      ORDER BY Name Desc
      OFFSET 5 ROWS FETCH NEXT 5 ROWS ONLY

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

Get processes and extention names with filter:
``` graphql
query GetProcessesWithExtensionName {
  processes(page: { page: 0, size: 5, orderBy: NAME, orderDirection: DESC }) {
    id
    name
    extensions(where: { filter: "1f" }) {
      extensions {
        name
      }
    }
  }
}
```

As result, we would have two SQL queries:
``` sql
SELECT Id, Name from Processes
ORDER BY Name Desc
OFFSET 0 ROWS FETCH NEXT 5 ROWS ONLY

SELECT
ex1.ProcessId,
null as Count,
(SELECT Id, Name
    FROM Extensions as ex2
    WHERE ex2.ProcessId = ex1.ProcessId
    AND ex2.Name like '%1f%'

    FOR JSON PATH) as Json
FROM Extensions as ex1
WHERE ex1.ProcessId IN ('e6dda4d6-2a97-4878-d887-08dc4b700e94', 'b5d37d30-f7c3-4b31-d860-08dc4b700e94', '4f6c88b7-be3b-4d65-d872-08dc4b700e94', 'af820d81-28cc-4de3-d8b5-08dc4b700e94', 'fb60c551-d01c-410b-d862-08dc4b700e94')
AND ex1.Name like '%1f%'
GROUP BY ex1.ProcessId, ex1.Name
```

Fetch processes, exntensions count and names with filter and pagination:
``` graphql
query GetProcessesWithExtensionNameAndCount {
  processes(page: { page: 0, size: 5, orderBy: NAME, orderDirection: DESC }) {
    id
    name
    extensions(where: { filter: "1f" }, page: { orderBy: NAME, orderDirection: ASC, page: 0, size: 10 }) {
      count
      extensions {
        name
      }
    }
  }
}
```

SQL:
``` sql
SELECT Id, Name from Processes
ORDER BY Name Desc
OFFSET 0 ROWS FETCH NEXT 5 ROWS ONLY

SELECT
ex1.ProcessId,
(SELECT COUNT(*)
    FROM Extensions as ex2
    WHERE ex2.ProcessId = ex1.ProcessId
    AND ex2.Name like '%1f%') AS Count,
(SELECT Id, Name
    FROM Extensions as ex2
    WHERE ex2.ProcessId = ex1.ProcessId
    AND ex2.Name like '%1f%'
    ORDER BY Name Asc
OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY

    FOR JSON PATH) as Json
FROM Extensions as ex1
WHERE ex1.ProcessId IN ('e6dda4d6-2a97-4878-d887-08dc4b700e94', 'b5d37d30-f7c3-4b31-d860-08dc4b700e94', '4f6c88b7-be3b-4d65-d872-08dc4b700e94', 'af820d81-28cc-4de3-d8b5-08dc4b700e94', 'fb60c551-d01c-410b-d862-08dc4b700e94')
AND ex1.Name like '%1f%'
GROUP BY ex1.ProcessId, ex1.Name
ORDER BY Name Asc
OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY
```