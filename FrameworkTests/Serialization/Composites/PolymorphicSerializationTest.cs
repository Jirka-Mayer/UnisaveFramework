using System.Collections.Generic;
using LightJson;
using NUnit.Framework;
using Unisave.Serialization;
using Unisave.Serialization.Context;

namespace FrameworkTests.Serialization.Composites
{
    /*
     * The polymorphism is tested on the concept of
     * player moves in a turn-based game.
     */

    public abstract class PlayerMove
    {
        // name of the player that performed the move
        public string player;
    }

    public class DoNothingMove : PlayerMove
    {
        // empty
    }

    public class PlayCardMove : PlayerMove
    {
        // card index within the player's hand
        public int cardIndex;
    }

    public class MyContainer
    {
        public PlayerMove move;
    }

    [TestFixture]
    public class PolymorphicSerializationTest
    {
        [Test]
        public void EachMoveCanBeSerialized()
        {
            Assert.AreEqual(
                new JsonObject {
                    ["player"] = "John",
                    ["$type"] = typeof(DoNothingMove).FullName
                }.ToString(),
                Serializer.ToJson<PlayerMove>(new DoNothingMove {
                    player = "John"
                }).ToString()
            );
            
            Assert.AreEqual(
                new JsonObject {
                    ["cardIndex"] = 2,
                    ["player"] = "Peter",
                    ["$type"] = typeof(PlayCardMove).FullName
                }.ToString(),
                Serializer.ToJson<PlayerMove>(new PlayCardMove {
                    player = "Peter",
                    cardIndex = 2
                }).ToString()
            );
        }

        [Test]
        public void SerializingSpecificallyDoesNotSerializeType()
        {
            Assert.AreEqual(
                new JsonObject {
                    ["player"] = "John"
                }.ToString(),
                Serializer.ToJson<DoNothingMove>(new DoNothingMove {
                    player = "John"
                }).ToString()
            );
        }

        [Test]
        public void EachMoveCanBeDeserializedSpecifically()
        {
            // DoNothingMove
            {
                var json = new JsonObject {
                    ["$type"] = typeof(DoNothingMove).FullName,
                    ["player"] = "John"
                };
                var value = Serializer.FromJson<DoNothingMove>(json);
                Assert.IsInstanceOf<DoNothingMove>(value);
                Assert.AreEqual("John", value.player);
            }

            // PlayCardMove
            {
                var json = new JsonObject {
                    ["$type"] = typeof(PlayCardMove).FullName,
                    ["player"] = "Peter",
                    ["cardIndex"] = 2
                };
                var value = Serializer.FromJson<PlayCardMove>(json);
                Assert.IsInstanceOf<PlayCardMove>(value);
                Assert.AreEqual("Peter", value.player);
                Assert.AreEqual(2, value.cardIndex);
            }
        }
        
        [Test]
        public void EachMoveCanBeDeserializedPolymorphicly()
        {
            // DoNothingMove
            {
                var json = new JsonObject {
                    ["$type"] = typeof(DoNothingMove).FullName,
                    ["player"] = "John"
                };
                var value = Serializer.FromJson<PlayerMove>(json);
                Assert.IsInstanceOf<DoNothingMove>(value);
                Assert.AreEqual("John", value.player);
            }

            // PlayCardMove
            {
                var json = new JsonObject {
                    ["$type"] = typeof(PlayCardMove).FullName,
                    ["player"] = "Peter",
                    ["cardIndex"] = 2
                };
                var value = Serializer.FromJson<PlayerMove>(json);
                Assert.IsInstanceOf<PlayCardMove>(value);
                Assert.AreEqual("Peter", value.player);
                Assert.AreEqual(2, ((PlayCardMove) value).cardIndex);
            }
        }
        
        [Test]
        public void EachMoveCanBeDeserializedAsObject()
        {
            // allow insecure deserialization to object
            var context = default(DeserializationContext);
            context.suppressInsecureDeserializationException = true;
            
            // DoNothingMove
            {
                var json = new JsonObject {
                    ["player"] = "John",
                    ["$type"] = typeof(DoNothingMove).FullName
                };
                var value = Serializer.FromJson<object>(json, context);
                Assert.IsInstanceOf<DoNothingMove>(value);
                Assert.AreEqual("John", ((PlayerMove) value).player);
            }

            // PlayCardMove
            {
                var json = new JsonObject {
                    ["player"] = "Peter",
                    ["cardIndex"] = 2,
                    ["$type"] = typeof(PlayCardMove).FullName
                };
                var value = Serializer.FromJson<object>(json, context);
                Assert.IsInstanceOf<PlayCardMove>(value);
                Assert.AreEqual("Peter", ((PlayerMove) value).player);
                Assert.AreEqual(2, ((PlayCardMove) value).cardIndex);
            }
        }

        [Test]
        public void PolymorphicListCanBeSerialized()
        {
            var list = new List<PlayerMove> {
                new DoNothingMove {
                    player = "John"
                },
                new PlayCardMove {
                    player = "Peter",
                    cardIndex = 2
                }
            };
            
            var json = new JsonArray {
                new JsonObject {
                    ["player"] = "John",
                    ["$type"] = typeof(DoNothingMove).FullName
                },
                new JsonObject {
                    ["cardIndex"] = 2,
                    ["player"] = "Peter",
                    ["$type"] = typeof(PlayCardMove).FullName
                }
            };
            
            Assert.AreEqual(
                json.ToString(),
                Serializer.ToJson<List<PlayerMove>>(list).ToString()
            );
        }

        [Test]
        public void PolymorphicListCanBeDeserialized()
        {
            var json = new JsonArray {
                new JsonObject {
                    ["player"] = "John",
                    ["$type"] = typeof(DoNothingMove).FullName
                },
                new JsonObject {
                    ["player"] = "Peter",
                    ["cardIndex"] = 2,
                    ["$type"] = typeof(PlayCardMove).FullName
                }
            };

            var list = Serializer.FromJson<List<PlayerMove>>(json);
            
            Assert.IsInstanceOf<DoNothingMove>(list[0]);
            Assert.AreEqual("John", list[0].player);
            
            Assert.IsInstanceOf<PlayCardMove>(list[1]);
            Assert.AreEqual("Peter", list[1].player);
            Assert.AreEqual(2, ((PlayCardMove) list[1]).cardIndex);
        }

        [Test]
        public void PolymorphicContainerCanBeSerialized()
        {
            Assert.AreEqual(
                new JsonObject {
                    ["move"] = new JsonObject {
                        ["player"] = "John",
                        ["$type"] = typeof(DoNothingMove).FullName
                    }
                }.ToString(),
                Serializer.ToJson(new MyContainer {
                    move = new DoNothingMove {
                        player = "John"
                    }
                }).ToString()
            );
        }
        
        [Test]
        public void PolymorphicContainerCanBeDeserialized()
        {
            var json = new JsonObject {
                ["move"] = new JsonObject {
                    ["player"] = "John",
                    ["$type"] = typeof(DoNothingMove).FullName
                }
            };

            var container = Serializer.FromJson<MyContainer>(json);
            
            Assert.IsInstanceOf<DoNothingMove>(container.move);
            Assert.AreEqual("John", container.move.player);
        }
    }
}