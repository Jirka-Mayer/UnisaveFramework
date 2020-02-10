﻿using System.Collections.Generic;
using Unisave.Database;
using Unisave.Database.Query;
using Unisave.Exceptions;

namespace Unisave.Contracts
{
    /// <summary>
    /// Interface used to resolve database connection
    /// from the service container.
    ///
    /// Provides the raw interface to the database. All framework features
    /// should be built on top of this interface. How this interface is
    /// implemented is a Unisave internal thing and you shouldn't rely on it,
    /// because it might change - it's not a guaranteed API.
    /// This interface however is.
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// Saves entity into the database, or if the ID is null,
        /// then the entity gets created. The newly generated ID will
        /// be set into the provided entity instance, together with
        /// the timestamps.
        ///
        /// Any pending ownership operations will be executed.
        /// </summary>
        void SaveEntity(RawEntity entity);

        /// <summary>
        /// Loads entity with given ID from the database.
        /// If the entity does not exist, then null is returned.
        ///
        /// It is not guaranteed that all owners will be loaded,
        /// since there might be too many of them.
        /// </summary>
        /// <param name="id">ID of the entity to be loaded</param>
        /// <param name="lockType">
        /// How to lock the entity:
        /// - null ... no lock, default
        /// - "shared" ... block modification by others
        /// - "for_update" ... block modification and locking reads by others
        /// </param>
        /// <exception cref="DatabaseDeadlockException"></exception>
        RawEntity LoadEntity(string id, string lockType = null);

        /// <summary>
        /// Iterates over all owners of a given entity.
        /// If the entity does not exist, the iterator will be empty.
        /// </summary>
        /// <param name="entityId">ID of the entity to query</param>
        IEnumerable<string> GetEntityOwners(string entityId);

        /// <summary>
        /// Tests whether a player is an owner of a given entity.
        /// If the entity or the player does not exist, false is returned.
        /// </summary>
        bool IsEntityOwner(string entityId, string playerId);

        /// <summary>
        /// Deletes an entity by it's ID
        /// </summary>
        /// <param name="id">ID of the entity</param>
        /// <returns>
        /// True if the entity was found and deleted,
        /// false if it didn't even exist.
        /// </returns>
        bool DeleteEntity(string id);

        /// <summary>
        /// Get entities of a given type, satisfying the provided query
        /// </summary>
        IEnumerable<RawEntity> QueryEntities(EntityQuery query);

        /// <summary>
        /// Starts a new database transaction
        /// </summary>
        void StartTransaction();

        /// <summary>
        /// Rolls back the top level transaction
        /// Does nothing if no transaction open
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Commits the top level transaction
        /// Does nothing if no transaction open
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Returns the number of nested transactions that are open
        /// </summary>
        int TransactionLevel();
    }
}