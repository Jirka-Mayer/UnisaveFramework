using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using Unisave.Entities;
using Unisave.Serialization;

namespace Unisave.Modules.Matchmaking
{
    public class BasicMatchmakerEntity : Entity
    {
        public const int CurrentVersion = 1;

        /// <summary>
        /// Entity version
        /// 
        /// When incremented, the entity gets recreated.
        /// This is to allow me to change entity schema
        /// without making migrations since the data inside
        /// the entity is not really that persistent.
        /// </summary>
        public int Version { get; set; } = CurrentVersion;
        
        /// <summary>
        /// What matchmaker this entity belongs to
        /// </summary>
        public string MatchmakerName { get; set; }
        
        /// <summary>
        /// Waiting tickets
        /// </summary>
        public List<JsonObject> Tickets { get; set; }
            = new List<JsonObject>();

        /// <summary>
        /// List of notifications to be sent to players
        /// </summary>
        public List<Notification> Notifications { get; set; }
            = new List<Notification>();

        public class Notification
        {
            // ID of the player (entity) to notify
            public string playerId;
            
            // ID of the matched match entity
            public string matchId;

            // when was the notification created
            public DateTime createdAt = DateTime.UtcNow;
        }

        public List<TMatchmakerTicket> DeserializeTickets<TMatchmakerTicket>()
            where TMatchmakerTicket : BasicMatchmakerTicket
        {
            return Tickets.Select(
                t => Serializer.FromJson<TMatchmakerTicket>(t)
            ).ToList();
        }

        public void SerializeTickets<TMatchmakerTicket>(
            List<TMatchmakerTicket> tickets
        ) where TMatchmakerTicket : BasicMatchmakerTicket
        {
            Tickets = tickets
                .Select(t => Serializer.ToJson(t).AsJsonObject)
                .ToList();
        }
    }
}