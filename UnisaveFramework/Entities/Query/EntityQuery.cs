using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LightJson;
using Unisave.Arango;
using Unisave.Arango.Query;
using Unisave.Contracts;

namespace Unisave.Entities.Query
{
    public class EntityQuery<TEntity> where TEntity : Entity
    {
        /// <summary>
        /// Underlying AQL query
        /// </summary>
        protected AqlQuery Query { get; set; }
        
        /// <summary>
        /// Arango instance to be used for execution
        /// </summary>
        protected IArango Arango { get; }
        
        protected EntityQuery(IArango arango)
        {
            Query = new AqlQuery();
            Arango = arango;
        }

        public static EntityQuery<TEntity> TakeAll(IArango arango)
        {
            var query = new EntityQuery<TEntity>(arango);
            
            query.Query.For("entity").In(
                EntityUtils.CollectionFromType(typeof(TEntity))
            ).Do();
            
            return query;
        }
        
        public static EntityQuery<T> TakeNeighbours<T, TRelation>(
            IArango arango, Entity entity
        ) where T : Entity where TRelation : Entity
        {
            var query = new EntityQuery<T>(arango);
            
            // TODO: implement graph traversal
//            query.Query.For("entity").InTraversal(
//                1, 1, "any", entity,
//                ... + Entity.GetEntityType<TRelation>()
//            );
            
            return query;
        }

        /// <summary>
        /// Filter entities by a predicate expression
        /// </summary>
        public EntityQuery<TEntity> Filter(Expression<Func<TEntity, bool>> e)
        {
            // TODO: add parameter substitution
            // (turn parameter to "entity")
            Query.AddFilterOperation(e.Body);
            return this;
        }

        /// <summary>
        /// Get all entities matching the query as an enumerable
        /// </summary>
        public IEnumerable<TEntity> GetEnumerable()
        {
            // add the return statement, otherwise nothing gets returned
            Query.Return("entity");
            
            // NOTE:
            // What follows is basically just a foreach that casts
            // json values to entity instances.
            //
            // The problem is that we need to check for "missing collection"
            // exception and that's where the clutter is produced.

            IEnumerator<TEntity> enumerator = null;
            
            try
            {
                // === create enumerator ===

                try
                {
                    enumerator = Arango.ExecuteAqlQuery(Query)
                        .Select(d => TurnDocumentToEntity(d))
                        .GetEnumerator();
                }
                catch (ArangoException e) when (e.ErrorNumber == 1203)
                {
                    yield break;
                }

                // === iterate over the enumerator ===
                
                while (true)
                {
                    TEntity current;
                    
                    try
                    {
                        if (!enumerator.MoveNext())
                            break;

                        current = enumerator.Current;
                    }
                    catch (ArangoException e) when (e.ErrorNumber == 1203)
                    {
                        yield break;
                    }

                    yield return current;
                }
            }
            finally
            {
                enumerator?.Dispose();
            }
        }

        private TEntity TurnDocumentToEntity(JsonObject document)
        {
            document["$type"] = EntityUtils.GetEntityStringType(
                typeof(TEntity)
            );
            
            return (TEntity) Entity.FromJson(
                document,
                typeof(TEntity)
            );
        }

        /// <summary>
        /// Get all entities matching the query as a list
        /// </summary>
        public List<TEntity> Get()
        {
            return GetEnumerable().ToList();
        }

        /// <summary>
        /// Get the first entity in the query
        /// </summary>
        public TEntity First()
        {
            // TODO: use proper limit operation
            // Query.AddLimitOperation(...)
            return GetEnumerable().FirstOrDefault();
        }
    }
}