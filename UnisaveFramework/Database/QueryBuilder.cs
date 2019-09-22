﻿using System;
using System.Collections.Generic;
using System.Linq;
using Unisave.Runtime;
using Unisave.Serialization;
using LightJson;
using Unisave.Services;

namespace Unisave.Database
{
    /// <summary>
    /// Builds an entity query by chaining commands
    /// </summary>
    public class QueryBuilder<E> where E : Entity, new()
    {
        /// <summary>
        /// Database on which to call the query once finished
        /// </summary>
        private IDatabase database;

        /// <summary>
        /// Parameters of the query
        /// </summary>
        private EntityQuery query = new EntityQuery();

        public QueryBuilder(IDatabase database = null)
        {
            this.database = database;
        }

        /// <summary>
        /// Creates new default query builder
        /// (used in the facade)
        /// </summary>
        public static QueryBuilder<E> Create()
        {
            return new QueryBuilder<E>(
                ServiceContainer.Default.Resolve<IDatabase>()
            );
        }

        /// <summary>
        /// Returns the built up query
        /// </summary>
        public EntityQuery GetQuery() => query;

        /// <summary>
        /// Execute the query and return the matched entities as an enumerable
        /// </summary>
        public IEnumerable<E> GetEnumerable()
        {
            if (database == null)
                throw new InvalidOperationException(
                    $"Cannot execute the query, because no {nameof(IDatabase)} instance is set."
                );

            return database
                .QueryEntities(Entity.GetEntityType<E>(), query)
                .Select((RawEntity r) => (E)Entity.FromRawEntity(r, typeof(E)));
        }

        /// <summary>
        /// Execute the query and return the matched entities in a list
        /// </summary>
        public List<E> Get() => GetEnumerable().ToList();

        /// <summary>
        /// Execute the query and take only the first metching entity, or null
        /// </summary>
        public E First()
        {
            query.takeFirstFound = true;
            return GetEnumerable().FirstOrDefault();
        }

        /// <summary>
        /// Finds an entity by it's id
        /// No "OfPlayer" or "Where" constraints are applied here.
        /// If entity does not exist, null is returned.
        /// </summary>
        public E Find(string entityId)
        {
            return (E)Entity.FromRawEntity(database.LoadEntity(entityId), typeof(E));
        }

        /// <summary>
        /// Focus only on entities, that are owned by the given player
        /// </summary>
        public QueryBuilder<E> OfPlayer(UnisavePlayer player)
        {
            query.requiredOwners.Add(player);

            return this;
        }

        /// <summary>
        /// Focus only on entities, that are owned by all the given players
        /// </summary>
        public QueryBuilder<E> OfPlayers(IEnumerable<UnisavePlayer> players)
        {
            foreach (UnisavePlayer player in players)
                query.requiredOwners.Add(player);

            return this;
        }

        /// <summary>
        /// Allow the target entities to be owned not only by the specified players,
        /// but also by any other players
        /// </summary>
        public QueryBuilder<E> AndOthers()
        {
            query.requireOwnersExactly = false;
            
            return this;
        }

        ///////////////////
        // Where clauses //
        ///////////////////
        
        /// <summary>
        /// Filters entities by data
        /// </summary>
        /// <param name="jsonPath">Path to the target data item</param>
        /// <param name="value">Desired value of the data item</param>
        public QueryBuilder<E> Where(string jsonPath, object value)
        {
            return Where(jsonPath, "=", value);
        }

        /// <summary>
        /// Filters entities by data
        /// </summary>
        /// <param name="jsonPath">Path to the target data item</param>
        /// <param name="op">Operator used to compare the values</param>
        /// <param name="value">Desired value of the data item</param>
        public QueryBuilder<E> Where(string jsonPath, string op, object value)
        {
            query.whereClauses.Add(
                new JsonObject()
                    .Add("type", "Basic")
                    .Add("path", jsonPath)
                    .Add("operator", op)
                    .Add("value", Serializer.ToJson(value))
                    .Add("boolean", "and")
            );

            return this;
        }
    }
}
