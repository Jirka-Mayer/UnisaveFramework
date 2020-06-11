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
            var substitutor = new ExpressionParameterSubstitutor(
                e.Parameters[0].Name,
                "entity"
            );

            e = substitutor.VisitAndConvert(e, nameof(Filter));
            
            Query.AddFilterOperation(e.Body);
            
            return this;
        }

        /// <summary>
        /// Get all entities matching the query as a list
        /// </summary>
        public List<TEntity> Get()
        {
            // add the return statement, otherwise nothing gets returned
            Query.Return("entity");

            try
            {
                return Arango.ExecuteAqlQuery(Query)
                    .Select(d => TurnDocumentToEntity(d))
                    .ToList();
            }
            catch (ArangoException e) when (e.ErrorNumber == 1203)
            {
                // collection does not exist
                return new List<TEntity>();
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
        /// Get the first entity in the query
        /// </summary>
        public TEntity First()
        {
            // TODO: use proper limit operation
            // Query.AddLimitOperation(...)

            var entities = Get();
            
            if (entities.Count == 0)
                return null;

            return entities[0];
        }
    }
}