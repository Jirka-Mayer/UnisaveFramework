﻿using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;

namespace Unisave.Database
{
    /// <summary>
    /// Describes a specific subset of entities of a given type
    /// </summary>
    public class EntityQuery
    {
        /// <summary>
        /// What owners do we require the entity to have
        /// </summary>
        public ISet<UnisavePlayer> requiredOwners = new HashSet<UnisavePlayer>();

        /// <summary>
        /// Is the set of required owners exact, or can it be just a subset of the actual owners?
        /// </summary>
        public bool requireOwnersExactly = true;

        /// <summary>
        /// Take the first found entity and return only that
        /// </summary>
        public bool takeFirstFound = false;

        /// <summary>
        /// Serialize query to json
        /// </summary>
        public JsonObject ToJson()
        {
            return new JsonObject()
                .Add(nameof(requiredOwners), new JsonArray(requiredOwners.Select(x => (JsonValue)x.Id).ToArray()))
                .Add(nameof(requireOwnersExactly), requireOwnersExactly)
                .Add(nameof(takeFirstFound), takeFirstFound);
        }

        /// <summary>
        /// Deserialize query from json
        /// </summary>
        public static EntityQuery FromJson(JsonObject json)
        {
            return new EntityQuery {
                requiredOwners = new HashSet<UnisavePlayer>(
                    json[nameof(requiredOwners)].AsJsonArray.Select(x => new UnisavePlayer(x.AsString))
                ),
                requireOwnersExactly = json[nameof(requireOwnersExactly)].AsBoolean,
                takeFirstFound = json[nameof(takeFirstFound)].AsBoolean
            };
        }
    }
}
