# ADX Natural Language to KQL Demo
A demonstration project that converts natural language queries into Kusto Query Language (KQL) using AI agents with Microsoft Semantic Kernel.

# Overview
This project showcases how to use AI agents to translate natural language questions into valid Kusto Query Language (KQL) queries for Azure Data Explorer (ADX). The system utilizes a multi-agent approach with:
1.	KustoQueryAgent - Generates KQL queries from natural language inputs
2.	KustoQueryValidationAgent - Validates the syntax of generated KQL queries
The agents collaborate through a group chat to produce and validate queries based on user requests.

# Features
- Natural language to KQL conversion
- AI-powered query generation with domain expertise
- Automatic validation of query syntax
- Multi-agent collaboration for improved results
- Built on Microsoft Semantic Kernel's agent framework

# Prerequisites
- .NET 9.0
- Azure OpenAI API access
- Visual Studio 2022 or compatible IDE

-# Configuration
The application uses the following configuration sources (in order of precedence):
1. User Secrets
2. appsettings.json
3. Environment Variables

Required Configuration Values
```csharp
{
  "AzureOpenAIDeploymentName": "your-deployment-name",
  "AzureOpenAIEndpoint": "https://your-resource-name.openai.azure.com/",
  "AzureOpenAIAPIKey": "your-api-key"
}
```

## Setting Up User Secrets
For local development, use .NET's Secret Manager:
```
dotnet user-secrets set "AzureOpenAIDeploymentName" "your-deployment-name"
dotnet user-secrets set "AzureOpenAIEndpoint" "https://your-resource-name.openai.azure.com/"
dotnet user-secrets set "AzureOpenAIAPIKey" "your-api-key"
```

## Usage
The application currently demonstrates a simple query example:
```csharp
// Example query
ChatMessageContent input = new(AuthorRole.User, "Where are all the sql servers that have a cpu percentage over 2?");
```

The agents will process this natural language query, convert it to a KQL query, validate it, and return the result when approved.

## Project Structure
- Program.cs - Main application entry point
- Agents/KustoQueryAgent.cs - Agent for generating KQL queries
- Agents/KustoQueryValidationAgent.cs - Agent for validating KQL syntax
- TerminationStrategies/ApprovalTerminationStrategy.cs - Controls when agent conversation ends

## How It Works
1. A natural language query is submitted to the agent group chat
2. The KustoQueryAgent transforms the request into a KQL query
3. The KustoQueryValidationAgent parses and validates the syntax
4. If valid, the validation agent responds with "approve"
5. The conversation terminates with the approved query

## Extending the Project
- Add more specialized agents for different query types
- Implement execution of validated queries against actual ADX clusters
- Enhance the validation with semantic correctness checks
- Add a user interface for interactive querying