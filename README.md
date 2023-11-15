# .NET 8 Playground <!-- omit in toc -->

- [/net8 Endpoints](#net8-endpoints)
  - [GET /net8/step/{id}](#get-net8stepid)
  - [Creating the JWT](#creating-the-jwt)

This was created with this command after installing the .NET 8 preview SDK in the GitHub Codespaces container.

```powershell
dotnet new webapi -o src
```

Then the `Program.cs` was edited to add some additional endpoints.

Features:

- [Parmeterless Constructors](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/instance-constructors#primary-constructors)
- [Spread Operator for collections](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#collection-expressions)
- [Default parameters for lambdas](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions#input-parameters-of-a-lambda-expression)
- [Alias of any type](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-12.0/using-alias-types)


## /net8 Endpoints


### GET /net8/step/{id}


### Creating the JWT

```powershell
dotnet user-jwts create --role admin -o token
```

