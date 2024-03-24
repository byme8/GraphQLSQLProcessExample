
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
```

``` sql
select ProcessId, count(*)
from Extensions
where ProcessId in @ProcessIds
AND Name like '%1f%'
group by ProcessId;
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
```

``` sql
select ProcessId,
       Id,
       Name
from Extensions
where ProcessId in @ProcessIds
   AND Name like '%1f%'
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
```

``` sql
select ProcessId, count(*)
from Extensions
where ProcessId in @ProcessIds
AND Name like '%1f%'
group by ProcessId;

select ProcessId,
       Id,
       Name
from Extensions
where ProcessId in @ProcessIds
   AND Name like '%1f%'
   ORDER BY Name Asc
OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY
```