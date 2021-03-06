using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using Unisave.Arango.Query;

namespace Unisave.Arango.Execution
{
    /// <summary>
    /// Executes queries
    /// </summary>
    public class QueryExecutor
    {
        /// <summary>
        /// Repository containing implementations of AQL functions
        /// </summary>
        public AqlFunctionRepository FunctionRepository { get; }

        /// <summary>
        /// Source of data for the execution
        /// </summary>
        public IExecutionDataSource DataSource { get; }
        
        public QueryExecutor(
            IExecutionDataSource dataSource,
            AqlFunctionRepository functionRepository
        )
        {
            DataSource = dataSource;
            FunctionRepository = functionRepository;
        }
        
        /// <summary>
        /// Executes an AQL query
        /// </summary>
        public List<JsonValue> Execute(AqlQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            
            query.ValidateQuery();
            
            var initialFrame = new ExecutionFrame();

            var frameStream = Enumerable.Repeat(initialFrame, 1);
            
            foreach (AqlOperation operation in query)
            {
                switch (operation)
                {
                    case AqlReturnOperation op:
                        return op
                            .ApplyToFrameStream(this, frameStream)
                            .ToList(); // make sure the query is executed
                                        // even if nobody looks at the results
                    
                    case AqlForOperation op:
                        frameStream = op.ApplyToFrameStream(this, frameStream);
                        break;
                    
                    case AqlInsertOperation op:
                        frameStream = op.ApplyToFrameStream(this, frameStream);
                        break;
                    
                    case AqlFilterOperation op:
                        frameStream = op.ApplyToFrameStream(this, frameStream);
                        break;
                    
                    case AqlReplaceOperation op:
                        frameStream = op.ApplyToFrameStream(this, frameStream);
                        break;
                    
                    case AqlRemoveOperation op:
                        frameStream = op.ApplyToFrameStream(this, frameStream);
                        break;
                    
                    default:
                        throw new ArgumentException(
                            "Unknown operation type " + operation
                        );
                }
            }
            
            // there was no return statement, so we return an empty collection
            // BUT !!! we still need to enumerate over it, otherwise nothing
            // happens (inserts, updates, deletes, ...)

            foreach (var _ in frameStream)
            { /* do nothing (let the pipeline work) */ }
            
            return new List<JsonValue>();
        }

        /// <summary>
        /// Executes the query and returns the result as a JSON array
        /// </summary>
        public JsonArray ExecuteToArray(AqlQuery query)
        {
            var array = new JsonArray();

            foreach (JsonValue item in Execute(query))
                array.Add(item);

            return array;
        }
    }
}