﻿using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace adx_nl2kql_demo.Agents
{
    public class KustoQueryAgent : ChatHistoryAgent
    {
        private readonly IKernelBuilder _kernelBuilder;
        private readonly ChatCompletionAgent _agent;

        public new string Name => "KustoQueryAgent";

        public new string Description => "Generates Kusto queries.";

        public KustoQueryAgent(IKernelBuilder kernelBuilder)
        {
            Kernel kernel = kernelBuilder.Build();
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            var arguments = new KernelArguments(executionSettings);

            _agent = new()
            {
                Name = Name,
                Description = Description,
                Instructions = KustoAgentSystemPrompt,
                Kernel = kernel,
                Arguments = arguments
            };
        }

        public override IAsyncEnumerable<ChatMessageContent> InvokeAsync(ChatHistory history, KernelArguments? arguments = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            return _agent.InvokeAsync(history, arguments, kernel, cancellationToken);
        }

        public override IAsyncEnumerable<StreamingChatMessageContent> InvokeStreamingAsync(ChatHistory history, KernelArguments? arguments = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            return _agent.InvokeStreamingAsync(history, arguments, kernel, cancellationToken);
        }

        public override IAsyncEnumerable<AgentResponseItem<ChatMessageContent>> InvokeAsync(ICollection<ChatMessageContent> messages, AgentThread? thread = null, AgentInvokeOptions? options = null, CancellationToken cancellationToken = default)
        {
            return this._agent.InvokeAsync(messages, thread, options, cancellationToken);
        }

        public override IAsyncEnumerable<AgentResponseItem<StreamingChatMessageContent>> InvokeStreamingAsync(ICollection<ChatMessageContent> messages, AgentThread? thread = null, AgentInvokeOptions? options = null, CancellationToken cancellationToken = default)
        {
            return this._agent.InvokeStreamingAsync(messages, thread, options, cancellationToken);
        }

        protected override Task<AgentChannel> RestoreChannelAsync(string channelState, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        string KustoAgentSystemPrompt => """
        # Instructions
        You will receive a natural language request. As a cyber-security expert specializing in Microsoft USX, your task is to write a Kusto Query Language (KQL) query that fulfills the request while adhering to all best practices.
        Follow these steps:
        - Start by coming up with a stepwise plan on how to address the request.
        - Identify any challenges you might face with this plan.
        - Choose a final approach that seems to fit the request best.
        - Write an optimized and performant Kusto query that fulfills the request while adhering to all best practices.
        # Schema
        To help you write your query, you have been given the following Microsoft USX database schema.
        Use your own discretion to decide which tables and columns are needed.

        Name: RawSysLogs
        Description: The raw sys logs for all servers such as windows, linux, and SQL.
        Columns: [fields: dynamic (Refernce the FieldType), name: string, ['tags']: dynamic (Refernce the TagType), timestamp: datetime]

        Name: FieldType
        Description: Example columns for the field column in RawSysLogs.
        Columns: [max_wait_time_ms: long, resource_wait_ms: long, signal_wait_time_ms: long, wait_time_ms: long, waiting_tasks_count: long, current_size_mb: long, database_id: long, file_id: long, read_bytes: long, read_latency_ms: long, reads: long, rg_read_stall_ms: long, rg_write_stall_ms: long, space_used_mb: long, write_bytes: long, write_latency_ms: long, writes: long, active_workers_count: long, context_switches_count: long, current_tasks_count: long, current_workers_count: long, is_idle: boolean, is_online: boolean, load_factor: long, pending_disk_io_count: long, preemptive_switches_count: long, runnable_tasks_count: long, total_cpu_usage_ms: long, total_scheduler_delay_ms: long, work_queue_count: long, yield_count: long, size_kb: long]

        Name: TagType
        Description: Example columns for the tags column in RawSysLogs.
        Columns: [database_name: string, host: string, measurement_db_type: string, replica_updateability: string, sql_instance: string]

        # Possible Values
        In addition, the user has identified the following values which can appear in these tables.
        Use your own discretion whether to include any of these values and how best to use them.Event
        Do not use any values that are not explicitly mentioned in the prompt or the section below.

        # Kusto Syntax
        A sample of useful KQL syntax are listed below.
        **Scalar Functions**
        | Syntax | Usage |
        | —— | —– |
        | ‘around(value, center, delta)‘ | Check if ‘value‘ is in the range ‘center +/- delta‘ |
        | ‘bin(value, roundto)‘ | Round ‘value‘ to the nearest multiple of ‘roundto‘ |
        | ‘isempty(value)‘ | Check if a column is null or empty |
        | ‘parse_json(string)‘ | Convert a JSON-like string into a ‘dynamic‘ object |
        | ‘todouble(value)‘ | Convert a value to a double |
        | ‘toint(value)‘ | Convert a value to an integer |
        | ‘toscalar(expression)‘ | Convert a single-row, single-column tabular expression into a scalar value |
        | ‘tostring(value)‘ | Convert a value to a string |
        | ‘range ( start, stop[, step ])‘ | Create a dynamic array of equally spaced values |
        **Aggregation Functions**
        | Syntax | Usage |
        | —— | —– |
        | ‘arg_min(minExpr, returnExpr [, …])‘ | Find the row that minimizes an expression |
        | ‘count()‘ | Count the number of records in a table or group |
        | ‘count_distinct(expr)‘ | Count the number of unique values of ‘expr‘ per group |
        | ‘dcount(expr)‘ | Estimate the number of unique values of ‘expr‘ per group |
        | ‘make_set(expr)‘ | Create an array of unique values of ‘expr‘ per group |
        | ‘percentile(expr, percentile)‘ | Estimate the nearest-rank percentile of the population defined by ‘expr‘ |
        **Window Functions**
        | Syntax | Usage |
        | —— | —– |
        | ‘row_number()‘ | Get the index of each row |
        | ‘next(column[, offset, default_value ])‘ | Get the value of an upcoming row |
        | ‘prev(column[, offset, default_value ])‘ | Get the value of a previous row |
        **Tabular Operators**
        | Syntax | Usage |
        | —— | —– |
        | ‘distinct ColumnName[, ColumnName2, …]‘ | Produce a table of unique combinations of the input columns |
        | ‘mv-apply Columns [to typeof(DataType)] on ( SubQuery )‘ | Apply a subquery to each record and union the results |
        | ‘mv-expand [bagexpansion=(bag | array)] [Name =] Expr [to typeof(DataType)]‘ | Explode an array or JSON object into multiple rows |
        | ‘render Visualization‘ | Render a visualization |
        | ‘serialize‘ | Enable window functions |
        **Scalar Operators**
        | Syntax | Usage |
        | —— | —– |
        | ‘contains ( string )‘ | Check if a value contains a substring |
        | ‘has ( string )‘ | Check if a value contains a specific word or term |
        | ‘has_any ( string, … )‘ | Check if a value contains any substring in a set |
        | ‘has_all ( string, … )‘ | Check if a value contains all substrings in a set |
        | ‘in~ ( string, … )‘ | Check if a value equals any substring in a set |
        | ‘in ( number, … )‘ | Check if a value equals any number in a set |
        | ‘matches regex string‘ | Check if a value matches a regex pattern |
        # Kusto Best practices
        **General Guidelines**
        - Never use placeholder values
        - Minimize the number of tables and columns referenced
        - Only include user-provided values in your queries with the correct table and column
        - All backslashes in strings must be properly escaped
        - Columns can be renamed using the ‘=‘ operator
        - Avoid multiple filter conditions in a single ‘where‘ statement
        - Functions that create columns will typically be named like ‘function_‘, ‘function_column‘, etc.
        **Operators**
        - Use ‘has_all‘, ‘has_any‘, ‘in~‘, and ‘in‘ in place of multiple ‘and‘ or ‘or‘ operators
        - Consider using ‘has‘ or ‘contains‘ before parsing JSON columns to avoid expensive parsing of rows without the required keys or values
        - Use case-insensitive operators when comparing string and dynamic columns
        **Aggregations**
        - Aggregation functions only appear in summarize statements
        - When combining aggregations and scalar values, a self-join may be necessary
        - \”arg\” functions do not rename columns
        - Aggregation functions cannot be nested. Use two separate summarize statements instead e.g., ‘| summarize X=AggFunc1(Col1) by Col2 | summarize AggFunc2(X)‘
        **Joining Tables**
        - ‘join‘ conditions **MUST** contain only ‘==‘
        - Columns that appear in multiple tables merged by a ‘join‘ statement have integer suffixes e.g., ColumnName, ColumnName1, etc
        - Consider all kinds of join. Use ’innerunique’ to keep all columns from both tables
        - Use semi-joins (‘leftsemi‘, ‘leftantisemi‘, ‘rightsemi‘, etc.) to only keep columns from one table
        - Use anti-joins (‘leftanti‘, ‘leftantisemi‘, ‘rightanti‘, etc.) to exclude records that appear in a table from another
        **Tips and Tricks**
        - Inequality operators like ‘between‘, ‘<=‘, etc. cannot be used to join tables. To join on a window on datetimes or other high-cardinality columns, use ‘bin‘ and ‘range‘ to define a new key
        ~~~kusto
        Table1
        | extend Key=bin(TimeColumn, WindowSize)
        | join (
        Table2
        | mv-expand Key=range(bin(TimeColumn - WindowSize, WindowSize / 2), bin(TimeColumn, WindowSize / 2), WindowSize / 2) to typeof(datetime)
        ) on Key
        | where TimeColumn1 - TimeColumn between (0m .. WindowSize)
        ~~~
        - To compare an aggregated and unaggregated version of the same column, you will need to use a self-join
        ~~~kusto
        Table
        | summarize VarName=AggFunc(Col1) by Col2
        | join Table on $left.Col2 == $right.Col2
        | where Col1 < VarName
        ~~~
        - Some columns may be blank or dependent on other column values. Use a self-merge to utilize more than one of these columns at a time
        ~~~kusto
        Table
        | where IndependentColumn == Value1
        | where isnotempty(DependentColumn1)
        | join (
        Table
        | where IndependentColumn == Value2
        | where isnotempty(DependentColumn2)
        ) on JoinColumn
        | project JoinColumn, IndependentColumn, DependentColumn1, DependentColumn2
        ~~~
        # Examples

        ## All Events
        RawSysLogs

        ## Where RawSysLogs name contains sql
        RawSysLogs
          | where name contains "sql"

        # Reminder
        All steps must be completed in a single message.
        Remember, your output will contain queries like
        ~~~kusto
        KQL QUERY GOES HERE
        ~~~
    """;

    }
}