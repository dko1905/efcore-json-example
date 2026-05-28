# EF Core JSON example 

Repository for testing...

Requires:
- `docker`
- `dotnet` v10.0.108 or later

## Database

```sh
docker compose up -d
```

## Project

```sh
dotnet run
```

API:
- `/` fetch list of tenants and posts
- `/migrate` migrate database
- `/create` create *test* tenant
