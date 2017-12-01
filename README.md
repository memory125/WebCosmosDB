# WebCosmosDB

* This repo is primary for Azure Cosmos DB, which includes Web App (AdsDasboard, azure-cosmosDB-dotnet, mongdb-dotnet, quickstartcore), console app (table-dotnet) and maven project (document-spring).

## AdsDashboard
>* This web app is developed by .net framework.
>* You should create the Azure Cosmos DB with Gremlin (graph) first.
>* Copy the url & key or connectionstring from the cosmos graph database created above.
>* Replace "endpoint", "authkey", "database" and "collection" with your real keys in Web.config.
>* Run this sample via Visual Studio 2017.

## azure-cosmosDB-dotnet
>* This web app is developed by .net framework.
>* You should create the Azure Cosmos DB with SQL API first.
>* Copy the url & key or connectionstring from the cosmos graph database created above.
>* Replace "endpoint", "authkey", "database" and "collection" with your real keys in Web.config.
>* Run this sample via Visual Studio 2017.

## document-spring
>* This maven project is developed by spring boot.
>* You should create the Azure Cosmos DB with SQL API first.
>* Copy the url & key or connectionstring from the cosmos graph database created above.
>* Replace "endpoint", "authkey", "database" and "collection" with your real keys in application.properties.
>* Run this sample via the follwing command.
    </br>  mvn package
    </br>  java -jar target/cosmosdb-0.0.1-SNAPSHOT.jar
* Note:
    </br> You should setup the maven development environment first.

## mongdb-dotnet
>* This web app is developed by .net framework.
>* You should create the Azure Cosmos DB with MongoDB API first.
>* Copy the url & key or connectionstring from the cosmos graph database created above.
>* Replace "endpoint", "authkey", "database" and "collection" with your real keys in Web.config.
>* Run this sample via Visual Studio 2017.

## quickstartcore
>* This web app is developed by .net core.
>* You should create the Azure Cosmos DB with SQL API first.
>* Copy the url & key or connectionstring from the cosmos graph database created above.
>* Replace "endpoint", "authkey", "database" and "collection" with your real keys in Web.config.
>* Run this sample via Visual Studio 2017.

## table-dotnet
>* This console app is developed by .net core.
>* You should create the Azure Cosmos DB with table API first.
>* Copy the url & key or connectionstring from the cosmos graph database created above.
>* Replace "StandardStorageConnectionString" with your real keys in App.config.
>* Run this sample via Visual Studio 2017.